 using System.Collections;
 using UnityEngine;

 public enum DropItemType
 {
     Gold,
     Exp
 }

 public class FieldDropItem : MonoBehaviour
 {
    [Header("Settings")]
    [SerializeField] private DropItemType type;
    [SerializeField] private int value;
    [SerializeField] private float attractSpeed = 12f;
    [SerializeField] private float pickupDistance = 0.5f;
    [SerializeField] private float startFollowDelay = 0.6f;
 
    private Transform _playerTransform;
    private bool _isFollowing = false;
    private Rigidbody _rb;
 
    public void Init(DropItemType type, int value)
    {
        this.type = type;
        this.value = value;
        _rb = GetComponent<Rigidbody>();
 
        // 생성 시 사방으로 튀어오르는 연출
        if (_rb != null)
        {
            Vector3 force = (Random.insideUnitSphere + Vector3.up * 1.5f).normalized * 4f;
            _rb.AddForce(force, ForceMode.Impulse);
        }
 
        StartCoroutine(CoWaitAndFollow());
    }
 
    private IEnumerator CoWaitAndFollow()
    {
        yield return new WaitForSeconds(startFollowDelay);
 
        if (Player.Instance != null)
        {
            _playerTransform = Player.Instance.transform;
            _isFollowing = true;
            
            if (_rb != null)
            {
                _rb.isKinematic = true;
                _rb.useGravity = false;
            }
        }
    }
 
    private void Update()
    {
        if (!_isFollowing || _playerTransform == null) return;
 
        // 플레이어 가슴 위치 정도를 타겟으로 함
        Vector3 targetPos = _playerTransform.position + Vector3.up * 1.0f;
 
        // 등속 이동 및 가속 효과
        attractSpeed += Time.deltaTime * 5f;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, attractSpeed * Time.deltaTime);
 
        // 도착 시 보상 지급 및 파괴
        if (Vector3.Distance(transform.position, targetPos) < pickupDistance)
        {
            ApplyReward();
            Destroy(gameObject);
        }
    }
 
    private void ApplyReward() 
    {
         var saveManager = Managers.Instance.SaveData;
         if (saveManager == null) return;
 
         if (type == DropItemType.Gold)
             saveManager.AddGold(value);
         else if (type == DropItemType.Exp)
             saveManager.AddExp(value);
 
         // TODO: 효과음 재생 (예: AudioSource.PlayClipAtPoint)
    } 
 }
 