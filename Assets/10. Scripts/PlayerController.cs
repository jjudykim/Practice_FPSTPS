using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static readonly int INPUT_X = Animator.StringToHash("InputX");
    private static readonly int INPUT_Y = Animator.StringToHash("InputY");
    private static readonly int IS_WEAPON_EQUIPPED = Animator.StringToHash("IsWeaponEquipped");

    [SerializeField] private float speed = 3.0f;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject curEquippedWeapon;

    public bool isWeaponEquipped;

    void Awake()
    {
        if(animator == null)
            animator = GetComponentInChildren<Animator>();

        isWeaponEquipped = false;
    }
    
    void Update()
    {
        Vector2 inputAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        transform.Translate(new Vector3(inputAxis.x, 0, inputAxis.y) * (speed * Time.deltaTime));
        
        animator.SetFloat(INPUT_X, inputAxis.x);
        animator.SetFloat(INPUT_Y, inputAxis.y);

        if (Input.GetKeyDown(KeyCode.Space))
            isWeaponEquipped = !isWeaponEquipped;
        
        if (isWeaponEquipped)
        {
            curEquippedWeapon.SetActive(true);
            animator.SetBool(IS_WEAPON_EQUIPPED, true);
        }
        else
        {
            curEquippedWeapon.SetActive(false);
            animator.SetBool(IS_WEAPON_EQUIPPED, false);
        }
    }
}
