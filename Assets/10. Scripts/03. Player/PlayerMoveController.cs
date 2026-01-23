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
    
    [Header("Refs")] 
    [SerializeField] private Animator animator;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private PlayerLookController lookController;
    
    [Header("Move Settings")] 
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 6.0f;

    [Header("Roll Settings (DOTWeen")] 
    [SerializeField] private float rollDistance = 3.0f;
    [SerializeField] private float rollDuration = 0.8f;
    [SerializeField] private Ease rollEase = Ease.OutQuad;
    
    private InputManager input;
    private Camera cam;

    private Tween rollTween;
    private bool isRolling;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        
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
    }

    private void TickMove()
    {
        // Move & Run
        Vector3 moveDir = GetMoveDirection();
        float speed = input.Run ? runSpeed : walkSpeed;
        
        transform.position += moveDir * (speed * Time.deltaTime);

        animator.SetFloat(INPUT_X, input.MoveX);
        animator.SetFloat(INPUT_Y, input.MoveY);
        animator.SetFloat(SPEED, speed);
    }

    private Vector3 GetMoveDirection()
    {
        Vector3 moveInput = new Vector3(input.MoveX, 0f, input.MoveY);
        
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        Vector3 moveDir;

        if (cameraController.Mode == CameraController.CameraMode.QuarterView)
        {
            Transform camTransform = cam.transform;
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDir = (camRight * moveInput.x) + (camForward * moveInput.z);
        }
        else
        {
            moveDir = (transform.right * moveInput.x) + (transform.forward * moveInput.z);    
        }

        return moveDir;
    }

    private void StartRoll()
    {
        isRolling = true;

        if (rollTween != null && rollTween.IsActive())
            rollTween.Kill();

        ApplyRollDirectionParams();

        Vector3 dir = GetRollWorldDirection();
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

    private void ApplyRollDirectionParams()
    {
        if (animator == null)
            return;

        if (lookController == null)
            return;

        Vector2 rollLocalDir = lookController.LookLocalDir;
    }

    private Vector3 GetRollWorldDirection()
    {
        if (cameraController != null && cameraController.Mode == CameraController.CameraMode.QuarterView)
        {
            if (lookController != null)
            {
                Vector3 d = lookController.LookWorldDirFlat;
                d.y = 0f;

                if (d.sqrMagnitude > 0.0001f)
                    return d.normalized;
            }
        }
        
        Vector3 dir = transform.forward;
        dir.y = 0f;
        return dir;
    }
}
