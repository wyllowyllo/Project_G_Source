using Monster.Group;
using UnityEngine;

namespace Monster.AI
{
    public class GroupCommandProvider
    {
        private readonly MonsterController _owner;
        private MonsterGroupDirector _monsterGroupDirector;

        // 개체 쿨다운/공격 타입
        private float _nextLightAttackTime;
        private float _nextHeavyAttackTime;
        private bool _nextAttackIsHeavy;
        private bool _currentAttackWasHeavy;

        // 프로퍼티
        public bool NextAttackIsHeavy => _nextAttackIsHeavy;
        public bool CurrentAttackWasHeavy => _currentAttackWasHeavy;

        public GroupCommandProvider(MonsterController owner)
        {
            _owner = owner;
            _nextLightAttackTime = 0f;
            _nextHeavyAttackTime = 0f;
        }

        // ===== EnemyGroup 상호작용 메서드 =====

        // EnemyGroup 설정
        public void SetEnemyGroup(MonsterGroupDirector groupDirector)
        {
            _monsterGroupDirector = groupDirector;
        }
        
        // 공격 슬롯 반환 (EnemyGroup에 위임)
        public void ReleaseAttackSlot()
        {
            _monsterGroupDirector?.ReleaseAttackSlot(_owner);
        }

        // 공격 가능 여부 확인 (EnemyGroup에 위임)
        public bool CanAttack()
        {
            return _monsterGroupDirector?.CanAttack(_owner) ?? false;
        }

        // 원하는 위치 가져오기 (EnemyGroup에 위임)
        public Vector3 GetDesiredPosition()
        {
            return _monsterGroupDirector?.GetDesiredPosition(_owner) ?? _owner.transform.position;
        }

        // 후퇴 시 cascading push-back 요청 (EnemyGroup에 위임)
        public void RequestPushback(Vector3 retreatDirection, float distance)
        {
            _monsterGroupDirector?.RequestPushback(_owner, retreatDirection, distance);
        }

        // EnemyGroup에서 몬스터 등록 해제 (EnemyGroup에 위임)
        public void UnregisterFromGroup()
        {
            _monsterGroupDirector?.UnregisterMonster(_owner);
        }

        // ===== 공격 쿨다운 관리 메서드 =====

        public bool CanLightAttack(float now)
        {
            return now >= _nextLightAttackTime;
        }

        public bool CanHeavyAttack(float now)
        {
            return now >= _nextHeavyAttackTime;
        }

        public void ConsumeLightAttack(float now, float cd)
        {
            _nextLightAttackTime = now + Mathf.Max(0.01f, cd);
        }

        public void ConsumeHeavyAttack(float now, float cd)
        {
            _nextHeavyAttackTime = now + Mathf.Max(0.01f, cd);
        }

        public void SetNextAttackHeavy(bool heavy)
        {
            _nextAttackIsHeavy = heavy;
        }

        public void MarkCurrentAttackHeavy(bool heavy)
        {
            _currentAttackWasHeavy = heavy;
        }

        public void ClearCurrentAttackHeavy()
        {
            _currentAttackWasHeavy = false;
        }
    }
}