using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LobbySaveSlotView : MonoBehaviour
{
    /*
    Save 슬롯 프리팹용 View 
    - titleText : "SAVE 1" 
    - metaText  : "2026-01-26 04:12\nStage 12" 또는 "New Game" 
    // - Modern UI Pack 버튼 이벤트가 프리팹 내부에 있어도, onClickUnityEvent를 통해 LobbyController가 안전하게 콜백을 주입할 수 있습니다.
     */

    [Header("Texts")]
    [SerializeField] private TMP_Text[] titleText;
    [SerializeField] private TMP_Text[] metaText;
    
    [Header("Click Event (Insurance)")]
    public UnityEvent onClickUnityEvent = new UnityEvent();

    public int SlotIndex { get; private set; }

    public void SetSlotIndex(int index)
    {
        SlotIndex = index;
    }

    public void SetText(string title, string meta, bool hasData)
    {
        if (titleText != null)
        {
            foreach(var t in titleText)
                if (t != null)
                    t.text = title;
        }

        bool showMeta = string.IsNullOrEmpty(meta) == false;
        
        if (metaText != null)
        {
            foreach (var m in metaText)
            {
                if (m == null)
                    continue;

                m.text = meta;
                m.gameObject.SetActive(showMeta);
            }
        }
    }

    public void SetOnClick(Action callback)
    {
        onClickUnityEvent.RemoveAllListeners();
        if (callback != null)
            onClickUnityEvent.AddListener(() => callback.Invoke());
    }
    
    public void InvokeClick()
    {
        onClickUnityEvent?.Invoke();
    }  
}