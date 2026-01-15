using Common;
using UnityEngine;
using UnityEngine.AI;

namespace Monster.Ability
{
    // NavMeshAgent를 래핑하여 이동 기능을 제공하는 Ability
    // Monster와 Boss에서 공통으로 사용 가능
    public class NavAgentAbility : EntityAbility
    {
        private NavMeshAgent _navAgent;
        private Transform _transform;

        public override void Initialize(IEntityController controller)
        {
            base.Initialize(controller);
            _navAgent = controller.NavAgent;
            _transform = controller.transform;
        }

        // 프로퍼티: 상태 조회
        public bool IsActive => _navAgent != null && _navAgent.isActiveAndEnabled;
        public bool IsStopped => _navAgent != null && _navAgent.isStopped;
        public Vector3 Velocity => _navAgent != null ? _navAgent.velocity : Vector3.zero;
        public bool HasPath => _navAgent != null && _navAgent.hasPath;
        public bool PathPending => _navAgent != null && _navAgent.pathPending;

        // 이동 명령
        public void SetDestination(Vector3 destination)
        {
            if (IsActive)
            {
                _navAgent.SetDestination(destination);
            }
        }

        // 이동 정지/재개
        public void Stop()
        {
            if (_navAgent != null)
            {
                _navAgent.isStopped = true;
            }
        }

        public void Resume()
        {
            if (_navAgent != null)
            {
                _navAgent.isStopped = false;
            }
        }

        // 목적지까지의 남은 거리
        public float RemainingDistance()
        {
            if (!IsActive || !HasPath)
                return float.MaxValue;

            return _navAgent.remainingDistance;
        }

        // 목적지에 도달했는지 여부
        public bool HasReachedDestination(float threshold = 0.1f)
        {
            if (!IsActive || !HasPath || PathPending)
                return false;

            return _navAgent.remainingDistance <= threshold;
        }

        // NavAgent 속도 설정
        public void SetSpeed(float speed)
        {
            if (_navAgent != null)
            {
                _navAgent.speed = speed;
            }
        }

        // 현재 속도 가져오기
        public float GetSpeed()
        {
            return _navAgent != null ? _navAgent.speed : 0f;
        }

        // NavAgent 완전히 비활성화 (사망 시 사용)
        public void Disable()
        {
            if (_navAgent != null && _navAgent.isActiveAndEnabled)
            {
                _navAgent.isStopped = true;
                _navAgent.enabled = false;
            }
        }

        // NavAgent 자동 회전 설정 (FacingAbility와 충돌 방지용)
        public void SetUpdateRotation(bool enabled)
        {
            if (_navAgent != null)
            {
                _navAgent.updateRotation = enabled;
            }
        }
    }
}