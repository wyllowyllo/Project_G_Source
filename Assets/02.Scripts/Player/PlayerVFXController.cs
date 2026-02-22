using System.Collections;
using Combat.Attack;
using Combat.Core;
using Combat.Damage;
using Drakkar.GameUtils;
using Pool.Core;
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

        [Header("Attack SFX")]
        [SerializeField] private AudioClip[] _attackSFX;
        [SerializeField] private AudioClip[] _hitSFX;

        [Header("Damaged SFX")]
        [SerializeField] private AudioClip[] _damagedSFX;

        [Header("Dodge SFX")]
        [SerializeField] private AudioClip[] _dodgeSFX;

        private MeleeAttacker _attacker;
        private Combatant _combatant;
        private AttackSession _currentTrailSession;
        private Coroutine _trailCoroutine;
        private bool _trailActive;

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
            PlayHitSFX(comboStep);
        }

        private void HandleDamaged(DamageInfo info)
        {
            SpawnDamagedVFX(info.HitPoint);
            PlayDamagedSFX();
        }

        public void SpawnDamagedVFX(Vector3 position)
        {
            if (_damagedVFX == null)
            {
                return;
            }

            PoolSpawner.SpawnVFX(_damagedVFX, position, Quaternion.identity, _vfxLifetime);
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

            PoolSpawner.SpawnVFX(vfxPrefab, _attackVFXSpawnPoint.position, _attackVFXSpawnPoint.rotation, _vfxLifetime);
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

            PoolSpawner.SpawnVFX(vfxPrefab, position, Quaternion.identity, _vfxLifetime);
        }
        
        public void Spawn(GameObject vfxPrefab, Vector3 position, Quaternion rotation)
        {
            if(vfxPrefab == null)
            {
                return;
            }

            PoolSpawner.SpawnVFX(vfxPrefab, position, rotation, _vfxLifetime);
        }
        
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

        public void StartTrail(AttackSession session)
        {
            _currentTrailSession = session;
            StartTrail();
        }

        public void StartTrail()
        {
            if (_weaponTrail == null)
                return;

            if (_trailCoroutine != null)
            {
                StopCoroutine(_trailCoroutine);
            }
            _trailCoroutine = StartCoroutine(StartTrailNextFrame());
        }

        private IEnumerator StartTrailNextFrame()
        {
            if (_trailActive)
            {
                _weaponTrail.Clear();
                _trailActive = false;
                yield return null;
            }

            _weaponTrail.Begin();
            _trailActive = true;
        }

        public void StopTrail(AttackSession session)
        {
            if (session != _currentTrailSession)
                return;

            _currentTrailSession = null;
            StopTrail();
        }

        public void StopTrail()
        {
            if (_trailActive)
            {
                _weaponTrail?.End();
                _trailActive = false;
            }
        }

        public void StopAllEffects()
        {
            if (_trailCoroutine != null)
            {
                StopCoroutine(_trailCoroutine);
                _trailCoroutine = null;
            }
            _currentTrailSession = null;

            if (_trailActive)
            {
                _weaponTrail?.Clear();
                _trailActive = false;
            }
        }

        public void PlayAttackSFX(int comboStep)
        {
            if (_attackSFX == null || _attackSFX.Length == 0) return;

            int index = Mathf.Clamp(comboStep - 1, 0, _attackSFX.Length - 1);
            AudioClip clip = _attackSFX[index];

            if (clip != null)
            {
                SoundManager.Instance?.PlaySfx(clip);
            }
        }

        private void PlayHitSFX(int comboStep)
        {
            if (_hitSFX == null || _hitSFX.Length == 0) return;

            int index = Mathf.Clamp(comboStep - 1, 0, _hitSFX.Length - 1);
            AudioClip clip = _hitSFX[index];

            if (clip != null)
            {
                SoundManager.Instance?.PlaySfx(clip);
            }
        }

        private void PlayDamagedSFX()
        {
            if (_damagedSFX == null || _damagedSFX.Length == 0) return;

            int index = Random.Range(0, _damagedSFX.Length);
            AudioClip clip = _damagedSFX[index];

            if (clip != null)
            {
                SoundManager.Instance?.PlaySfx(clip);
            }
        }

        public void PlayDodgeSFX()
        {
            if (_dodgeSFX == null || _dodgeSFX.Length == 0) return;

            int index = Random.Range(0, _dodgeSFX.Length);
            AudioClip clip = _dodgeSFX[index];

            if (clip != null)
            {
                SoundManager.Instance?.PlaySfx(clip);
            }
        }
    }
}

