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

        private float _slotRequestCooldown = 5f;
        private float _slotRequestTimer = 0f;

        // 스트레이프 관련
        private float _strafeDirectionChangeInterval = 2f;
        private float _strafeDirectionTimer = 0f;
        private int _strafeDirection = 1; // 1 = 오른쪽, -1 = 왼쪽

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

            float distanceToPlayer = Vector3.Distance(_transform.position, _controller.PlayerTransform.position);
            
            
            if (distanceToPlayer > _controller.Data.PreferredMaxDistance)
            {
              
                _stateMachine.ChangeState(EMonsterState.Approach);
                return;
            }
            // 공격권을 그룹이 배정하면, 슬롯 보유 시 Attack으로 전환
            if (_controller.EnemyGroup != null && _controller.EnemyGroup.CanAttack(_controller))
            {
                _stateMachine.ChangeState(EMonsterState.Attack);
                return;
            }

            // DesiredPosition (각도 슬롯 + separation)
            Vector3 desired = (_controller.EnemyGroup != null)
                ? _controller.EnemyGroup.GetDesiredPosition(_controller)
                : _controller.PlayerTransform.position;

            // Probe 진행
            _modeTimer += Time.deltaTime;

            switch (_mode)
            {
                case EProbeMode.Reposition:
                    DoReposition(desired);
                    break;
                case EProbeMode.Hold:
                    DoHold();
                    break;
                case EProbeMode.Shuffle:
                    DoShuffle(desired);
                    break;
                case EProbeMode.FeintIn:
                    DoFeint(desired, towardPlayer: true);
                    break;
                case EProbeMode.FeintOut:
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
            if (_controller.NavAgent == null || !_controller.NavAgent.isActiveAndEnabled) return;

            float d = Vector3.Distance(_transform.position, desired);

            // 목표 근처 도달: 멈칫/셔플로 전환(“눈치보기” 시작)
            if (d <= RepositionStopRadius)
            {
                PickMode(Random.value < 0.55f ? EProbeMode.Hold : EProbeMode.Shuffle, 0.25f, 0.6f);
                return;
            }

            _controller.NavAgent.isStopped = false;
            _controller.NavAgent.SetDestination(desired);
        }

        private void DoHold()
        {
            if (_controller.NavAgent != null) _controller.NavAgent.isStopped = true;
            LookAtPlayerSoft();
        }

        private void DoShuffle(Vector3 desired)
        {
            if (_controller.NavAgent == null || !_controller.NavAgent.isActiveAndEnabled) return;

            // 목표 자리 근방에서 작은 원호 이동(“슬금슬금”)
            if (_modeTimer <= 0.01f)
            {
                Vector3 center = desired;
                Vector3 toSelf = (_transform.position - center);
                toSelf.y = 0f;
                if (toSelf.sqrMagnitude < 0.01f) toSelf = _transform.right;
                toSelf.Normalize();

                Vector3 tangent = Vector3.Cross(Vector3.up, toSelf) * (Random.value < 0.5f ? 1f : -1f);
                _probeTarget = center + (toSelf * ShuffleRadius) + (tangent * (ShuffleRadius * 0.6f));
                _probeTarget.y = _transform.position.y;
            }

            _controller.NavAgent.isStopped = false;
            _controller.NavAgent.SetDestination(_probeTarget);

            LookAtPlayerSoft();
        }

        private void DoFeint(Vector3 desired, bool towardPlayer)
        {
            if (_controller.NavAgent == null || !_controller.NavAgent.isActiveAndEnabled) return;

            if (_modeTimer <= 0.01f)
            {
                Vector3 dir = (_controller.PlayerTransform.position - _transform.position);
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.01f) dir = _transform.forward;
                dir.Normalize();

                if (!towardPlayer) dir = -dir;

                _probeTarget = _transform.position + dir * FeintStep;
                _probeTarget.y = _transform.position.y;
            }

            _controller.NavAgent.isStopped = false;
            _controller.NavAgent.SetDestination(_probeTarget);

            LookAtPlayerSoft();
        }

        private void ChooseNextProbeMode(Vector3 desired)
        {
            // 목표로 다시 조금 재배치할 필요가 있으면 Reposition
            float d = Vector3.Distance(_transform.position, desired);
            if (d > RepositionStopRadius * 1.25f)
            {
                PickMode(EProbeMode.Reposition, 0.2f, 0.35f);
                return;
            }

            // 목표 근처면 Hold/Shuffle/Feint를 섞어 “간보기”
            float r = Random.value;
            if (r < 0.50f) PickMode(EProbeMode.Hold, 0.25f, 0.65f);
            else if (r < 0.80f) PickMode(EProbeMode.Shuffle, 0.35f, 0.75f);
            else if (r < 0.90f) PickMode(EProbeMode.FeintIn, 0.20f, 0.45f);
            else PickMode(EProbeMode.FeintOut, 0.20f, 0.45f);
        }

        private void PickMode(EProbeMode mode, float minDur, float maxDur)
        {
            _mode = mode;
            _modeTimer = 0f;
            _modeDuration = Random.Range(minDur, maxDur);
        }

        private void LookAtPlayerSoft()
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
