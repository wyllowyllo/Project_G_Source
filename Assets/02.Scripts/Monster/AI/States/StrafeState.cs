using Monster.Ability;
using Monster.Data;
using UnityEngine;

namespace Monster.AI.States
{
    // 스트레이프 상태: 플레이어 주변을 자연스럽게 배회하며 공격 기회를 엿봄
    public class StrafeState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly GroupCommandProvider _groupCommandProvider;
        private readonly Transform _transform;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly PlayerDetectAbility _playerDetectAbility;
        private readonly FacingAbility _facingAbility;
        private readonly AnimatorAbility _animatorAbility;

        // 행동 모드
        private enum EStrafeMode
        {
            Circle,           // 플레이어 주변을 원호로 이동 (서성이기)
            AdjustDistance,   // 거리 조절 (전진/후퇴)
            Pause,            // 잠시 정지하고 관찰
            ApproachForAttack // 일반공격을 위해 밀착 거리까지 접근
        }

        private EStrafeMode _currentMode;
        private float _modeTimer;
        private float _modeDuration;

        // 원호 이동용
        private int _circleDirection; // 1: 시계방향, -1: 반시계방향

        // 부드러운 이동을 위한 보간
        private Vector3 _currentVelocity;
        private Vector3 _targetVelocity;

        // 캐시
        private MonsterData Data => _controller.Data;

        public EMonsterState StateType => EMonsterState.Strafe;

