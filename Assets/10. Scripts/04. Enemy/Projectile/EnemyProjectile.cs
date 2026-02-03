using System;
using UnityEngine;

using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Spin (Local X")]
    [SerializeField] private bool enableSpin = true;     
    [SerializeField] private float spinDegPerSec = 720f;
    
    [Header("Particles (Optional)")]
    [SerializeField] private bool playChildParticlesOnInit = true;
    
    [Header("Runtime")]
    private GameObject owner;         // 발사자
    private int damage;
    private float speed;
    private float lifeTime;
    private Vector3 dir;

    private float timer;
    
    private ParticleSystem[] childParticles;

    private void Awake()
    {
        childParticles = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void Init(GameObject owner, Vector3 direction, int damage, float speed, float lifeTime)
    {
        this.owner = owner;
        this.dir = direction.normalized;
        this.damage = damage;
        this.speed = speed;
        this.lifeTime = lifeTime;
        this.timer = 0f;
        
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir);
        
        if (playChildParticlesOnInit && childParticles != null)
        {
            for (int i = 0; i < childParticles.Length; i++)
            {
                ParticleSystem ps = childParticles[i];
                if (ps == null) continue;
                
                if (ps.isPlaying == false)
                    ps.Play(true);
            }
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        transform.position += dir * (speed * dt);

        if (enableSpin && spinDegPerSec != 0f)
        {
            transform.Rotate(Vector3.right, spinDegPerSec * dt, Space.Self);
        }
        
        timer += dt;
        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;
        
        if (owner != null)
        {
            if (other.gameObject == owner)
                return;

            Transform root = other.transform.root;
            if (root != null && root.gameObject == owner)
                return;
        }
        
        if (Managers.Instance != null && Managers.Instance.Combat != null)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitNormal = -dir;

            Managers.Instance.Combat.TryDealDamage(
                attacker: owner,
                victimGO: other.transform.root.gameObject,
                damage: damage,
                hitPoint: hitPoint,
                hitNormal: hitNormal,
                hitPart: HitPart.Body,
                source: "EnemyProjectile"
            );

            Destroy(gameObject);
            return;
        }
    }
}
