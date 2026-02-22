using UnityEngine;

namespace Boss.Data
{
    [CreateAssetMenu(fileName = "BossData", menuName = "ProjectG/Boss/BossData")]
    public class BossData : ScriptableObject
    {
        [Header("기본 스탯")]
        [SerializeField] private float _maxHP = 1000f;
        [SerializeField] private float _moveSpeed = 4f;
        [SerializeField] private float _rotationSpeed = 120f;

        [Header("슈퍼아머")]
        [Tooltip("최대 포이즈 값")]
        [SerializeField] private float _maxPoise = 100f;
        [Tooltip("포이즈 회복 딜레이 (그로기 종료 후)")]
        [SerializeField] private float _poiseRecoveryDelay = 2f;

        [Header("근접 공격 (Attack01)")]
        [SerializeField] private float _meleeRange = 3f;
        [SerializeField] private float _meleeDamage = 30f;
        [SerializeField] private float _meleeCooldown = 2f;

        [Header("돌진 공격")]
        [SerializeField] private float _chargeSpeed = 12f;
        [SerializeField] private float _chargeDistance = 15f;
        [SerializeField] private float _chargeDamage = 50f;
        [SerializeField] private float _chargeCooldown = 8f;

        [Header("브레스 공격 (Attack03)")]
        [Tooltip("브레스 부채꼴 각도")]
        [SerializeField] private float _breathAngle = 60f;
        [SerializeField] private float _breathRange = 10f;
        [SerializeField] private float _breathDamage = 20f;
        [Tooltip("브레스 지속 시간")]
        [SerializeField] private float _breathDuration = 3f;
        [SerializeField] private float _breathCooldown = 12f;

        [Header("투사체 공격 (Attack02)")]
        [SerializeField] private int _projectileCount = 3;
        [SerializeField] private float _projectileDamage = 25f;
        [SerializeField] private float _projectileSpeed = 15f;
        [SerializeField] private float _projectileCooldown = 6f;
        [SerializeField] private GameObject _projectilePrefab;

        [Header("소환")]
        [Tooltip("소환할 잡졸 프리팹 목록")]
        [SerializeField] private GameObject[] _summonPrefabs;
        [Tooltip("한 번에 소환할 수")]
        [SerializeField] private int _summonCount = 3;
        [Tooltip("동시 생존 가능 최대 수")]
        [SerializeField] private int _maxAliveMinions = 5;
        [SerializeField] private float _summonCooldown = 20f;
        [Tooltip("소환 가능 HP% (이하일 때)")]
        [SerializeField, Range(0f, 1f)] private float _summonHPThreshold = 0.7f;
        [Tooltip("소환 위치 반경 (보스 기준)")]
        [SerializeField] private float _minionSpawnRadius = 5f;
        [Tooltip("잡졸 그룹 공격 슬롯 수")]
        [SerializeField] private int _minionAttackSlots = 2;

        [Header("분노")]
        [Tooltip("분노 발동 HP%")]
        [SerializeField, Range(0f, 1f)] private float _enrageHPThreshold = 0.3f;
        [SerializeField] private float _enrageDamageMultiplier = 1.3f;
        [SerializeField] private float _enrageSpeedMultiplier = 1.2f;

        [Header("그로기")]
        [Tooltip("그로기 지속 시간")]
        [SerializeField] private float _staggerDuration = 3f;
        [Tooltip("그로기 종료 후 무적 시간")]
        [SerializeField] private float _postStaggerInvincibilityDuration = 1f;

        [Header("감지")]
        [SerializeField] private float _detectionRange = 20f;

        [Header("페이즈")]
        [SerializeField] private BossPhaseData[] _phases;

        // 기본 스탯 Properties
        public float MaxHP => _maxHP;
        public float MoveSpeed => _moveSpeed;
        public float RotationSpeed => _rotationSpeed;

        // 슈퍼아머 Properties
        public float MaxPoise => _maxPoise;
        public float PoiseRecoveryDelay => _poiseRecoveryDelay;

        // 근접 공격 Properties
        public float MeleeRange => _meleeRange;
        public float MeleeDamage => _meleeDamage;
        public float MeleeCooldown => _meleeCooldown;

        // 돌진 공격 Properties
        public float ChargeSpeed => _chargeSpeed;
        public float ChargeDistance => _chargeDistance;
        public float ChargeDamage => _chargeDamage;
        public float ChargeCooldown => _chargeCooldown;

        // 브레스 공격 Properties
        public float BreathAngle => _breathAngle;
        public float BreathRange => _breathRange;
        public float BreathDamage => _breathDamage;
        public float BreathDuration => _breathDuration;
        public float BreathCooldown => _breathCooldown;

        // 투사체 공격 Properties
        public int ProjectileCount => _projectileCount;
        public float ProjectileDamage => _projectileDamage;
        public float ProjectileSpeed => _projectileSpeed;
        public float ProjectileCooldown => _projectileCooldown;
        public GameObject ProjectilePrefab => _projectilePrefab;

        // 소환 Properties
        public GameObject[] SummonPrefabs => _summonPrefabs;
        public int SummonCount => _summonCount;
        public int MaxAliveMinions => _maxAliveMinions;
        public float SummonCooldown => _summonCooldown;
        public float SummonHPThreshold => _summonHPThreshold;
        public float MinionSpawnRadius => _minionSpawnRadius;
        public int MinionAttackSlots => _minionAttackSlots;

        // 분노 Properties
        public float EnrageHPThreshold => _enrageHPThreshold;
        public float EnrageDamageMultiplier => _enrageDamageMultiplier;
        public float EnrageSpeedMultiplier => _enrageSpeedMultiplier;

        // 그로기 Properties
        public float StaggerDuration => _staggerDuration;
        public float PostStaggerInvincibilityDuration => _postStaggerInvincibilityDuration;

        // 감지 Properties
        public float DetectionRange => _detectionRange;

        // 페이즈 Properties
        public BossPhaseData[] Phases => _phases;
    }
}