        public StrafeState(MonsterController controller, MonsterStateMachine stateMachine, GroupCommandProvider groupCommandProvider)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _groupCommandProvider = groupCommandProvider;
            _transform = controller.transform;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _playerDetectAbility = controller.GetAbility<PlayerDetectAbility>();
            _facingAbility = controller.GetAbility<FacingAbility>();
            _animatorAbility = controller.GetAbility<AnimatorAbility>();
        }

        public void Enter()
        {
            _animatorAbility?.SetInCombat(true);

            _currentVelocity = Vector3.zero;
            _targetVelocity = Vector3.zero;

            // 초기 방향 랜덤 선택
            _circleDirection = Random.value < 0.5f ? 1 : -1;

            // 거리 체크 후 초기 모드 결정
            DecideInitialMode();
        }

        public void Update()
        {
            float now = Time.time;

            // 너무 멀어지면 접근 상태로 전환
            if (_playerDetectAbility.IsTooFar())
            {
                _stateMachine.ChangeState(EMonsterState.Approach);
                return;
            }

            // 공격 시도
            if (TryAttack(now))
            {
                return;
            }

            // 항상 플레이어를 바라봄
            LookAtPlayer();

            // 현재 모드에 따른 행동 실행
            _modeTimer += Time.deltaTime;
            ExecuteCurrentMode();

            // 모드 지속 시간 완료 시 다음 모드 선택
            if (_modeTimer >= _modeDuration)
            {
                ChooseNextMode();
            }

            // 부드러운 속도 보간 적용
            ApplySmoothedMovement();

            // 애니메이션 업데이트
            UpdateAnimation();
        }

        public void Exit()
        {
            _navAgentAbility?.Resume();
        }

        private void DecideInitialMode()
        {
            float dist = _playerDetectAbility.DistanceToPlayer;
            float minDist = Data.PreferredMinDistance;
            float maxDist = Data.PreferredMaxDistance;

            if (dist < minDist)
            {
                
                SetMode(EStrafeMode.AdjustDistance, 1.0f, 2.0f);
            }
            else if (dist > maxDist)
            {
                
                SetMode(EStrafeMode.AdjustDistance, 1.0f, 2.0f);
            }
            else
            {
               
                SetMode(EStrafeMode.Circle, Data.StrafeMinDuration, Data.StrafeMaxDuration);
            }
        }

        private void ExecuteCurrentMode()
        {
            switch (_currentMode)
            {
                case EStrafeMode.Circle:
                    ExecuteCircle();
                    break;
                case EStrafeMode.AdjustDistance:
                    ExecuteAdjustDistance();
                    break;
                case EStrafeMode.Pause:
                    ExecutePause();
                    break;
                case EStrafeMode.ApproachForAttack:
                    ExecuteApproachForAttack();
                    break;
            }
        }

        private void ExecuteCircle()
        {
            if (!_playerDetectAbility.HasPlayer) return;

            Vector3 playerPos = _playerDetectAbility.PlayerPosition;
            Vector3 toMonster = _transform.position - playerPos;
            toMonster.y = 0f;

            float currentDist = toMonster.magnitude;
            if (currentDist < 0.1f) return;

            toMonster.Normalize();

            // 각속도를 라디안으로 변환
            float angularSpeed = Data.CircleAngularSpeed * Mathf.Deg2Rad;
            float angle = angularSpeed * Time.deltaTime * _circleDirection;

            // 회전 행렬 적용
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            Vector3 rotatedDir = new Vector3(
                toMonster.x * cos - toMonster.z * sin,
                0f,
                toMonster.x * sin + toMonster.z * cos
            );

            // 목표 거리 유지 (선호 거리의 중간값) - 서성이기
            float preferredDist = (Data.PreferredMinDistance + Data.PreferredMaxDistance) * 0.5f;
            Vector3 targetPos = playerPos + rotatedDir * preferredDist;
            targetPos.y = _transform.position.y;

            // 목표 속도 계산
            Vector3 moveDir = (targetPos - _transform.position);
            moveDir.y = 0f;
            float moveDist = moveDir.magnitude;

            if (moveDist > 0.01f)
            {
                moveDir.Normalize();
                _targetVelocity = moveDir * Data.StrafeSpeed;
            }
            else
            {
                _targetVelocity = Vector3.zero;
            }
        }

        private void ExecuteAdjustDistance()
        {
            if (!_playerDetectAbility.HasPlayer) return;

            float dist = _playerDetectAbility.DistanceToPlayer;
            float minDist = Data.PreferredMinDistance;
            float maxDist = Data.PreferredMaxDistance;
            float preferredDist = (minDist + maxDist) * 0.5f;

            Vector3 dirToPlayer = _playerDetectAbility.DirectionToPlayer();
            dirToPlayer.y = 0f;
            dirToPlayer.Normalize();

            if (dist < minDist)
            {
                // 후퇴
                _targetVelocity = -dirToPlayer * Data.StrafeSpeed;

                // 푸시백 요청
                if (Data.EnablePushback)
                {
                    float backoffDist = minDist - dist + 0.5f;
                    _groupCommandProvider.RequestPushback(-dirToPlayer, backoffDist);
                }
            }
            else if (dist > maxDist)
            {
                // 접근
                _targetVelocity = dirToPlayer * Data.StrafeSpeed * 0.8f;
            }
            else
            {
                // 적정 거리 도달 - 속도 줄임
                _targetVelocity = Vector3.Lerp(_targetVelocity, Vector3.zero, Time.deltaTime * 2f);
            }
        }

        private void ExecutePause()
        {
            // 정지 상태 - 부드럽게 감속
            _targetVelocity = Vector3.zero;
        }

        private void ExecuteApproachForAttack()
        {
            if (!_playerDetectAbility.HasPlayer) return;

            float lightAttackRange = Data.AttackRange * 0.6f;
            float dist = _playerDetectAbility.DistanceToPlayer;

            // 밀착 거리 도달 시 모드 종료 (TryAttack에서 공격 처리)
            if (dist <= lightAttackRange)
            {
                _targetVelocity = Vector3.zero;
                return;
            }

            // 플레이어 방향으로 접근
            Vector3 dirToPlayer = _playerDetectAbility.DirectionToPlayer();
            dirToPlayer.y = 0f;
            dirToPlayer.Normalize();

            _targetVelocity = dirToPlayer * Data.StrafeSpeed * 1.2f; // 약간 빠르게 접근
        }

        private void ChooseNextMode()
        {
            float dist = _playerDetectAbility.DistanceToPlayer;
            float minDist = Data.PreferredMinDistance;
            float maxDist = Data.PreferredMaxDistance;

            // 거리가 밴드를 벗어나면 우선 조절
            if (dist < minDist * 0.9f || dist > maxDist * 1.1f)
            {
                SetMode(EStrafeMode.AdjustDistance, 1.0f, 2.0f);
                return;
            }

            // 일반공격 쿨다운 준비 시 접근 시도 (확률 기반)
            bool canLightAttack = _groupCommandProvider.CanLightAttack(Time.time);
            bool lightAttackEnabled = Data.AttackMode == EAttackMode.Both || Data.AttackMode == EAttackMode.LightOnly;

            if (canLightAttack && lightAttackEnabled && Random.value < Data.LightAttackChance)
            {
                // 일반공격을 위해 밀착 거리로 접근
                SetMode(EStrafeMode.ApproachForAttack, 1.5f, 3.0f);
                return;
            }

            // 적정 거리일 때 모드 선택
            float roll = Random.value;

            if (roll < Data.StrafePauseChance)
            {
                // 잠시 정지
                SetMode(EStrafeMode.Pause, 0.8f, 1.5f);
            }
            else
            {
                // 원호 이동 (방향 변경 확률 적용)
                if (Random.value < Data.DirectionChangeChance)
                {
                    _circleDirection *= -1;
                }
                SetMode(EStrafeMode.Circle, Data.StrafeMinDuration, Data.StrafeMaxDuration);
            }
        }

        private void SetMode(EStrafeMode mode, float minDuration, float maxDuration)
        {
            _currentMode = mode;
            _modeTimer = 0f;
            _modeDuration = Random.Range(minDuration, maxDuration);
        }

        private void ApplySmoothedMovement()
        {
            // 부드러운 속도 보간
            float lerpFactor = Data.SpeedLerpFactor * Time.deltaTime;
            _currentVelocity = Vector3.Lerp(_currentVelocity, _targetVelocity, lerpFactor);

            // NavAgent에 이동 적용
            if (_navAgentAbility != null && _navAgentAbility.IsActive)
            {
                Vector3 targetPos = _transform.position + _currentVelocity * 0.5f;
                _navAgentAbility.SetDestination(targetPos);

                // 속도가 낮으면 정지
                if (_currentVelocity.magnitude < 0.1f)
                {
                    _navAgentAbility.Stop();
                }
                else
                {
                    _navAgentAbility.Resume();
                }
            }
        }

        private void LookAtPlayer()
        {
            if (_playerDetectAbility.HasPlayer)
            {
                _facingAbility?.FaceTo(_playerDetectAbility.PlayerPosition);
            }
        }

        private void UpdateAnimation()
        {
            if (_animatorAbility == null) return;

            // 로컬 속도로 변환
            Vector3 localVelocity = _transform.InverseTransformDirection(_currentVelocity);

            float strafeSpeed = Data.StrafeSpeed;
            float moveX = Mathf.Clamp(localVelocity.x / strafeSpeed, -1f, 1f);
            float moveY = Mathf.Clamp(localVelocity.z / strafeSpeed, -1f, 1f);

            _animatorAbility.SetMoveDirection(moveX, moveY);
        }

        private bool TryAttack(float now)
        {
            EAttackMode attackMode = Data.AttackMode;

            // 약공 시도 (제자리 공격 - 밀착 거리 필요)
            if (attackMode == EAttackMode.Both || attackMode == EAttackMode.LightOnly)
            {
                // 제자리 공격이므로 실제 히트 가능 거리로 설정 (AttackRange의 60%)
                float lightRange = Data.AttackRange * 0.6f;

                if (_playerDetectAbility.DistanceToPlayer <= lightRange &&
                    _groupCommandProvider.CanLightAttack(now) &&
                    Random.value < Data.LightAttackChance)
                {
                    _groupCommandProvider.SetNextAttackHeavy(false);
                    _groupCommandProvider.ConsumeLightAttack(now, Data.AttackCooldown);
                    _stateMachine.ChangeState(EMonsterState.Attack);
                    return true;
                }
            }

            // 강공 시도 (공격 범위 내에서만)
            if (attackMode == EAttackMode.Both || attackMode == EAttackMode.HeavyOnly)
            {
                if (_playerDetectAbility.DistanceToPlayer <= Data.AttackRange &&
                    _groupCommandProvider.CanAttack() &&
                    _groupCommandProvider.CanHeavyAttack(now) &&
                    Random.value < Data.HeavyAttackChance)
                {
                    _groupCommandProvider.SetNextAttackHeavy(true);
                    _groupCommandProvider.ConsumeHeavyAttack(now, Data.HeavyAttackCooldown);
                    _stateMachine.ChangeState(EMonsterState.Attack);
                    return true;
                }
            }

            return false;
        }
    }
}
