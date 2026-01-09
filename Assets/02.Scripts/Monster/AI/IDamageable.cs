using UnityEngine;

namespace Monster.AI
{
    // 데미지를 받을 수 있는 객체를 정의하는 인터페이스
    public interface IDamageable
    {
        
        void TakeDamage(float damage, Vector3 attackerPosition);

      
        bool IsAlive { get; }

       
        float CurrentHealth { get; }
    }
}
