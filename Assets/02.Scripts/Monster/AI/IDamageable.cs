using UnityEngine;

namespace ProjectG.Monster
{
    /// <summary>
    /// 데미지를 받을 수 있는 객체를 정의하는 인터페이스.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// 데미지를 받는다.
        /// </summary>
        /// <param name="damage">받을 데미지 양</param>
        /// <param name="attackerPosition">공격자의 위치 (넉백 방향 계산용)</param>
        void TakeDamage(float damage, Vector3 attackerPosition);

        /// <summary>
        /// 현재 생존 여부를 반환한다.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 현재 체력을 반환한다.
        /// </summary>
        float CurrentHealth { get; }
    }
}
