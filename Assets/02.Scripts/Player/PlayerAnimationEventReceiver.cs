using Combat.Attack;
using UnityEngine;

namespace Player
{
    public class PlayerAnimationEventReceiver : MonoBehaviour
    {
        private MeleeAttacker _attacker;
        private PlayerCombat _playerCombat;

        private void Awake()
        {
            _attacker = GetComponent<MeleeAttacker>();
            _playerCombat = GetComponent<PlayerCombat>();

            if (_attacker == null)
            {
                Debug.LogError("[PlayerAnimationEventReceiver] MeleeAttacker component 필요");
            }
            if (_playerCombat == null)
            {
                Debug.LogError("[PlayerAnimationEventReceiver] PlayerCombat component 필요");
            }
        }
        
        public void OnAttackHitStart()
        {
            _attacker?.OnAttackHitStart();
        }
        
        public void OnAttackHitEnd()
        {
            _attacker?.OnAttackHitEnd();
        }
        
        public void OnAttackAnimationEnd()
        {
            _attacker?.OnComboWindowStart();

            // 버퍼된 입력이 있으면 다음 공격 시작
            if (_playerCombat != null && _playerCombat.TryExecuteBufferedAttack())
                return;

            // 버퍼 없으면 공격 종료 처리
            _playerCombat?.OnAttackComplete();
        }
    }
}
