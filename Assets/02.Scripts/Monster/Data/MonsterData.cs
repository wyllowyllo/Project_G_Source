using UnityEngine;

namespace ProjectG.Monster
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
    }
}
