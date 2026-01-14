using System;
using System.Collections.Generic;
using Combat.Core;
using Combat.Damage;
using Player;
using Progression;
using UnityEngine;

namespace Skill
{
    [RequireComponent(typeof(SkillHitbox))]
    public class SkillCaster : MonoBehaviour, ISkillAnimationReceiver
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
        private GlideController _glideController;

        private readonly Dictionary<SkillSlot, int> _skillLevels = new()
        {
            { SkillSlot.Q, 0 },
            { SkillSlot.E, 0 },
            { SkillSlot.R, 0 }
        };
        private readonly Dictionary<SkillSlot, float> _cooldownEndTimes = new()
        {
            { SkillSlot.Q, 0f },
            { SkillSlot.E, 0f },
            { SkillSlot.R, 0f }
        };
        private bool _isCasting;

        private PlayerSkillData _currentSkill;
        private SkillTierData _currentTier;
        private AttackContext _currentAttackContext;

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
            _glideController = GetComponent<GlideController>();

            if (_progression != null)
                _progression.OnSkillEnhanced += HandleSkillEnhanced;

            if (_inputHandler != null)
                _inputHandler.OnSkillInputPressed += HandleSkillInput;

            if (_skillHitbox != null)
                _skillHitbox.OnHit += HandleHit;

            if (_glideController != null)
            {
                _glideController.OnGlideEnded += HandleGlideEnded;
                _glideController.OnDiveBombDamageRequest += HandleDiveBombDamage;
            }
        }

        private void OnDestroy()
        {
            if (_progression != null)
                _progression.OnSkillEnhanced -= HandleSkillEnhanced;

            if (_inputHandler != null)
                _inputHandler.OnSkillInputPressed -= HandleSkillInput;

            if (_skillHitbox != null)
                _skillHitbox.OnHit -= HandleHit;

            if (_glideController != null)
            {
                _glideController.OnGlideEnded -= HandleGlideEnded;
                _glideController.OnDiveBombDamageRequest -= HandleDiveBombDamage;
            }
        }

        public void HandleSkillInput(SkillSlot slot)
        {
            TryUseSkill(slot);
        }

        public bool TryUseSkill(SkillSlot slot)
        {
            if (!IsSkillReady(slot)) return false;
            if (_isCasting) return false;

            if (slot == SkillSlot.E && _glideController != null)
            {
                return TryUseGlideSkill(slot);
            }

            var skill = GetSkillData(slot);
            if (skill == null) return false;

            ExecuteSkill(skill, slot);
            return true;
        }

        private bool TryUseGlideSkill(SkillSlot slot)
        {
            if (_glideController.IsActive) return false;

            var skill = GetSkillData(slot);
            var tier = skill?.GetTier(_skillLevels[slot]);
            float cooldown = tier?.Cooldown ?? 10f;

            _isCasting = true;
            _cooldownEndTimes[slot] = Time.time + cooldown;
            OnSkillUsed?.Invoke(slot, cooldown);

            _combatant.SetSuperArmor(true);
            _glideController.PrepareGlide();
            return true;
        }

        private void HandleGlideEnded()
        {
            _isCasting = false;
            _combatant?.SetSuperArmor(false);
        }

        private void HandleDiveBombDamage(SkillAreaContext context, float damageMultiplier)
        {
            _currentAttackContext = AttackContext.Scaled(
                _combatant,
                damageMultiplier,
                1f,
                DamageType.Skill
            );

            _skillHitbox.PerformCheck(context);
        }

        public bool IsSkillReady(SkillSlot slot)
        {
            if (!_cooldownEndTimes.TryGetValue(slot, out float endTime)) return false;
            return Time.time >= endTime;
        }

        public float GetRemainingCooldown(SkillSlot slot)
        {
            if (!_cooldownEndTimes.TryGetValue(slot, out float endTime)) return 0f;
            return Mathf.Max(0f, endTime - Time.time);
        }

        private void ExecuteSkill(PlayerSkillData skill, SkillSlot slot)
        {
            var tier = skill.GetTier(_skillLevels[slot]);

            _currentSkill = skill;
            _currentTier = tier;
            _currentAttackContext = AttackContext.Scaled(
                _combatant,
                tier.DamageMultiplier,
                1f,
                DamageType.Skill
            );

            _isCasting = true;
            _cooldownEndTimes[slot] = Time.time + tier.Cooldown;
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

            var areaContext = SkillAreaContext.Create(
                _currentSkill,
                _currentTier,
                _enemyLayer,
                _combatant.Team
            );
            _skillHitbox.PerformCheck(areaContext);

            SpawnEffect(_currentTier);
        }

        private void HandleHit(HitInfo hitInfo)
        {
            var damageInfo = DamageProcessor.Process(
                _currentAttackContext,
                hitInfo,
                transform.position
            );
            hitInfo.Target.TakeDamage(damageInfo);
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
            if (_skillLevels.ContainsKey(slot))
            {
                _skillLevels[slot]++;
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

        public void StartSkillTrail()
        {
            _vfxController?.StartSkillTrail();
        }

        public void StopSkillTrail()
        {
            _vfxController?.StopSkillTrail();
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
