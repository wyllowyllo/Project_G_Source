using System.Collections.Generic;
using UnityEngine;

namespace Monster
{
    /// <summary>
    /// 공격 슬롯을 관리하여 동시에 공격하는 몬스터 수를 제한하는 클래스.
    /// 난이도 조절과 가독성 확보를 위해 사용됩니다.
    /// </summary>
    public class AttackSlotManager
    {
        private readonly int _maxSlots;
        private readonly List<MonsterController> _activeSlots;

        public int MaxSlots => _maxSlots;
        public int ActiveSlotCount => _activeSlots.Count;
        public int AvailableSlots => _maxSlots - _activeSlots.Count;

        public AttackSlotManager(int maxSlots)
        {
            _maxSlots = Mathf.Max(1, maxSlots);
            _activeSlots = new List<MonsterController>(_maxSlots);
        }

        /// <summary>
        /// 공격 슬롯 요청
        /// </summary>
        /// <param name="monster">요청하는 몬스터</param>
        /// <returns>슬롯 획득 성공 여부</returns>
        public bool RequestSlot(MonsterController monster)
        {
            if (monster == null)
            {
                Debug.LogWarning("AttackSlotManager: null 몬스터가 슬롯을 요청했습니다.");
                return false;
            }

            // 이미 슬롯을 보유 중인 경우
            if (_activeSlots.Contains(monster))
            {
                return true;
            }

            // 슬롯이 가득 찬 경우
            if (_activeSlots.Count >= _maxSlots)
            {
                return false;
            }

            // 슬롯 할당
            _activeSlots.Add(monster);
            Debug.Log($"AttackSlotManager: {monster.name}에게 슬롯 할당. ({_activeSlots.Count}/{_maxSlots})");
            return true;
        }

        /// <summary>
        /// 공격 슬롯 반환
        /// </summary>
        /// <param name="monster">반환하는 몬스터</param>
        public void ReleaseSlot(MonsterController monster)
        {
            if (monster == null)
            {
                return;
            }

            if (_activeSlots.Remove(monster))
            {
                Debug.Log($"AttackSlotManager: {monster.name}의 슬롯 반환. ({_activeSlots.Count}/{_maxSlots})");
            }
        }

        /// <summary>
        /// 특정 몬스터가 슬롯을 보유하고 있는지 확인
        /// </summary>
        /// <param name="monster">확인할 몬스터</param>
        /// <returns>슬롯 보유 여부</returns>
        public bool HasSlot(MonsterController monster)
        {
            if (monster == null)
            {
                return false;
            }

            return _activeSlots.Contains(monster);
        }

        /// <summary>
        /// 모든 슬롯 초기화
        /// </summary>
        public void ClearAllSlots()
        {
            _activeSlots.Clear();
            Debug.Log("AttackSlotManager: 모든 슬롯 초기화");
        }

        /// <summary>
        /// 슬롯이 가득 찼는지 확인
        /// </summary>
        public bool IsFull()
        {
            return _activeSlots.Count >= _maxSlots;
        }

        /// <summary>
        /// 슬롯이 비어있는지 확인
        /// </summary>
        public bool IsEmpty()
        {
            return _activeSlots.Count == 0;
        }
    }
}
