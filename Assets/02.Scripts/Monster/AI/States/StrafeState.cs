using UnityEngine;

namespace Monster.AI.States
{
    /// <summary>
    /// 스트레이프 상태.
    /// 거리 밴드 안에서 플레이어를 압박하며 좌우로 이동합니다.
    /// </summary>
    public class StrafeState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;
        
        // Probe 서브모드
        private enum EProbeMode { Reposition, Hold, Shuffle, FeintIn, FeintOut }

        private EProbeMode _mode;
        private float _modeTimer;
        private float _modeDuration;

        // 셔플/페이크용
        private Vector3 _probeTarget;

        // 튜닝
        private const float RepositionStopRadius = 1.3f;   // 목표 근처면 멈칫 모드로 전환
        private const float ShuffleRadius = 1.2f;
        private const float FeintStep = 0.8f;
        
        // 프로퍼티
        public EMonsterState StateType => EMonsterState.Strafe;

        public StrafeState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;
        }

        public void Enter()
        {
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = false;
            }
            
            PickMode(EProbeMode.Reposition, 0.2f, 0.3f);
        }

        public void Update()
        {
            float now = Time.time;
            float distanceToPlayer = Vector3.Distance(_transform.position, _controller.PlayerTransform.position);


            if (distanceToPlayer > _controller.Data.PreferredMaxDistance)
            {
                _stateMachine.ChangeState(EMonsterState.Approach);
                return;
            }

            // BDO식 전투 리듬
            // 1) 약공: 슬롯 없이 자주
            // 2) 강공: 슬롯(강공권) 있을 때만
            
            // DesiredPosition (각도 슬롯 + separation)
            Vector3 desired = _controller.EnemyGroup.GetDesiredPosition(_controller);
            float distToDesired = Vector3.Distance(_transform.position, desired);
            
            float lightRange = _controller.Data.AttackRange + 0.35f;
            if (distToDesired <= 1.0f && distanceToPlayer <= lightRange && _controller.CanLightAttack(now) && Random.value < _controller.Data.LightAttackChance)
            {
                _controller.SetNextAttackHeavy(false);
                _controller.ConsumeLightAttack(now, _controller.Data.AttackCooldown);
                _stateMachine.ChangeState(EMonsterState.Attack);
                return;
            }

            // 강공은 강공권이 있을 때만 시도
            if (_controller.EnemyGroup != null && _controller.EnemyGroup.CanAttack(_controller)
                && _controller.CanHeavyAttack(now) && Random.value < _controller.Data.HeavyAttackChance)
            {
                _controller.SetNextAttackHeavy(true);
                _controller.ConsumeHeavyAttack(now, _controller.Data.HeavyAttackCooldown);
                _stateMachine.ChangeState(EMonsterState.Attack);
                return;
            }

            
            
            _modeTimer += Time.deltaTime;

            switch (_mode)
            {
                case EProbeMode.Reposition: // 자리 목표 변경 시 이동
                    DoReposition(desired);
                    break;
                case EProbeMode.Hold: // 이동 정지 + 플레이어 주시
                    DoHold();
                    break;
                case EProbeMode.Shuffle: // 자리 목표 주변에서 옆걸음
                    DoShuffle(desired);
                    break;
                case EProbeMode.FeintIn: // 반걸음 전진
                    DoFeint(desired, towardPlayer: true);
                    break;
                case EProbeMode.FeintOut: // 반걸음 후퇴
                    DoFeint(desired, towardPlayer: false);
                    break;
            }

            if (_modeTimer >= _modeDuration)
            {
                ChooseNextProbeMode(desired);
            }
        }

       private void DoReposition(Vector3 desired)
        {
            if (!_controller.NavAgent.isActiveAndEnabled) return;

            float dist = Vector3.Distance(_transform.position, desired);

            // 목표 근처 도달: 멈칫 or 셔플로 전환(“눈치보기” 시작)
            if (dist <= RepositionStopRadius)
            {
                PickMode(Random.value < 0.55f ? EProbeMode.Hold : EProbeMode.Shuffle, 0.25f, 0.6f);
                return;
            }

            _controller.NavAgent.isStopped = false;
            _controller.NavAgent.SetDestination(desired);
        }

        private void DoHold()
        {
            _controller.NavAgent.isStopped = true;
            LookAtTarget();
        }

        private void DoShuffle(Vector3 desired)
        {
            if (!_controller.NavAgent.isActiveAndEnabled) return;

            // 목표 자리 근방에서 작은 원호 이동(“슬금슬금”)
            if (_modeTimer <= 0.01f)
            {
                Vector3 center = desired;
                Vector3 toSelf = (_transform.position - center);
                toSelf.y = 0f;
                toSelf.Normalize();

                Vector3 tangent = Vector3.Cross(Vector3.up, toSelf) * (Random.value < 0.5f ? 1f : -1f);
                _probeTarget = center + (toSelf * ShuffleRadius) + (tangent * (ShuffleRadius * 0.6f));
                _probeTarget.y = _transform.position.y;
            }

            _controller.NavAgent.isStopped = false;
            _controller.NavAgent.SetDestination(_probeTarget);

            LookAtTarget();
        }

        private void DoFeint(Vector3 desired, bool towardPlayer)
        {
            if (!_controller.NavAgent.isActiveAndEnabled) return;

            if (_modeTimer <= 0.01f)
            {
                Vector3 dir = (_controller.PlayerTransform.position - _transform.position);
                dir.y = 0f;
                dir.Normalize();

                if (!towardPlayer) dir = -dir;

                _probeTarget = _transform.position + dir * FeintStep;
                _probeTarget.y = _transform.position.y;
            }

            _controller.NavAgent.isStopped = false;
            _controller.NavAgent.SetDestination(_probeTarget);

            LookAtTarget();
        }

        private void ChooseNextProbeMode(Vector3 desired)
        {
            // 목표로 다시 조금 재배치할 필요가 있으면 Reposition
            float distance = Vector3.Distance(_transform.position, desired);
            if (distance > RepositionStopRadius * 1.25f)
            {
                PickMode(EProbeMode.Reposition, 0.2f, 0.35f);
                return;
            }

            // 목표 근처면 Hold/Shuffle/Feint를 섞어 "간보기" (짧게 조정)
            float r = Random.value;
            if (r < 0.50f) PickMode(EProbeMode.Hold, 0.15f, 0.35f);       // 단축
            else if (r < 0.80f) PickMode(EProbeMode.Shuffle, 0.2f, 0.45f); // 단축
            else if (r < 0.90f) PickMode(EProbeMode.FeintIn, 0.15f, 0.3f); // 단축
            else PickMode(EProbeMode.FeintOut, 0.15f, 0.3f);               // 단축
        }

        private void PickMode(EProbeMode mode, float minDur, float maxDur)
        {
            _mode = mode;
            _modeTimer = 0f;
            _modeDuration = Random.Range(minDur, maxDur);
        }

        private void LookAtTarget()
        {
            Vector3 dir = (_controller.PlayerTransform.position - _transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;

            Quaternion target = Quaternion.LookRotation(dir.normalized);
            _transform.rotation = Quaternion.RotateTowards(
                _transform.rotation,
                target,
                _controller.Data.RotationSpeed * Time.deltaTime
            );
        }
       

        public void Exit()
        {
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = false;
            }
        }
    }
}
