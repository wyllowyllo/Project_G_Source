using Monster.Combat;
using UnityEngine;

namespace Monster.AI
{
    // 애니메이션 이벤트를 수신하여 MonsterController에 전달하는 브릿지 컴포넌트
    // Animator가 있는 오브젝트(Body 하위의 프리팹)에 부착
    public class MonsterAnimationEventReceiver : MonoBehaviour
    {
        private MonsterController _controller;
        private MonsterAttacker _monsterAttacker;

        public void Initialize(MonsterController controller, MonsterAttacker monsterAttacker)
        {
            _controller = controller;
            _monsterAttacker = monsterAttacker;
        }

        // Animation Event: 공격 콜라이더 활성화
        public void EnableHitbox()
        {
            if (_monsterAttacker != null && _controller != null)
            {
                bool isHeavy = _controller.IsCurrentAttackHeavy();
                _monsterAttacker.EnableHitbox(isHeavy);
            }
        }

        // Animation Event: 공격 콜라이더 비활성화
        public void DisableHitbox()
        {
            _monsterAttacker?.DisableHitbox();
        }

        // Animation Event: 공격 애니메이션 완료
        public void OnAttackComplete()
        {
            _controller?.OnAttackAnimationComplete();
        }

        // Animation Event: 경계 애니메이션 완료
        public void OnAlertComplete()
        {
            _controller?.OnAlertAnimationComplete();
        }

        // Animation Event: 피격 애니메이션 완료
        public void OnHitComplete()
        {
            _controller?.OnHitAnimationComplete();
        }

        // Animation Event: 사망 애니메이션 완료
        public void OnDeathComplete()
        {
            _controller?.OnDeathAnimationComplete();
        }
    }
}
