using System.Collections.Generic;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
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

        // 프로퍼티
        public Vector3 GroupCenter => _groupCenter;
        public int MonsterCount => _monsters.Count;
        public Transform PlayerTransform => _playerTransform;

        private void Awake()
        {
            InitializeGroup();
        }

        private void Update()
        {
            // EnemyGroup은 공격 슬롯만 관리
            // Aggro 감지는 각 몬스터의 IdleState에서 개별적으로 처리 (업계 표준)
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
