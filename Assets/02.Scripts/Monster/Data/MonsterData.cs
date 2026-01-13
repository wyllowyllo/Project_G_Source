using UnityEngine;

namespace Monster.Data
{
    // 공격 모드 (AI 테스트용)
    public enum EAttackMode
    {
        Both,       // 약공 + 강공 둘 다 사용
        LightOnly,  // 약공만 사용 (토큰 발행 없음)
        HeavyOnly   // 강공만 사용 (약공 없음)
    }

    // 몬스터의 기본 스탯과 설정을 정의하는 ScriptableObject
    [CreateAssetMenu(fileName = "MonsterData", menuName = "ProjectG/Monster/MonsterData")]
    public class MonsterData : ScriptableObject
    {
        [Header("테스트 옵션")]
        [Tooltip("공격 모드 (AI 테스트용)")]
        [SerializeField] private EAttackMode _attackMode = EAttackMode.Both;
        [Tooltip("Cascading Push-back 활성화 (AI 테스트용)")]
        [SerializeField] private bool _enablePushback = true;

        [Header("행동 패턴")]
        [Tooltip("순찰 모드 활성화 (비활성화 시 제자리 대기)")]
        [SerializeField] private bool _enablePatrol = true;
        [Tooltip("홈 복귀 활성화 (비활성화 시 끝까지 추적)")]
        [SerializeField] private bool _enableReturnHome = true;
        [Tooltip("순찰 반경 (홈 위치 기준)")]
        [SerializeField] private float _patrolRadius = 8f;
        [Tooltip("순찰 지점 도착 후 최소 대기 시간")]
        [SerializeField] private float _patrolWaitTimeMin = 1f;
        [Tooltip("순찰 지점 도착 후 최대 대기 시간")]
        [SerializeField] private float _patrolWaitTimeMax = 3f;

        [Header("기본 정보")]
        [SerializeField] private string _monsterName = "Monster";
        [SerializeField] private int _monsterLevel = 1;

        [Header("공격 행동")]
        [SerializeField] private float _attackRange = 2f;
        [Tooltip("약공 사정거리 (제자리 공격)")]
        [SerializeField] private float _lightAttackRange = 1.5f;
        [Tooltip("강공 사정거리 (돌진 공격)")]
        [SerializeField] private float _heavyAttackRange = 2.5f;
        [SerializeField] private float _attackCooldown = 1.5f;

        [Header("이동 스탯")]
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _rotationSpeed = 120f;

        [Header("감지 범위")]
        [SerializeField] private float _detectionRange = 12f;
        [SerializeField] private float _engageRange = 10f;

        [Header("거리 밴드 시스템")]
        [Tooltip("선호 최소 거리 - 이보다 가까우면 후퇴")]
        [SerializeField] private float _preferredMinDistance = 1.0f;
        [Tooltip("선호 최대 거리 - 이보다 멀면 접근")]
        [SerializeField] private float _preferredMaxDistance = 4.0f;
        [Tooltip("스트레이프(좌우 이동) 속도")]
        [SerializeField] private float _strafeSpeed = 1.5f;

        [Header("테더 시스템")]
        [Tooltip("홈 포지션으로부터 최대 이탈 거리")]
        [SerializeField] private float _tetherRadius = 20f;

        [Header("스킬 패턴")]
        [Tooltip("공격 준비 시간 (텔레그래프)")]
        [SerializeField] private float _windupTime = 0.3f;
        [Tooltip("공격 실행 시간")]
        [SerializeField] private float _executeTime = 0.2f;
        [Tooltip("약공 후 회복 시간")]
        [SerializeField] private float _lightAttackRecoverTime = 0.5f;
        [Tooltip("강공 후 회복 시간")]
        [SerializeField] private float _heavyAttackRecoverTime = 2f;

        [Header("근접 공격 - 돌진 패턴")]
        [Tooltip("돌진 속도 (기본 이동 속도보다 빠름)")]
        [SerializeField] private float _chargeSpeed = 7f;
        [Tooltip("후퇴 거리 (공격 후 뒤로 물러나는 거리)")]
        [SerializeField] private float _retreatDistance = 2f;

