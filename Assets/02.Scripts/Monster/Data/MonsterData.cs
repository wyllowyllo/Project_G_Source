using UnityEngine;

namespace Monster.Data
{
    /// <summary>
    /// 몬스터의 기본 스탯과 설정을 정의하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterData", menuName = "ProjectG/Monster/MonsterData")]
    public class MonsterData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string _monsterName = "Monster";
        [SerializeField] private int _monsterLevel = 1;

        [Header("전투 스탯")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _attackDamage = 10f;
        [SerializeField] private float _attackRange = 2f;
        [SerializeField] private float _attackCooldown = 1.5f;

        [Header("이동 스탯")]
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _rotationSpeed = 120f;

        [Header("감지 범위")]
        [SerializeField] private float _detectionRange = 12f;
        [SerializeField] private float _engageRange = 10f;

        [Header("BDO 스타일 - 거리 밴드 시스템")]
        [Tooltip("선호 최소 거리 - 이보다 가까우면 후퇴")]
        [SerializeField] private float _preferredMinDistance = 2.0f;
        [Tooltip("선호 최대 거리 - 이보다 멀면 접근")]
        [SerializeField] private float _preferredMaxDistance = 4.0f;
        [Tooltip("스트레이프(좌우 이동) 속도")]
        [SerializeField] private float _strafeSpeed = 2.5f;

        [Header("BDO 스타일 - 테더 시스템")]
        [Tooltip("홈 포지션으로부터 최대 이탈 거리")]
        [SerializeField] private float _tetherRadius = 20f;

        [Header("BDO 스타일 - 스킬 패턴")]
        [Tooltip("공격 준비 시간 (텔레그래프)")]
        [SerializeField] private float _windupTime = 0.3f;
        [Tooltip("공격 실행 시간")]
        [SerializeField] private float _executeTime = 0.2f;
        [Tooltip("공격 후딜 시간")]
        [SerializeField] private float _recoverTime = 0.5f;

        [Header("근접 공격 - 돌진 패턴")]
        [Tooltip("돌진 속도 (기본 이동 속도보다 빠름)")]
        [SerializeField] private float _chargeSpeed = 7f;
        [Tooltip("후퇴 거리 (공격 후 뒤로 물러나는 거리)")]
        [SerializeField] private float _retreatDistance = 2f;

        [Header("경험치 및 보상")]
        [SerializeField] private int _experienceReward = 10;
        [SerializeField] private int _goldReward = 5;

        // Properties
        public string MonsterName => _monsterName;
        public int MonsterLevel => _monsterLevel;
        public float MaxHealth => _maxHealth;
        public float AttackDamage => _attackDamage;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public float MoveSpeed => _moveSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float DetectionRange => _detectionRange;
        public float EngageRange => _engageRange;
        public int ExperienceReward => _experienceReward;
        public int GoldReward => _goldReward;

        // BDO 스타일 Properties
        public float PreferredMinDistance => _preferredMinDistance;
        public float PreferredMaxDistance => _preferredMaxDistance;
        public float StrafeSpeed => _strafeSpeed;
        public float TetherRadius => _tetherRadius;
        public float WindupTime => _windupTime;
        public float ExecuteTime => _executeTime;
        public float RecoverTime => _recoverTime;

        // 근접 공격 Properties
        public float ChargeSpeed => _chargeSpeed;
        public float RetreatDistance => _retreatDistance;
    }
}
