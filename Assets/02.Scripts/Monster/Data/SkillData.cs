using UnityEngine;

namespace Monster.Data
{
    /// <summary>
    /// BDO 스타일 몬스터 스킬 패턴 데이터.
    /// 텔레그래프 → 실행 → 후딜 구조를 정의합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillData", menuName = "ProjectG/Monster/SkillData")]
    public class SkillData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string _skillName = "Basic Attack";
        [SerializeField] private string _skillDescription = "";

        [Header("타이밍 (BDO 스타일 3단계)")]
        [Tooltip("텔레그래프 시간 (준비 동작)")]
        [SerializeField] private float _windupTime = 0.3f;
        [Tooltip("실행 시간 (데미지 발생)")]
        [SerializeField] private float _executeTime = 0.2f;
        [Tooltip("후딜 시간 (회복)")]
        [SerializeField] private float _recoverTime = 0.5f;

        [Header("전투 데이터")]
        [SerializeField] private float _damage = 10f;
        [SerializeField] private float _range = 2f;
        [Tooltip("전방 각도 (0 = 원형 공격, 90 = 전방 90도)")]
        [SerializeField] private float _attackAngle = 90f;
        [SerializeField] private float _cooldown = 2f;

        [Header("애니메이션 (선택)")]
        [Tooltip("Animator 트리거 이름")]
        [SerializeField] private string _animationTrigger = "";

        // Properties
        public string SkillName => _skillName;
        public string SkillDescription => _skillDescription;
        public float WindupTime => _windupTime;
        public float ExecuteTime => _executeTime;
        public float RecoverTime => _recoverTime;
        public float Damage => _damage;
        public float Range => _range;
        public float AttackAngle => _attackAngle;
        public float Cooldown => _cooldown;
        public string AnimationTrigger => _animationTrigger;

        /// <summary>
        /// 스킬 전체 실행 시간
        /// </summary>
        public float TotalDuration => _windupTime + _executeTime + _recoverTime;
    }
}
