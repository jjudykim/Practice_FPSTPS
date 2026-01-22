using System;
using UnityEngine;

public class InputManager
{
    // ==========================
    // Key Control
    // ==========================
    // Move (지속 입력)
    public float MoveX { get; private set; }
    public float MoveY { get; private set; }
    
    // Run (지속 입력)
    public bool Run { get; private set; }
    
    // Roll (원샷 입력)
    public bool Roll { get; private set; }
    
    // Reload (원샷 입력)
    public bool Reload { get; private set; }

    // View Change (원샷 입력)
    public bool ViewChange { get; private set; }
    
    // Map Toggle (원샷 입력)
    public bool MiniMap { get; private set; }
    
    // Inventory Toggle (원샷 입력)
    public bool Inventory { get; private set; }
    
    // QuickSlot (원샷 입력)
    public bool QuickSlot1 { get; private set; }
    public bool QuickSlot2 { get; private set; }
    public bool QuickSlot3 { get; private set; }
    public bool QuickSlot4 { get; private set; }
    public bool QuickSlot5 { get; private set; }

    
    // ==========================
    // Mouse Control
    // ==========================
    
    // POV (마우스 축 회전 입력)
    public float POVX { get; private set; }
    public float POVY { get; private set; }
    
    // Shooting
    [field: SerializeField] public bool Shoot { get; private set; }
    
    // Shooting
    [field: SerializeField] public bool Aiming { get; private set; }

    public bool GamePlayInputEnable { get; set; } = true;  // 이동 / 공격 허용
    public bool UIInputEnabled { get; set; } = true;

    public void Update()
    {
        ClearFrameInputs();
        
        if (GamePlayInputEnable)
        {
            MoveX = Input.GetAxis("Horizontal");
            MoveY = Input.GetAxis("Vertical");
            
            Run = Input.GetKey(KeyCode.LeftShift);
            Roll = Input.GetKeyDown(KeyCode.Space);
            Reload = Input.GetKeyDown(KeyCode.R);
            Shoot = Input.GetMouseButton(0);
            Aiming = Input.GetMouseButton(1);
            
            POVX = Input.GetAxisRaw("Mouse X");
            POVY = Input.GetAxisRaw("Mouse Y");
            ViewChange = Input.GetKeyDown(KeyCode.V);
            
            QuickSlot1 = Input.GetKeyDown(KeyCode.Alpha1);
            QuickSlot2 = Input.GetKeyDown(KeyCode.Alpha2);
            QuickSlot3 = Input.GetKeyDown(KeyCode.Alpha3);
            QuickSlot4 = Input.GetKeyDown(KeyCode.Alpha4);
            QuickSlot5 = Input.GetKeyDown(KeyCode.Alpha5);
        }
        else
        {
            MoveX = 0f;
            MoveY = 0f;
            Run = false;
            Shoot = false;
            Aiming = false;
            POVX = 0f;
            POVY = 0f;
        }
        
        // UI Inputs
        if (UIInputEnabled)
        {
            MiniMap = Input.GetKeyDown(KeyCode.M);
            Inventory = Input.GetKeyDown(KeyCode.I);
        }
    }

    private void ClearFrameInputs()
    {
        Roll = false;
        Reload = false;

        ViewChange = false;
        MiniMap = false;
        Inventory = false;

        QuickSlot1 = false;
        QuickSlot2 = false;
        QuickSlot3 = false;
        QuickSlot4 = false;
        QuickSlot5 = false;
    }
}