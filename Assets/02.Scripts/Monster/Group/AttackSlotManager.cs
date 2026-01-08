using System.Collections.Generic;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    // 공격 슬롯을 관리하여 동시에 공격하는 몬스터 수를 제한
    public class AttackSlotManager
    {
        private readonly int _maxSlots;
        private readonly List<MonsterController> _activeSlots;

        public int MaxSlots => _maxSlots;
        public int ActiveSlotCount => _activeSlots.Count;
        public int AvailableSlots => _maxSlots - _activeSlots.Count;

        public AttackSlotManager(int maxSlots)
        {
            _maxSlots = Mathf.Max(0, maxSlots);
            _activeSlots = new List<MonsterController>(_maxSlots);
        }

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

        public bool HasSlot(MonsterController monster)
        {
            if (monster == null)
            {
                return false;
            }

            return _activeSlots.Contains(monster);
        }

        public void ClearAllSlots()
        {
            _activeSlots.Clear();
            Debug.Log("AttackSlotManager: 모든 슬롯 초기화");
        }

        public bool IsFull()
        {
            return _activeSlots.Count >= _maxSlots;
        }

        public bool IsEmpty()
        {
            return _activeSlots.Count == 0;
        }
    }
}
