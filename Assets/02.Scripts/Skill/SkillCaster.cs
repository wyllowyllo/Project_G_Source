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
        private const float DEFAULT_GLIDE_COOLDOWN = 10f;

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
        private PlayerCombat _playerCombat;
        private PlayerTargetController _targetController;
        private SkillCameraDirector _cameraDirector;

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
        private SkillSlot _currentSlot;
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
            _playerCombat = GetComponent<PlayerCombat>();
            _targetController = GetComponent<PlayerTargetController>();
            _cameraDirector = GetComponentInChildren<SkillCameraDirector>();

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
            if (_playerCombat != null && _playerCombat.IsDodging) return false;

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
            float cooldown = tier?.Cooldown ?? DEFAULT_GLIDE_COOLDOWN;

            _isCasting = true;
            _cooldownEndTimes[slot] = Time.time + cooldown;
            OnSkillUsed?.Invoke(slot, cooldown);

            _combatant.SetSuperArmor(true);
            _targetController?.RotateTowardsNearestTarget();
            int rank = _skillLevels[slot] + 1;
            _glideController.PrepareGlide(rank);
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
            _currentSlot = slot;
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

            _targetController?.RotateTowardsNearestTarget();
            _animationController?.PlaySkill(slot);

            StartCameraSequence(tier);
        }

        private void StartCameraSequence(SkillTierData tier)
        {
            if (_cameraDirector == null) return;

            var cameraConfig = tier.CameraConfig;
            if (cameraConfig == null) return;

            float animDuration = tier.AnimationDuration > 0 ? tier.AnimationDuration : 1f;
            _cameraDirector.StartSequence(cameraConfig, animDuration);
        }
        
        public void OnSkillDamageFrame()
        {
            if (!_isCasting || _currentSkill == null || _currentTier == null)
                return;

            int rank = _skillLevels.TryGetValue(_currentSlot, out var level) ? level + 1 : 1;
            var areaContext = SkillAreaContext.Create(
                _currentSkill,
                _currentTier,
                _enemyLayer,
                _combatant.Team,
                rank
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
                    tier.ConeHeight,
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

            _vfxController?.StopTrail();
            _cameraDirector?.CancelSequence();

            _isCasting = false;
            _currentSkill = null;
            _currentTier = null;
            _combatant?.SetSuperArmor(false);
        }

        public void StartSkillTrail()
        {
            _vfxController?.StartTrail();
        }

        public void StopSkillTrail()
        {
            _vfxController?.StopTrail();
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
