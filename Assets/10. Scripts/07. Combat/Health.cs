using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int curHp = 100;

    public int MaxHp => maxHp;
    public int CurHp => curHp;

    public bool IsAlive => curHp > 0;

    public void SetMaxHp(int value, bool alsoFill)
    {
        maxHp = Mathf.Max(1, value);
        if (alsoFill)
            curHp = maxHp;
        else
            curHp = Mathf.Clamp(curHp, 0, maxHp);
    }

    public void Heal(int amount)
    {
        if (IsAlive == false)
            return;

        curHp = Mathf.Clamp(curHp + Mathf.Max(0, amount), 0, maxHp);
    }

    public void Damage(float amount)
    {
        if (IsAlive == false)
            return;

        
        curHp -= Mathf.Max(0, (int)amount);
        if (curHp < 0) curHp = 0;
    }
}