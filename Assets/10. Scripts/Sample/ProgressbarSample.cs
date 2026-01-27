using System;
using KooHoo;
using UnityEngine;
using UnityEngine.UI;

public class ProgressbarSample : MonoBehaviour
{
    public class PlayerStat
    {
        public ObservableValue<int> Hp;
        public ObservableValue<int> Stamina;
    }
        
    
    public Image Progressbar;
    public PlayerStat playerStat = new PlayerStat();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStat.Hp = new ObservableValue<int>();
        playerStat.Stamina = new ObservableValue<int>();


        playerStat.Hp.OnValueChanged += UpdateHpProgressBar;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playerStat.Hp.Value -= 10;
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            playerStat.Hp.Value += 10;
        }
    }

    private void UpdateHpProgressBar(int prev, int current)
    {
        
        Progressbar.fillAmount = (float)current / 100.0f;
    }
}
