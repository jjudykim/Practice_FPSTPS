using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMoveController : MonoBehaviour
{
    private static readonly int INPUT_X = Animator.StringToHash("InputX");
    private static readonly int INPUT_Y = Animator.StringToHash("InputY");
    private static readonly int SPEED = Animator.StringToHash("Speed");
    
    private static readonly int TRIGGER_ROLL = Animator.StringToHash("Roll");

    private static readonly int IS_WEAPON_EQUIPPED = Animator.StringToHash("IsWeaponEquipped");

    [Header("Refs")] 
    [SerializeField] private Animator animator;
    [SerializeField] private CameraController cameraController;
    
    [Header("Move Settings")] 
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 6.0f;

    [Header("Roll Settings (DOTWeen")] 
    [SerializeField] private float rollDistance = 3.0f;
    [SerializeField] private float rollDuration = 0.8f;
    [SerializeField] private Ease rollEase = Ease.OutQuad;

    //[Header("Weapon Settings")]
    //[SerializeField] private GameObject curEquippedWeapon;

    private InputManager input;
    private Camera cam;

    private Tween rollTween;
    private bool isRolling;

    public bool isWeaponEquipped;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        //isWeaponEquipped = false;

        input = Managers.Instance.Input;
        cam = Camera.main;
        
        if (cameraController == null)
            cameraController = Camera.main.GetComponent<CameraController>();
    }

    void Update()
    {
        if (input.Roll)
        {
            if (isRolling == false)
            {
                StartRoll();
                return;
            }
        }

        if (isRolling)
        {
            animator.SetFloat(INPUT_X, 0f);
            animator.SetFloat(INPUT_Y, 0f);
            animator.SetFloat(SPEED, 0f);
            return;
        }

        TickMove();

        if (input.Reload)
        {
            Debug.Log("[Player] Reload Trigger!");
        }


        //if (Input.GetKeyDown(KeyCode.K))
        //    isWeaponEquipped = !isWeaponEquipped;
        //
        //if (isWeaponEquipped)
        //{
        //    curEquippedWeapon.SetActive(true);
        //    animator.SetBool(IS_WEAPON_EQUIPPED, true);
        //}
        //else
        //{
        //    curEquippedWeapon.SetActive(false);
        //    animator.SetBool(IS_WEAPON_EQUIPPED, false);
        //}
    }

    private void TickMove()
    {
        // Move & Run
        Vector3 moveInput = new Vector3(input.MoveX, 0f, input.MoveY);

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        float speed = input.Run ? runSpeed : walkSpeed;

        Vector3 moveWorld;

        if (cameraController.Mode == CameraController.CameraMode.QuarterView)
        {
            Transform camTransform = cam.transform;
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            moveWorld = (camRight * moveInput.x) + (camForward * moveInput.z);
        }
        else
            moveWorld = (transform.right * moveInput.x) + (transform.forward * moveInput.z);

        transform.position += moveWorld * (speed * Time.deltaTime);

        animator.SetFloat(INPUT_X, input.MoveX);
        animator.SetFloat(INPUT_Y, input.MoveY);
        animator.SetFloat(SPEED, speed);
    }

    private void StartRoll()
    {
        isRolling = true;

        if (rollTween != null && rollTween.IsActive())
            rollTween.Kill();

        Vector3 dir = transform.forward;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.forward;
        
        dir.Normalize();
        
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + dir * rollDistance;
        
        animator.SetTrigger(TRIGGER_ROLL);
        
        rollTween = transform.DOMove(targetPos, rollDuration)
                             .SetEase(rollEase)
                             .SetUpdate(UpdateType.Normal)
                             .OnComplete(() =>
                             {
                                 isRolling = false;
                                 rollTween = null;
                             });
    }
}
