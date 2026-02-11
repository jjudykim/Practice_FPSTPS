using System;
using UnityEngine;

public class PlayerDamageReceiver : MonoBehaviour, IDamageable
{
    private static readonly int DEAD = Animator.StringToHash("Dead");

    [Header("Refs")] 
    [SerializeField] private Player player;
    [SerializeField] private Animator animator;
    
    [Header("On Death - Disable Components")]
    [SerializeField] private Behaviour[] disableOnDeath;
    
    [Header("On Death - Disable Colliders")]
    [Tooltip("죽은 뒤 추가 피격/로그를 막고 싶으면 true.")]
    [SerializeField] private bool disableCollidersOnDeath = true;

    private Collider[] cachedColliders;
    private bool deathHandled = false;

    public bool IsAlive => player != null && player.IsDead == false;
    
    private void Reset()
    {
        AutoResolve();
    }

    private void Awake()
    {
        AutoResolve();
        
        if (cachedColliders == null || cachedColliders.Length == 0)
            cachedColliders = GetComponentsInChildren<Collider>(true);
    }

    private void OnEnable()
    {
        if (player != null)
            player.Stats.Resources.OnDead += HandleDeath;
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.Stats.Resources.OnDead -= HandleDeath;
        }
    }

    public void ApplyDamage(DamageInfo info)
    {
        if (player == null)
        {
            Debug.LogWarning("[PlayerDamageReceiver] player is null.");
            return;
        }

        if (player.IsDead)
            return;

        if (player.IsRolling)
            return;
        
        float raw = info.Damage;
        
        int dmg = Mathf.CeilToInt(Mathf.Max(0f, raw));
        if (dmg <= 0)
            return;
        
        player.ApplyDamage(dmg);

        Debug.Log($"[PlayerDamageReceiver] Hit! source={info.Source}, damage={dmg}, hp={player.Stats.Resources.CurHp.Value}/{player.Stats.MaxHp}"); 
    }
    
    private void HandleDeath()
    {
        if (deathHandled)
            return;

        deathHandled = true;

        Debug.Log("[PlayerDamageReceiver] Player Dead! Disabling controllers...");

        // 1) Player 상태 플래그 정리
        player.IsRolling = false;
        player.IsRunning = false;
        player.IsAiming = false;
        player.IsReloading = false;
        
        animator.SetTrigger(DEAD);

        // 2) 조작/전투/시야 컨트롤러 비활성화
        if (disableOnDeath != null)
        {
            for (int i = 0; i < disableOnDeath.Length; i++)
            {
                if (disableOnDeath[i] != null)
                    disableOnDeath[i].enabled = false;
            }
        }

        // 3) 콜라이더 끄기(죽은 뒤 추가 피격/연타 로그 방지)
        if (disableCollidersOnDeath && cachedColliders != null)
        {
            for (int i = 0; i < cachedColliders.Length; i++)
            {
                if (cachedColliders[i] != null)
                    cachedColliders[i].enabled = false;
            }
        }

        Managers.Instance.Game.GameOver();
    }

    public void Resurrect()
    {
        deathHandled = false;
      
      // 비활성화했던 컴포넌트들 다시 활성화
      if (disableOnDeath != null)
      {
          foreach(var behaviour in disableOnDeath)
          {
              if (behaviour != null && !(behaviour is PlayerCombatController))
              {
                  behaviour.enabled = true;
              }
          }
      }

      //콜라이더 다시 활성화
      if (cachedColliders != null)
      {
          foreach(var col in cachedColliders)
          {
              if (col != null)
                  col.enabled = true;
          }
      }
    }

    private void AutoResolve()
    {
        if (player == null)
        {
            player = GetComponentInParent<Player>();
            if (player == null && Player.Instance != null)
                player = Player.Instance;
        }

        var move = GetComponentInChildren<PlayerMoveController>(true);
        var look = GetComponentInChildren<PlayerLookController>(true);
        var combat = GetComponentInChildren<PlayerCombatController>(true);
        
        int count = 0;
        if (move != null) count++;
        if (look != null) count++;
        if (combat != null) count++;

        if (count == 0)
        {
            disableOnDeath = new Behaviour[0];
            return;
        }

        disableOnDeath = new Behaviour[count];
        int idx = 0;

        if (move != null) disableOnDeath[idx++] = move;
        if (look != null) disableOnDeath[idx++] = look;
        if (combat != null) disableOnDeath[idx++] = combat;

        if (cachedColliders == null || cachedColliders.Length == 0)
            cachedColliders = GetComponentsInChildren<Collider>(true);
    }
}