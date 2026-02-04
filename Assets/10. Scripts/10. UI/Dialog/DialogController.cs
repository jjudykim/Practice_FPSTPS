using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogController : MonoBehaviour
{
    // ===========================
    // TSV 데이터 모델
    // ===========================
    [Serializable]
    public class DialogNodeRow
    {
        public string Id { get; set; }        // 노드 고유 ID
        public string Speaker { get; set; }   // 화자(대부분 1명이라면 고정값 가능)
        public string Text { get; set; }      // 대사 본문
        public string NextId { get; set; }    // 자동 진행할 다음 노드 ID(없으면 선택지 노드일 수 있음)
    }

    [Serializable]
    public class DialogChoiceRow
    {
        public string FromId { get; set; }     // 이 노드에서 선택지를 띄움
        public string ChoiceText { get; set; } // 버튼에 표시할 문구
        public string NextId { get; set; }     // 선택 시 이동할 노드 ID
    }

    // ===========================
    // 인스펙터 연결
    // ===========================
    [Header("UI")]
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text contentText;

    [Header("Choice UI")]
    [SerializeField] private Transform choiceRoot;         // 선택지 버튼들이 생성될 부모 Transform
    [SerializeField] private GameObject choiceButtonPrefab; // Button + TMP_Text 포함 프리팹

    [Header("Typing")]
    [SerializeField] private float typingInterval = 0.03f;
    [SerializeField] private bool skipTypingOnClick = true;

    [Header("Data")]
    [SerializeField] private string nodesFileName = "DialogNodes.tsv";
    [SerializeField] private string choicesFileName = "DialogChoices.tsv";

    [Header("Auto Close")] 
    [SerializeField] private bool autoCloseOnFinish = true;
    [SerializeField] private float autoCloseDelay = 0.2f;

    [Header("Start Node")]
    [SerializeField] private string startNodeId = "START";

    // ===========================
    // 런타임 캐시
    // ===========================
    private Dictionary<string, DialogNodeRow> nodeById;
    private Dictionary<string, List<DialogChoiceRow>> choicesByFromId;

    private Coroutine playCoroutine;
    private bool isTyping;
    private bool requestSkipTyping;

    private void Awake()
    {
        LoadAllData();
        ClearChoices();
    }

    private void Update()
    {
        if (skipTypingOnClick && Input.GetMouseButtonDown(0))
        {
            requestSkipTyping = true;
        }

        // (선택) ESC로 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSelf();
        }
    }

    // ===========================
    // 외부에서 호출하는 시작 함수
    // ===========================
    public void StartDialog(string nodeId)
    {
        if (playCoroutine != null)
            StopCoroutine(playCoroutine);

        ClearChoices();

        playCoroutine = StartCoroutine(CoPlay(nodeId));
    }

    // ===========================
    // 데이터 로딩
    // ===========================
    private void LoadAllData()
    {
        // Nodes
        string nodesPath = Path.Combine(Application.persistentDataPath + "/Table/", nodesFileName);
        List<DialogNodeRow> nodes = TSVReader.ReadTable<DialogNodeRow>(nodesPath);

        nodeById = new Dictionary<string, DialogNodeRow>(StringComparer.Ordinal);
        for (int i = 0; i < nodes.Count; i++)
        {
            DialogNodeRow row = nodes[i];

            if (string.IsNullOrWhiteSpace(row.Id))
            {
                Debug.LogWarning($"[DialogController] Node row has empty Id at index={i}");
                continue;
            }

            if (nodeById.ContainsKey(row.Id))
            {
                Debug.LogWarning($"[DialogController] Duplicate Node Id detected: {row.Id} (index={i})");
                continue;
            }

            nodeById.Add(row.Id, row);
        }

        // Choices
        string choicesPath = Path.Combine(Application.persistentDataPath + "/Table/", choicesFileName);
        List<DialogChoiceRow> choices = TSVReader.ReadTable<DialogChoiceRow>(choicesPath);

        choicesByFromId = new Dictionary<string, List<DialogChoiceRow>>(StringComparer.Ordinal);
        for (int i = 0; i < choices.Count; i++)
        {
            DialogChoiceRow c = choices[i];
            if (string.IsNullOrWhiteSpace(c.FromId))
                continue;

            if (choicesByFromId.TryGetValue(c.FromId, out List<DialogChoiceRow> list) == false)
            {
                list = new List<DialogChoiceRow>();
                choicesByFromId.Add(c.FromId, list);
            }

            list.Add(c);
        }

        Debug.Log($"[DialogController] Loaded Nodes={nodeById.Count}, ChoicesGroups={choicesByFromId.Count}");
    }

    // ===========================
    // 메인 재생 코루틴
    // ===========================
    private IEnumerator CoPlay(string startId)
    {
        string currentId = startId;

        while (string.IsNullOrWhiteSpace(currentId) == false)
        {
            if (nodeById.TryGetValue(currentId, out DialogNodeRow node) == null)
            {
                Debug.LogError($"[DialogController] Node not found: {currentId}");
                yield break;
            }

            // 1) UI 반영
            if (speakerText != null)
                speakerText.SetText(node.Speaker);

            // 2) 대사 타자 출력
            yield return StartCoroutine(CoTypeText(node.Text));

            // 3) 선택지 노드인지 검사
            if (choicesByFromId.TryGetValue(currentId, out List<DialogChoiceRow> choiceList) 
                && choiceList != null 
                && choiceList.Count > 0)
            {
                // 선택지 띄우고, 선택될 때까지 대기
                yield return StartCoroutine(CoWaitChoice(choiceList, nextId =>
                {
                    currentId = nextId;
                }));

                // 선택 후 다음 루프로 진행
                continue;
            }

            // 4) 자동 NextId가 있으면 진행
            if (!string.IsNullOrWhiteSpace(node.NextId))
            {
                currentId = node.NextId;
                continue;
            }

            // 5) NextId도 없고 선택지도 없으면 종료
            break;
        }

        if (autoCloseOnFinish)
        {
            if (autoCloseDelay > 0f)
                yield return new WaitForSeconds(autoCloseDelay);

            CloseSelf();
        }
        
        Debug.Log("[DialogController] Dialog finished. (autoCloseOff)");
    }

    // ===========================
    // 타자 출력
    // ===========================
    private IEnumerator CoTypeText(string text)
    {
        if (contentText == null)
            yield break;

        requestSkipTyping = false;
        isTyping = true;

        contentText.SetText(text);
        contentText.maxVisibleCharacters = 0;

        int length = string.IsNullOrEmpty(text) ? 0 : text.Length;

        for (int i = 0; i < length; i++)
        {
            if (skipTypingOnClick && requestSkipTyping)
            {
                // 즉시 전체 출력
                contentText.maxVisibleCharacters = length;
                break;
            }

            contentText.maxVisibleCharacters += 1;
            yield return new WaitForSeconds(typingInterval);
        }

        isTyping = false;
        requestSkipTyping = false;
    }

    // ===========================
    // 선택지 UI 생성 및 입력 대기
    // ===========================
    private IEnumerator CoWaitChoice(List<DialogChoiceRow> choiceList, Action<string> onPicked)
    {
        ClearChoices();

        bool picked = false;
        string pickedNextId = null;

        // 버튼 생성
        for (int i = 0; i < choiceList.Count; i++)
        {
            DialogChoiceRow c = choiceList[i];
            GameObject go = Instantiate(choiceButtonPrefab, choiceRoot);

            Button btn = go.GetComponent<Button>();
            TMP_Text label = go.GetComponentInChildren<TMP_Text>();

            if (label != null)
                label.SetText(c.ChoiceText);

            int capturedIndex = i; // 클로저 캡쳐 안전
            btn.onClick.AddListener(() =>
            {
                // 타자 출력 중 클릭했을 때는 "스킵"으로만 처리하고 싶다면 여기서 분기 가능
                pickedNextId = choiceList[capturedIndex].NextId;
                picked = true;
            });
        }

        // 선택될 때까지 대기
        while (!picked)
            yield return null;

        ClearChoices();

        // 다음 노드 지정
        onPicked?.Invoke(pickedNextId);
    }

    // ===========================
    // 선택지 버튼 제거
    // ===========================
    private void ClearChoices()
    {
        if (choiceRoot == null)
            return;

        for (int i = choiceRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(choiceRoot.GetChild(i).gameObject);
        }
    }
    
    private void CloseSelf()
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.Close();
            return;
        }
        
        Destroy(gameObject);
    }
}
