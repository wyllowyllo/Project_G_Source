using UnityEngine;
using Monster.AI.Services;

namespace Monster.AI.States
{
    // 공격 상태: Windup (준비) → Execute (실행) → Recover (후딜) 3단계로 구성
    // 공격 슬롯 시스템과 연동하여 동시 공격을 제한
    public class AttackState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly GroupCommandProvider _groupCommandProvider;
        private readonly ChargeService _chargeService;
        private readonly Transform _transform;

        private enum EAttackPhase { Windup, Execute }
        private EAttackPhase _currentPhase;

        private bool _isHeavyAttack;
        private float _distanceToPlayer;
        private float _phaseTimer;
        private bool _damageDealt;
        private float _hitRadius;

        public EMonsterState StateType => EMonsterState.Attack;

        public AttackState(MonsterController controller, MonsterStateMachine stateMachine, GroupCommandProvider groupCommandProvider)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _groupCommandProvider = groupCommandProvider;
            _transform = controller.transform;
            _chargeService = new ChargeService();
        }

        public void Enter()
        {
            _chargeService.Initialize(_controller.NavAgent, _transform);
            _chargeService.SaveAgentSettings();

            DetermineAttackType();

            // 약공일 때: 플레이어와 직접 대면한 경우만 공격 (다른 몬스터에게 가로막힌 경우 Strafe)
            if (!_isHeavyAttack && !HasDirectLineOfSightToPlayer())
            {
                Debug.Log($"{_controller.gameObject.name}: 약공 취소 (플레이어 시야 차단됨) - Strafe로 전환");
                _stateMachine.ChangeState(EMonsterState.Strafe);
                return;
            }

            ConfigureAttackParameters();
            SetAttackVisualFeedback();
            InitializeAttackPhase();

            Debug.Log($"{_controller.gameObject.name}: 공격 시작 (Windup)");
        }

        public void Update()
        {
            if (_isHeavyAttack && !_groupCommandProvider.CanAttack())
            {
                ReturnToCombat();
                return;
            }

            _distanceToPlayer = Vector3.Distance(_transform.position, _controller.PlayerTransform.position);

            if (_currentPhase == EAttackPhase.Windup)
            {
                if (_distanceToPlayer > _controller.Data.AttackRange * 2f)
                {
                    ReturnToCombat();
                    return;
                }

                LookAtPlayer();
            }

            _phaseTimer += Time.deltaTime;

            switch (_currentPhase)
            {
                case EAttackPhase.Windup:
                    UpdateWindupPhase();
                    break;

                case EAttackPhase.Execute:
                    UpdateExecutePhase();
                    break;
            }
        }

        public void Exit()
        {
            _chargeService.RestoreAgentSettings();
        }

        private void UpdateWindupPhase()
        {
            if (_phaseTimer >= _controller.Data.WindupTime)
            {
                StartChargePhase();
                _currentPhase = EAttackPhase.Execute;
                _phaseTimer = 0f;
                Debug.Log($"{_controller.gameObject.name}: 돌진 시작 (Execute)");
            }
        }

        private void UpdateExecutePhase()
        {
            // 타격 판정
            if (!_damageDealt && _distanceToPlayer <= _hitRadius)
            {
                DealDamage();
                _damageDealt = true;
            }

            ChargeResult result = _chargeService.UpdateCharge(Time.deltaTime);

            if (result.IsComplete)
            {
                _stateMachine.ChangeState(EMonsterState.Recover);
            }
        }

        private void StartChargePhase()
        {
            ChargeParameters parameters = CreateChargeParameters();
            _chargeService.StartCharge(_controller.PlayerTransform.position, parameters);
        }

        private ChargeParameters CreateChargeParameters()
        {
            if (_isHeavyAttack)
            {
                return new ChargeParameters
                {
                    ChargeSpeed = _controller.Data.ChargeSpeed,
                    MaxChargeDistance = 5f,
                    ExecuteDuration = _controller.Data.ExecuteTime,
                    MaxExecuteDuration = Mathf.Max(2f, _controller.Data.ExecuteTime + 0.6f)
                };
            }

            float executeDuration = Mathf.Max(0.08f, _controller.Data.ExecuteTime * 0.6f);
            return new ChargeParameters
            {
                ChargeSpeed = Mathf.Max(_controller.Data.MoveSpeed * 1.8f, _controller.Data.MoveSpeed + 2.0f),
                MaxChargeDistance = 1.6f,
                ExecuteDuration = executeDuration,
                MaxExecuteDuration = Mathf.Max(0.9f, executeDuration + 0.4f)
            };
        }

        private void DetermineAttackType()
        {
            _isHeavyAttack = _groupCommandProvider.NextAttackIsHeavy;
            _groupCommandProvider.MarkCurrentAttackHeavy(_isHeavyAttack);
            _groupCommandProvider.SetNextAttackHeavy(false);
        }

        private void ConfigureAttackParameters()
        {
            _hitRadius = _isHeavyAttack ? 0.6f : 0.7f;
        }

        private void SetAttackVisualFeedback()
        {
            Renderer renderer = _controller.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = _isHeavyAttack ? Color.red : new Color(1f, 0.6f, 0f);
            }
        }

        private void InitializeAttackPhase()
        {
            _currentPhase = EAttackPhase.Windup;
            _phaseTimer = 0f;
            _damageDealt = false;
        }

        private void LookAtPlayer()
        {
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
        }

        private void DealDamage()
        {
            Debug.Log($"{_controller.gameObject.name} 공격! 데미지: {_controller.Data.AttackDamage}");

            if (_controller.PlayerTransform.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(_controller.Data.AttackDamage, _transform.position);
            }
        }

        private void ReturnToCombat()
        {
            Renderer renderer = _controller.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = _controller.OriginalMaterialColor;
            }

            float distanceToPlayer = Vector3.Distance(_transform.position, _controller.PlayerTransform.position);

            if (distanceToPlayer > _controller.Data.PreferredMaxDistance)
                _stateMachine.ChangeState(EMonsterState.Approach);
            else
                _stateMachine.ChangeState(EMonsterState.Strafe);
        }

        private bool HasDirectLineOfSightToPlayer()
        {
            Vector3 startPosition = _transform.position + Vector3.up * 1.0f;
            Vector3 playerPosition = _controller.PlayerTransform.position + Vector3.up * 1.0f;
            Vector3 directionToPlayer = playerPosition - startPosition;
            float distance = directionToPlayer.magnitude;

            if (Physics.Raycast(startPosition, directionToPlayer.normalized, out RaycastHit hit, distance))
            {
                if (hit.transform != _controller.PlayerTransform)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
