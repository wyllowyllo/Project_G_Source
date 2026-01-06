using System.Collections.Generic;
using UnityEngine;

namespace Monster
{
    /// <summary>
    /// 몬스터 그룹의 전투를 운영하는 핵심 클래스.
    /// Aggro 관리, 공격 슬롯 분배를 담당합니다. (핵앤슬래시 스타일)
    /// </summary>
    public class EnemyGroup : MonoBehaviour
    {
        [Header("그룹 설정")]
        [SerializeField] private Vector3 _groupCenter;
        [SerializeField] private float _aggroRange = 12f;
        [SerializeField] private int _maxAttackSlots = 2;

        [Header("디버그")]
        [SerializeField] private bool _showDebugGizmos = true;

        // 참조
        private Transform _playerTransform;
        private AttackSlotManager _attackSlotManager;

        // 그룹 상태
        private List<MonsterController> _monsters = new List<MonsterController>();
        private bool _isInCombat = false;

        // 프로퍼티
        public Vector3 GroupCenter => _groupCenter;
        public bool IsInCombat => _isInCombat;
        public int MonsterCount => _monsters.Count;
        public Transform PlayerTransform => _playerTransform;

        private void Awake()
        {
            InitializeGroup();
        }

        private void Update()
        {
            if (_monsters.Count == 0)
            {
                return;
            }

            // 전투 상태 체크 및 리셋
            if (_isInCombat)
            {
                CheckCombatReset();
            }

            // Aggro 체크
            if (!_isInCombat)
            {
                CheckAggro();
            }
        }

        private void InitializeGroup()
        {
            // 그룹 센터가 설정되지 않았으면 현재 위치 사용
            if (_groupCenter == Vector3.zero)
            {
                _groupCenter = transform.position;
            }

            // AttackSlotManager 생성
            _attackSlotManager = new AttackSlotManager(_maxAttackSlots);
        }

        /// <summary>
        /// 몬스터를 그룹에 등록
        /// </summary>
        public void RegisterMonster(MonsterController monster)
        {
            if (monster == null)
            {
                Debug.LogWarning("EnemyGroup: 등록하려는 몬스터가 null입니다.");
                return;
            }

            if (!_monsters.Contains(monster))
            {
                _monsters.Add(monster);
                Debug.Log($"EnemyGroup: {monster.name} 등록됨. 현재 그룹 크기: {_monsters.Count}");
            }
        }

        /// <summary>
        /// 몬스터를 그룹에서 제거
        /// </summary>
        public void UnregisterMonster(MonsterController monster)
        {
            if (monster == null)
            {
                return;
            }

            if (_monsters.Remove(monster))
            {
                // 공격 슬롯 반환
                _attackSlotManager?.ReleaseSlot(monster);
                Debug.Log($"EnemyGroup: {monster.name} 제거됨. 현재 그룹 크기: {_monsters.Count}");
            }
        }

        /// <summary>
        /// 플레이어를 찾아서 설정
        /// </summary>
        public void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("EnemyGroup: Player를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// Aggro 범위 체크 - 플레이어가 범위 안에 들어오면 전투 시작
        /// </summary>
        private void CheckAggro()
        {
            if (_playerTransform == null)
            {
                FindPlayer();
                return;
            }

            float distanceToPlayer = Vector3.Distance(_groupCenter, _playerTransform.position);

            if (distanceToPlayer <= _aggroRange)
            {
                TransitionToCombat();
            }
        }

        /// <summary>
        /// BDO 스타일 - 그룹 전체를 전투 상태로 전환
        /// </summary>
        private void TransitionToCombat()
        {
            if (_isInCombat)
            {
                return;
            }

            _isInCombat = true;
            Debug.Log($"EnemyGroup: 전투 시작! 그룹 크기: {_monsters.Count}");

            // 모든 몬스터를 Approach 상태로 전환 (BDO 스타일)
            foreach (MonsterController monster in _monsters)
            {
                if (monster != null && monster.IsAlive)
                {
                    monster.StateMachine.ChangeState(EMonsterState.Approach);
                }
            }
        }

        /// <summary>
        /// 전투 상태 리셋 체크 - 모든 몬스터가 비전투 상태면 전투 종료
        /// </summary>
        private void CheckCombatReset()
        {
            if (!_isInCombat)
            {
                return;
            }

            // 모든 몬스터가 Idle 또는 Dead 상태인지 확인
            bool allMonstersNonCombat = true;

            foreach (MonsterController monster in _monsters)
            {
                if (monster == null || !monster.IsAlive)
                {
                    continue;
                }

                EMonsterState currentState = monster.StateMachine.CurrentStateType;

                // 전투 관련 상태가 하나라도 있으면 계속 전투 중
                if (currentState != EMonsterState.Idle && currentState != EMonsterState.Dead)
                {
                    allMonstersNonCombat = false;
                    break;
                }
            }

            // 모든 몬스터가 비전투 상태면 전투 종료
            if (allMonstersNonCombat)
            {
                _isInCombat = false;
                Debug.Log($"EnemyGroup: 전투 종료 - 모든 몬스터가 비전투 상태");
            }
        }


        /// <summary>
        /// 공격 슬롯 요청
        /// </summary>
        public bool RequestAttackSlot(MonsterController monster)
        {
            if (_attackSlotManager == null)
            {
                return false;
            }

            return _attackSlotManager.RequestSlot(monster);
        }

        /// <summary>
        /// 공격 슬롯 반환
        /// </summary>
        public void ReleaseAttackSlot(MonsterController monster)
        {
            _attackSlotManager?.ReleaseSlot(monster);
        }

        /// <summary>
        /// 공격 가능 여부 확인
        /// </summary>
        public bool CanAttack(MonsterController monster)
        {
            if (_attackSlotManager == null)
            {
                return false;
            }

            return _attackSlotManager.HasSlot(monster);
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos)
            {
                return;
            }

            // 그룹 센터
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_groupCenter, 0.5f);

            // Aggro 범위
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_groupCenter, _aggroRange);
        }
    }
}
