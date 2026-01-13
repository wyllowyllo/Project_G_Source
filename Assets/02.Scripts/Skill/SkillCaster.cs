using System;
using UnityEngine;
using Combat.Core;
using Player;
using Progression;

namespace Skill
{
    [RequireComponent(typeof(SkillHitbox))]
    public class SkillCaster : MonoBehaviour
    {
        [Header("Skills")]
        [SerializeField] private PlayerSkillData _qSkill;
        [SerializeField] private PlayerSkillData _eSkill;
        [SerializeField] private PlayerSkillData _rSkill;
        [SerializeField] private LayerMask _enemyLayer;

        private Combatant _combatant;
        private PlayerProgression _progression;
        private PlayerInputHandler _inputHandler;
        private PlayerAnimationController _animationController;
        private PlayerMovement _playerMovement;
        private PlayerVFXController _vfxController;
        private SkillHitbox _skillHitbox;

        private readonly int[] _skillLevels = new int[3];
        private readonly float[] _cooldownEndTimes = new float[3];
        private bool _isCasting;
        
        private PlayerSkillData _currentSkill;
        private SkillTierData _currentTier;

        public bool IsCasting => _isCasting;
        public event Action<SkillSlot, float> OnSkillUsed;
        public event Action<HitInfo> OnSkillHit;

        private PlayerSkillData GetSkillData(SkillSlot slot) => slot switch
        {
            SkillSlot.Q => _qSkill,
            SkillSlot.E => _eSkill,
            SkillSlot.R => _rSkill,
            _ => null
        };

        private void Awake()
        {
            _skillHitbox = GetComponent<SkillHitbox>();
            ValidateSkills();
        }

        private void Start()
        {
            _combatant = GetComponent<Combatant>();
            _progression = GetComponent<PlayerProgression>();
            _inputHandler = GetComponent<PlayerInputHandler>();
            _animationController = GetComponent<PlayerAnimationController>();
            _playerMovement = GetComponent<PlayerMovement>();
            _vfxController = GetComponent<PlayerVFXController>();

            if (_progression != null)
                _progression.OnSkillEnhanced += HandleSkillEnhanced;

            if (_inputHandler != null)
                _inputHandler.OnSkillInputPressed += HandleSkillInput;

            if (_skillHitbox != null)
                _skillHitbox.OnHit += HandleHit;
        }

        private void OnDestroy()
        {
            if (_progression != null)
                _progression.OnSkillEnhanced -= HandleSkillEnhanced;

            if (_inputHandler != null)
                _inputHandler.OnSkillInputPressed -= HandleSkillInput;

            if (_skillHitbox != null)
                _skillHitbox.OnHit -= HandleHit;
        }

        public void HandleSkillInput(SkillSlot slot)
        {
            TryUseSkill(slot);
        }

        public bool TryUseSkill(SkillSlot slot)
        {
            if (!IsSkillReady(slot)) return false;
            if (_isCasting) return false;

            var skill = GetSkillData(slot);
            if (skill == null) return false;

            ExecuteSkill(skill, slot);
            return true;
        }

        public bool IsSkillReady(SkillSlot slot)
        {
            int index = (int)slot - 1;
            if (index < 0 || index >= _cooldownEndTimes.Length) return false;
            return Time.time >= _cooldownEndTimes[index];
        }

        public float GetRemainingCooldown(SkillSlot slot)
        {
            int index = (int)slot - 1;
            if (index < 0 || index >= _cooldownEndTimes.Length) return 0f;
            return Mathf.Max(0f, _cooldownEndTimes[index] - Time.time);
        }

        private void ExecuteSkill(PlayerSkillData skill, SkillSlot slot)
        {
            int index = (int)slot - 1;
            var tier = skill.GetTier(_skillLevels[index]);
            
            _currentSkill = skill;
            _currentTier = tier;

            _isCasting = true;
            _cooldownEndTimes[index] = Time.time + tier.Cooldown;
            OnSkillUsed?.Invoke(slot, tier.Cooldown);

            _combatant.SetSuperArmor(true);

            if (!tier.AllowMovement)
            {
                _playerMovement?.SetMovementEnabled(false);
            }

            _animationController?.PlaySkill(slot);
        }
        
        public void OnSkillDamageFrame()
        {
            if (!_isCasting || _currentSkill == null || _currentTier == null)
                return;
            
            _skillHitbox.PerformCheck(
                _currentSkill.AreaType,
                _currentTier.Range,
                _currentTier.Angle,
                _currentTier.BoxWidth,
                _currentTier.BoxHeight,
                _currentTier.PositionOffset,
                _enemyLayer,
                _combatant.Team
            );
            
            SpawnEffect(_currentTier);
        }

        private void HandleHit(HitInfo hitInfo)
        {
            if (hitInfo.TargetCombatant is Combatant targetCombatant)
            {
                var attackContext = AttackContext.Scaled(
                    _combatant,
                    _currentTier.DamageMultiplier,
                    1f,
                    DamageType.Skill
                );
                var hitContext = new HitContext(
                    hitInfo.TargetCollider.transform.position,
                    (hitInfo.TargetCollider.transform.position - transform.position).normalized,
                    DamageType.Skill
                );
                targetCombatant.TakeDamage(attackContext, hitContext);
            }

            OnSkillHit?.Invoke(hitInfo);
        }

        private void SpawnEffect(SkillTierData tier)
        {
            if (tier.EffectPrefab != null)
            {
                Instantiate(tier.EffectPrefab, transform.position, transform.rotation);
            }
            else
            {
                SkillDebugVisual.Spawn(
                    _currentSkill.AreaType,
                    transform.position,
                    transform.rotation,
                    tier.Range,
                    tier.Angle,
                    tier.BoxWidth,
                    tier.BoxHeight,
                    tier.PositionOffset
                );
            }
        }

        private void HandleSkillEnhanced(SkillSlot slot)
        {
            int index = (int)slot - 1;
            if (index >= 0 && index < _skillLevels.Length)
            {
                _skillLevels[index]++;
            }
        }
        
        public void OnSkillComplete()
        {
            if (!_isCasting)
                return;

            if (_currentTier != null && !_currentTier.AllowMovement)
            {
                _playerMovement?.SetMovementEnabled(true);
            }

            _isCasting = false;
            _currentSkill = null;
            _currentTier = null;
            _combatant?.SetSuperArmor(false);
            _animationController?.EndSkill();
        }

        public void CancelSkill()
        {
            if (!_isCasting)
                return;

            if (_currentTier != null && !_currentTier.AllowMovement)
            {
                _playerMovement?.SetMovementEnabled(true);
            }

            _vfxController?.StopSkillTrail();

            _isCasting = false;
            _currentSkill = null;
            _currentTier = null;
            _combatant?.SetSuperArmor(false);
        }

#if UNITY_EDITOR
        private void ValidateSkills()
        {
            if (_qSkill == null) Debug.LogWarning($"[{nameof(SkillCaster)}] Q skill not assigned on {gameObject.name}");
            if (_eSkill == null) Debug.LogWarning($"[{nameof(SkillCaster)}] E skill not assigned on {gameObject.name}");
            if (_rSkill == null) Debug.LogWarning($"[{nameof(SkillCaster)}] R skill not assigned on {gameObject.name}");
        }
#else
        private void ValidateSkills() { }
#endif
    }
}