        [Header("전투 리듬")]
        [Tooltip("약공 발동 확률")]
        [SerializeField, Range(0f, 1f)] private float _lightAttackChance = 0.45f;
        [Tooltip("강공 발동 확률")]
        [SerializeField, Range(0f, 1f)] private float _heavyAttackChance = 0.15f;
        [Tooltip("강공 쿨다운")]
        [SerializeField] private float _heavyAttackCooldown = 3.0f;

        [Header("스트레이프 행동")]
        [Tooltip("원호 이동 각속도 (도/초)")]
        [SerializeField] private float _circleAngularSpeed = 15f;
        [Tooltip("스트레이프 행동 최소 지속 시간")]
        [SerializeField] private float _strafeMinDuration = 1.5f;
        [Tooltip("스트레이프 행동 최대 지속 시간")]
        [SerializeField] private float _strafeMaxDuration = 3.5f;
        [Tooltip("정지 상태 확률")]
        [SerializeField, Range(0f, 1f)] private float _strafePauseChance = 0.2f;
        [Tooltip("방향 전환 확률 (Circle 모드에서)")]
        [SerializeField, Range(0f, 1f)] private float _directionChangeChance = 0.3f;
        [Tooltip("속도 보간 계수 (높을수록 빠르게 가감속)")]
        [SerializeField] private float _speedLerpFactor = 3f;
        
        [Header("피격 반응")]
        [Tooltip("넉백 강도")]
        [SerializeField] private float _knockbackForce = 2f;

        [Header("경험치 및 보상")]
        [SerializeField] private int _experienceReward = 10;
        [SerializeField] private int _goldReward = 5;

        // Properties
        public string MonsterName => _monsterName;
        public int MonsterLevel => _monsterLevel;
        public float AttackRange => _attackRange;
        public float LightAttackRange => _lightAttackRange;
        public float HeavyAttackRange => _heavyAttackRange;
        public float AttackCooldown => _attackCooldown;
        public float MoveSpeed => _moveSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float DetectionRange => _detectionRange;
        public float EngageRange => _engageRange;
        public int ExperienceReward => _experienceReward;
        public int GoldReward => _goldReward;

       
        public float PreferredMinDistance => _preferredMinDistance;
        public float PreferredMaxDistance => _preferredMaxDistance;
        public float StrafeSpeed => _strafeSpeed;
        public float TetherRadius => _tetherRadius;
        public float WindupTime => _windupTime;
        public float ExecuteTime => _executeTime;
        public float LightAttackRecoverTime => _lightAttackRecoverTime;
        public float HeavyAttackRecoverTime => _heavyAttackRecoverTime;

        // 근접 공격 Properties
        public float ChargeSpeed => _chargeSpeed;
        public float RetreatDistance => _retreatDistance;

        // 전투 리듬
        public EAttackMode AttackMode => _attackMode;
        public bool EnablePushback => _enablePushback;
        public float LightAttackChance => _lightAttackChance;
        public float HeavyAttackChance => _heavyAttackChance;
        public float HeavyAttackCooldown => _heavyAttackCooldown;

        // 스트레이프 행동
        public float CircleAngularSpeed => _circleAngularSpeed;
        public float StrafeMinDuration => _strafeMinDuration;
        public float StrafeMaxDuration => _strafeMaxDuration;
        public float StrafePauseChance => _strafePauseChance;
        public float DirectionChangeChance => _directionChangeChance;
        public float SpeedLerpFactor => _speedLerpFactor;

        // 피격 반응
        public float KnockbackForce => _knockbackForce;

        // 행동 패턴
        public bool EnablePatrol => _enablePatrol;
        public bool EnableReturnHome => _enableReturnHome;
        public float PatrolRadius => _patrolRadius;
        public float PatrolWaitTimeMin => _patrolWaitTimeMin;
        public float PatrolWaitTimeMax => _patrolWaitTimeMax;
    }
}
