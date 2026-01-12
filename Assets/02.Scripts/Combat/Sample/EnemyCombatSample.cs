using Combat.Core;
using Combat.Damage;
using UnityEngine;

namespace Combat.Sample
{
    /// <summary>
    /// 전투 시스템을 활용한 적(몬스터) 샘플 코드입니다.
    /// 
    /// 필요한 컴포넌트:
    /// - Combatant: 전투 참여자 (Team을 Enemy로 설정)
    /// 
    /// Inspector 설정:
    /// - Team: Enemy
    /// - StatsData: CombatStatsData SO 할당
    /// - HitReactionSettings: HitReactionSettings SO 할당 (피격 시 자동 무적/경직)
    /// </summary>
    [RequireComponent(typeof(Combatant))]
    public class EnemyCombatSample : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _destroyDelay = 1f;
        [SerializeField] private bool _useObjectPooling;

        [Header("References")]
        [SerializeField] private Animator _animator;

        [Header("VFX")]
        [SerializeField] private GameObject _hitVFXPrefab;
        [SerializeField] private GameObject _criticalHitVFXPrefab;
        [SerializeField] private GameObject _deathVFXPrefab;
        [SerializeField] private float _vfxLifetime = 2f;

        [Header("SFX")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _criticalHitSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;

        [Header("Damage Number")]
        [SerializeField] private GameObject _damageNumberPrefab;
        [SerializeField] private Vector3 _damageNumberOffset = new Vector3(0, 2f, 0);
        
        private Combatant _combatant;
        private Rigidbody _rigidbody;
        
        private static readonly int DamageTrigger = Animator.StringToHash("Hit");
        private static readonly int DeathTrigger = Animator.StringToHash("Death");

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            SetupDeathBehavior();
            SubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SetupDeathBehavior()
        {
            if (_useObjectPooling)
            {
                _combatant.DisableOnDeath();
            }
            else
            {
                _combatant.DestroyOnDeath(_destroyDelay);
            }
        }

        private void SubscribeEvents()
        {
            _combatant.OnDamaged += HandleDamaged;
            _combatant.OnDeath += HandleDeath;
            _combatant.OnHitStunStart += HandleHitStunStart;
            _combatant.OnHitStunEnd += HandleHitStunEnd;
            
            if (_rigidbody != null)
            {
                _combatant.ApplyKnockbackOnDamage(_rigidbody, 3f);
            }
        }

        private void UnsubscribeEvents()
        {
            _combatant.OnDamaged -= HandleDamaged;
            _combatant.OnDeath -= HandleDeath;
            _combatant.OnHitStunStart -= HandleHitStunStart;
            _combatant.OnHitStunEnd -= HandleHitStunEnd;
        }

        private void HandleDamaged(DamageInfo info)
        {
            Debug.Log($"[Enemy] 피격: {info.Amount} 데미지 (남은 체력: {_combatant.CurrentHealth:F0})");

            PlayDamageAnimation();
            SpawnHitVFX(info);
            PlayHitSFX(info);
            SpawnDamageNumber(info);
        }

        private void HandleDeath()
        {
            Debug.Log("[Enemy] 사망");

            PlayDeathAnimation();
            SpawnDeathVFX();
            PlayDeathSFX();
            DropLoot();
            DisableCollision();
        }

        private void HandleHitStunStart()
        {
            // 경직 시작 - AI 행동 중지
        }

        private void HandleHitStunEnd()
        {
            // 경직 종료 - AI 행동 재개
        }

        private void PlayDamageAnimation()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(DamageTrigger);
            }
        }

        private void PlayDeathAnimation()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(DeathTrigger);
            }
        }

        private void SpawnHitVFX(DamageInfo info)
        {
            var prefab = info.IsCritical ? _criticalHitVFXPrefab : _hitVFXPrefab;
            if (prefab == null) return;

            Vector3 spawnPos = info.HitPoint != Vector3.zero
                ? info.HitPoint
                : transform.position + Vector3.up;

            var vfx = Instantiate(prefab, spawnPos, Quaternion.identity);
            Destroy(vfx, _vfxLifetime);
        }

        private void SpawnDeathVFX()
        {
            if (_deathVFXPrefab == null) return;

            var vfx = Instantiate(_deathVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, _vfxLifetime);
        }

        private void PlayHitSFX(DamageInfo info)
        {
            if (_audioSource == null) return;

            var clip = info.IsCritical ? _criticalHitSound : _hitSound;
            if (clip != null)
            {
                _audioSource.PlayOneShot(clip, _sfxVolume);
            }
        }

        private void PlayDeathSFX()
        {
            if (_audioSource == null || _deathSound == null) return;
            _audioSource.PlayOneShot(_deathSound, _sfxVolume);
        }

        private void SpawnDamageNumber(DamageInfo info)
        {
            if (_damageNumberPrefab == null) return;

            var pos = transform.position + _damageNumberOffset;
            var go = Instantiate(_damageNumberPrefab, pos, Quaternion.identity);

            var damageNumberUI = go.GetComponent<UI.DamageNumberUI>();
            damageNumberUI?.Initialize(info.Amount, info.IsCritical);
        }

        private void DropLoot()
        {
            // TODO: 아이템 드롭 시스템과 연동
            // LootSystem.Drop(transform.position);
        }

        private void DisableCollision()
        {
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }
        }

        /// <summary>
        /// 오브젝트 풀에서 재사용 시 호출
        /// </summary>
        public void ResetForPool()
        {
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = true;
            }
            
            gameObject.SetActive(true);
        }
    }
}
