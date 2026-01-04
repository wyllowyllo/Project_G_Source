using UnityEngine;

namespace Monster
{
    /// <summary>
    /// 몬스터의 공격 상태.
    /// 플레이어를 공격하고, 공격 범위를 벗어나거나 슬롯을 잃으면 Engage 상태로 복귀합니다.
    /// 알파: 공격 슬롯 시스템과 연동
    /// </summary>
    public class AttackState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        private float _attackTimer;

        public MonsterState StateType => MonsterState.Attack;

        public AttackState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;
        }

        public void Enter()
        {
            // 공격 시작 시 이동 정지
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = true;
            }

            _attackTimer = 0f;
        }

        public void Update()
        {
            if (_controller.PlayerTransform == null)
            {
                _stateMachine.ChangeState(MonsterState.Idle);
                return;
            }

            // 알파: 공격 슬롯을 잃으면 Engage로 복귀
            if (_controller.EnemyGroup != null && !_controller.EnemyGroup.CanAttack(_controller))
            {
                _stateMachine.ChangeState(MonsterState.Engage);
                return;
            }

            float distanceToPlayer = Vector3.Distance(
                _transform.position,
                _controller.PlayerTransform.position
            );

            // 공격 범위를 벗어나면 Engage로 복귀
            if (distanceToPlayer > _controller.Data.AttackRange)
            {
                _stateMachine.ChangeState(MonsterState.Engage);
                return;
            }

            // 플레이어를 바라보기
            Vector3 directionToPlayer = (_controller.PlayerTransform.position - _transform.position).normalized;
            directionToPlayer.y = 0f;

            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                _transform.rotation = Quaternion.RotateTowards(
                    _transform.rotation,
                    targetRotation,
                    _controller.Data.RotationSpeed * Time.deltaTime
                );
            }

            // 공격 쿨타임 체크
            _attackTimer += Time.deltaTime;

            if (_attackTimer >= _controller.Data.AttackCooldown)
            {
                PerformAttack();
                _attackTimer = 0f;
            }
        }

        public void Exit()
        {
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = false;
            }

            // 알파: 공격 슬롯 반환
            if (_controller.EnemyGroup != null)
            {
                _controller.EnemyGroup.ReleaseAttackSlot(_controller);
            }
        }

        private void PerformAttack()
        {
            // TODO: 애니메이션 트리거
            // TODO: 플레이어에게 데미지 처리

            Debug.Log($"{_controller.gameObject.name} 공격! 데미지: {_controller.Data.AttackDamage}");

            // 임시: 플레이어가 IDamageable을 구현했다면 데미지 적용
            if (_controller.PlayerTransform.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(_controller.Data.AttackDamage, _transform.position);
            }
        }
    }
}
