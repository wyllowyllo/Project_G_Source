using Combat.Attack;
using Combat.Core;
using Combat.Damage;
using Drakkar.GameUtils;
using UnityEngine;

namespace Player
{
    public class PlayerVFXController : MonoBehaviour
    {
        [Header("Attack VFX")]
        [SerializeField] private GameObject[] _attackVFX;
        [SerializeField] private Transform _attackVFXSpawnPoint;

        [Header("Hit VFX")]
        [SerializeField] private GameObject[] _hitVFX;

        [Header("Damaged VFX")]
        [SerializeField] private GameObject _damagedVFX;

        [Header("VFX Settings")]
        [SerializeField] private float _vfxLifetime = 2f;

        [Header("Trail Effect")]
        [SerializeField] private DrakkarTrail _weaponTrail;

        private MeleeAttacker _attacker;
        private Combatant _combatant;

        private void Awake()
        {
            _attacker = GetComponent<MeleeAttacker>();
            _combatant = GetComponent<Combatant>();

            if (_attackVFXSpawnPoint == null)
            {
                Debug.LogWarning("[PlayerVFXController] AttackVFXSpawnPoint not assigned");
            }
        }

        private void OnEnable()
        {
            if (_attacker != null)
            {
                _attacker.OnComboAttack += HandleComboAttack;
                _attacker.OnHit += HandleHit;
            }

            if (_combatant != null)
            {
                _combatant.OnDamaged += HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (_attacker != null)
            {
                _attacker.OnComboAttack -= HandleComboAttack;
                _attacker.OnHit -= HandleHit;
            }

            if (_combatant != null)
            {
                _combatant.OnDamaged -= HandleDamaged;
            }
        }

        private void HandleComboAttack(int step, float multiplier)
        {
            SpawnAttackVFX(step);
        }

        private void HandleHit(IDamageable target, DamageInfo info)
        {
            int comboStep = _attacker != null ? _attacker.CurrentComboStep : 0;
            SpawnHitVFX(info.HitPoint, comboStep);
        }

        private void HandleDamaged(DamageInfo info)
        {
            SpawnDamagedVFX(info.HitPoint);
        }

        public void SpawnDamagedVFX(Vector3 position)
        {
            if (_damagedVFX == null)
            {
                return;
            }

            GameObject vfx = Instantiate(_damagedVFX, position, Quaternion.identity);
            Destroy(vfx, _vfxLifetime);
        }

        public void SpawnAttackVFX(int comboStep)
        {
            if(_attackVFX == null || _attackVFX.Length == 0)
            {
                return;
            }

            int index = Mathf.Clamp(comboStep - 1, 0, _attackVFX.Length - 1);
            GameObject vfxPrefab = _attackVFX[index];

            if (vfxPrefab == null || _attackVFXSpawnPoint == null)
            {
                return;
            }

            GameObject vfx = Instantiate(vfxPrefab, _attackVFXSpawnPoint.position, _attackVFXSpawnPoint.rotation);

            Destroy(vfx, _vfxLifetime);
        }

        public void SpawnHitVFX(Vector3 position, int comboStep)
        {
            if(_hitVFX == null || _hitVFX.Length == 0)
            {
                return;
            }

            int index = Mathf.Clamp(comboStep - 1, 0, _hitVFX.Length - 1);
            GameObject vfxPrefab = _hitVFX[index];

            if (vfxPrefab == null)
            {
                return;
            }

            GameObject vfx = Instantiate(vfxPrefab, position, Quaternion.identity);

            Destroy(vfx, _vfxLifetime);
        }

        // 커스텀
        public void Spawn(GameObject vfxPrefab, Vector3 position, Quaternion rotation)
        {
            if(vfxPrefab == null)
            {
                return;
            }
        
            GameObject vfx = Instantiate(vfxPrefab, position, rotation);
            Destroy(vfx, _vfxLifetime);
        }

        // VFX 설정
        public void SetAttackVFX(GameObject[] vfxArray)
        {
            _attackVFX = vfxArray;
        }

        public void SetHitVFX(GameObject[] vfxArray)
        {
            _hitVFX = vfxArray;
        }

        public void SetVFXLifetime(float lifetime)
        {
            _vfxLifetime = lifetime;
        }

        public void StartTrail()
        {
            _weaponTrail?.Begin();
        }

        

        public void StopAllEffects()
        {
            _weaponTrail?.End();
        }
    }
}

