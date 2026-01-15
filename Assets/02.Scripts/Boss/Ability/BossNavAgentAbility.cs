using UnityEngine;
using UnityEngine.AI;

namespace Boss.Ability
{
    public class BossNavAgentAbility : BossAbility
    {
        private NavMeshAgent _navAgent;
        private Transform _transform;

        public override void Initialize(AI.BossController controller)
        {
            base.Initialize(controller);
            _navAgent = controller.NavAgent;
            _transform = controller.transform;
        }

        public bool IsActive => _navAgent != null && _navAgent.isActiveAndEnabled;
        public bool IsStopped => _navAgent != null && _navAgent.isStopped;
        public Vector3 Velocity => _navAgent != null ? _navAgent.velocity : Vector3.zero;
        public bool HasPath => _navAgent != null && _navAgent.hasPath;
        public bool PathPending => _navAgent != null && _navAgent.pathPending;

        public void SetDestination(Vector3 destination)
        {
            if (IsActive)
            {
                _navAgent.SetDestination(destination);
            }
        }

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

        public float RemainingDistance()
        {
            if (!IsActive || !HasPath)
                return float.MaxValue;

            return _navAgent.remainingDistance;
        }

        public bool HasReachedDestination(float threshold = 0.1f)
        {
            if (!IsActive || !HasPath || PathPending)
                return false;

            return _navAgent.remainingDistance <= threshold;
        }

        public void SetSpeed(float speed)
        {
            if (_navAgent != null)
            {
                _navAgent.speed = speed;
            }
        }

        public float GetSpeed()
        {
            return _navAgent != null ? _navAgent.speed : 0f;
        }

        public void Disable()
        {
            if (_navAgent != null && _navAgent.isActiveAndEnabled)
            {
                _navAgent.isStopped = true;
                _navAgent.enabled = false;
            }
        }

        public void SetUpdateRotation(bool enabled)
        {
            if (_navAgent != null)
            {
                _navAgent.updateRotation = enabled;
            }
        }
    }
}
