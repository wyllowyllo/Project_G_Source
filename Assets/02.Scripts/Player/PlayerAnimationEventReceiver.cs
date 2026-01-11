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

        public void OnAttackAnimationEnd(AttackSession session)
        {
            if (_attacker != null && _attacker.CurrentSession != session)
                return;

            _attacker?.OnComboWindowStart();

            if (_playerCombat != null && _playerCombat.TryExecuteBufferedAttack())
                return;

            _playerCombat?.OnAttackComplete();
        }

        public void OnDodgeAnimationEnd()
        {
            _playerCombat?.OnDodgeAnimationEnd();
        }
    }
}
