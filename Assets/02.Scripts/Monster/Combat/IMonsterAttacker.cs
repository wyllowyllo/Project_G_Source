using System;
using Combat.Core;
using Combat.Damage;

namespace Monster.Combat
{
    // 몬스터 공격자 인터페이스 (근접/원거리 공통)
    public interface IMonsterAttacker
    {
        void Initialize();
        void ExecuteAttack(bool isHeavy);
        void CancelAttack();
        bool IsAttacking { get; }
        event Action<IDamageable, DamageInfo> OnHit;
    }
}
