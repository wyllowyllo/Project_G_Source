using System.Collections.Generic;
using Boss.AI.States;
using UnityEngine;

namespace Boss.AI
{
    public class BossStateMachine
    {
        private readonly BossController _controller;
        private readonly Dictionary<EBossState, IBossState> _states;
        private IBossState _currentState;

        public IBossState CurrentState => _currentState;
        public EBossState CurrentStateType => _currentState?.StateType ?? EBossState.Idle;

        public BossStateMachine(BossController controller)
        {
            _controller = controller;
            _states = new Dictionary<EBossState, IBossState>();
        }

        public void RegisterState(EBossState stateType, IBossState state)
        {
            if (!_states.ContainsKey(stateType))
            {
                _states.Add(stateType, state);
            }
        }

        public void Initialize(EBossState initialState)
        {
            if (_states.TryGetValue(initialState, out IBossState state))
            {
                _currentState = state;
                _currentState.Enter();
            }
            else
            {
                Debug.LogError($"BossStateMachine: 초기 상태 {initialState}가 등록되지 않았습니다.");
            }
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public void ChangeState(EBossState newStateType)
        {
            if (_currentState?.StateType == newStateType)
            {
                return;
            }

            if (_states.TryGetValue(newStateType, out IBossState newState))
            {
                _currentState?.Exit();
                _currentState = newState;
                _currentState.Enter();
            }
            else
            {
                Debug.LogError($"BossStateMachine: 상태 {newStateType}가 등록되지 않았습니다.");
            }
        }

        public bool TryReEnterCurrentState()
        {
            if (_currentState is IBossReEnterable reEnterable)
            {
                reEnterable.ReEnter();
                return true;
            }
            return false;
        }
    }
}
