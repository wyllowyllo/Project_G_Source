using Monster.Data;
using UnityEngine;

namespace Monster.AI.States
{
    // 스트레이프 상태: 거리 밴드 안에서 플레이어를 압박하며 좌우로 이동
    public class StrafeState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly GroupCommandProvider _groupCommandProvider;
        private readonly Transform _transform;
        
        // Probe 서브모드
        private enum EProbeMode { Reposition, Hold, Shuffle, FeintIn, FeintOut }

        private EProbeMode _mode;
        private float _modeTimer;
        private float _modeDuration;

        // 셔플/페이크용
        private Vector3 _probeTarget;

        // 프로퍼티
        public EMonsterState StateType => EMonsterState.Strafe;

        public StrafeState(MonsterController controller, MonsterStateMachine stateMachine, GroupCommandProvider groupCommandProvider)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _groupCommandProvider = groupCommandProvider;
            _transform = controller.transform;
        }

        public void Enter()
        {
            EnableNavigation();

            float distanceToPlayer = Vector3.Distance(_transform.position, _controller.PlayerTransform.position);
            EnsureMinimumDistance(distanceToPlayer);
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

            Vector3 desired = _groupCommandProvider.GetDesiredPosition();

            // AttackMode에 따라 공격 타입 결정
            EAttackMode attackMode = _controller.Data.AttackMode;

            // 약공 실행 시도 (Both 또는 LightOnly 모드일 때만)
            if (attackMode == EAttackMode.Both || attackMode == EAttackMode.LightOnly)
            {
                if (TryExecuteLightAttack(desired, distanceToPlayer, now))
                {
                    return;
                }
            }

            // 강공 실행 시도 (Both 또는 HeavyOnly 모드일 때만)
            if (attackMode == EAttackMode.Both || attackMode == EAttackMode.HeavyOnly)
            {
                if (TryExecuteHeavyAttack(now))
                {
                    return;
                }
            }


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
            if (!_controller.NavAgent.isActiveAndEnabled) return;

            float dist = Vector3.Distance(_transform.position, desired);

            // 목표 근처 도달: 멈칫 or 셔플로 전환("눈치보기" 시작)
            if (dist <= _controller.Data.RepositionStopRadius)
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
                float shuffleRadius = _controller.Data.ShuffleRadius;
                _probeTarget = center + (toSelf * shuffleRadius) + (tangent * (shuffleRadius * 0.6f));
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

                _probeTarget = _transform.position + dir * _controller.Data.FeintStep;
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
            if (distance > _controller.Data.RepositionStopRadius * 1.25f)
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

        private void EnableNavigation()
        {
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = false;
            }
        }

        private void EnsureMinimumDistance(float distanceToPlayer)
        {
            if (distanceToPlayer < _controller.Data.PreferredMinDistance)
            {
                Vector3 dirAway = (_transform.position - _controller.PlayerTransform.position);
                dirAway.y = 0f;
                dirAway.Normalize();

                float backoffDistance = _controller.Data.PreferredMinDistance - distanceToPlayer + 0.5f;
                _probeTarget = _transform.position + dirAway * backoffDistance;
                _probeTarget.y = _transform.position.y;

                // Cascading push-back 요청 (옵션이 활성화된 경우에만)
                if (_controller.Data.EnablePushback)
                {
                    _groupCommandProvider.RequestPushback(dirAway, backoffDistance);
                }

                PickMode(EProbeMode.FeintOut, 0.2f, 0.3f);
            }
            else
            {
                PickMode(EProbeMode.Reposition, 0.2f, 0.3f);
            }
        }

        private bool TryExecuteLightAttack(Vector3 desired, float distanceToPlayer, float now)
        {
            float distToDesired = Vector3.Distance(_transform.position, desired);
            float lightRange = _controller.Data.AttackRange + 0.35f;

            if (distToDesired <= 1.0f && distanceToPlayer <= lightRange && _groupCommandProvider.CanLightAttack(now) && Random.value < _controller.Data.LightAttackChance)
            {
                _groupCommandProvider.SetNextAttackHeavy(false);
                _groupCommandProvider.ConsumeLightAttack(now, _controller.Data.AttackCooldown);
                _stateMachine.ChangeState(EMonsterState.Attack);
                return true;
            }

            return false;
        }

        private bool TryExecuteHeavyAttack(float now)
        {
            if (_groupCommandProvider.CanAttack() && _groupCommandProvider.CanHeavyAttack(now)
                && Random.value < _controller.Data.HeavyAttackChance)
            {
                _groupCommandProvider.SetNextAttackHeavy(true);
                _groupCommandProvider.ConsumeHeavyAttack(now, _controller.Data.HeavyAttackCooldown);
                _stateMachine.ChangeState(EMonsterState.Attack);
                return true;
            }

            return false;
        }
    }
}
