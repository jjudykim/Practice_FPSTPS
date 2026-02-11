using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TMP_Text textMesh;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float duration = 1f;

    public void Play(string message, Color color)
    {
        textMesh.SetText(message);
        textMesh.color = color;
        Managers.Instance.StartCoroutine(CoPlay());
    }

    private IEnumerator CoPlay()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward
                        , Camera.main.transform.rotation * Vector3.up);
        
        float timer = 0f;
        Vector3 startPos = transform.position;
        Color startColor = textMesh.color;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
        
            // 1. 위로 떠오르기
            transform.position = startPos + Vector3.up * (moveSpeed * progress);
        
            // 2. 알파값 조절 (사라지기)
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, progress);
            textMesh.color = c;
        
            yield return null;
        }
        
        // 오브젝트 풀로 반환 (또는 파괴)
        gameObject.SetActive(false);
        Managers.Instance.FloatingText.ReturnToPool(this);
    }
}
