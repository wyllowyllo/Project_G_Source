using UnityEngine;

namespace Monster.AI
{
    // 애니메이션 이벤트를 수신하여 MonsterController에 전달하는 브릿지 컴포넌트
    // Animator가 있는 오브젝트(Body 하위의 프리팹)에 부착
    public class MonsterAnimationEventReceiver : MonoBehaviour
    {
        private MonsterController _controller;
        private Collider _attackCollider;

        public void Initialize(MonsterController controller, Collider attackCollider)
        {
            _controller = controller;
            _attackCollider = attackCollider;

            if (_attackCollider != null)
            {
                _attackCollider.enabled = false;
            }
        }

        // Animation Event: 공격 콜라이더 활성화
        public void EnableHitbox()
        {
            if (_attackCollider != null)
            {
                _attackCollider.enabled = true;
            }
        }

        // Animation Event: 공격 콜라이더 비활성화
        public void DisableHitbox()
        {
            if (_attackCollider != null)
            {
                _attackCollider.enabled = false;
            }
        }

        // Animation Event: 공격 애니메이션 완료
        public void OnAttackComplete()
        {
            _controller?.OnAttackAnimationComplete();
        }
    }
}
