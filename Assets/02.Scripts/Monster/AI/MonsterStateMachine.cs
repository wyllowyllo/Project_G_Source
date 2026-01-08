using System.Collections.Generic;
using Monster.AI.States;
using UnityEngine;

namespace Monster.AI
{
    // 몬스터의 상태를 관리하는 상태 머신
    public class MonsterStateMachine
    {
        private readonly MonsterController _controller;
        private readonly Dictionary<EMonsterState, IMonsterState> _states;
        private IMonsterState _currentState;

        public IMonsterState CurrentState => _currentState;
        public EMonsterState CurrentStateType => _currentState?.StateType ?? EMonsterState.Idle;

        public MonsterStateMachine(MonsterController controller)
        {
            _controller = controller;
            _states = new Dictionary<EMonsterState, IMonsterState>();
        }

        // 상태를 등록
        public void RegisterState(EMonsterState stateType, IMonsterState state)
        {
            if (!_states.ContainsKey(stateType))
            {
                _states.Add(stateType, state);
            }
        }

        // 초기 상태를 설정
        public void Initialize(EMonsterState initialState)
        {
            if (_states.TryGetValue(initialState, out IMonsterState state))
            {
                _currentState = state;
                _currentState.Enter();
            }
            else
            {
                Debug.LogError($"MonsterStateMachine: 초기 상태 {initialState}가 등록되지 않았습니다.");
            }
        }

        // 매 프레임 현재 상태를 업데이트
        public void Update()
        {
            _currentState?.Update();
        }

        // 상태를 전환
        public void ChangeState(EMonsterState newStateType)
        {
            if (_currentState?.StateType == newStateType)
            {
                return;
            }

            if (_states.TryGetValue(newStateType, out IMonsterState newState))
            {
                _currentState?.Exit();
                _currentState = newState;
                _currentState.Enter();
            }
            else
            {
                Debug.LogError($"MonsterStateMachine: 상태 {newStateType}가 등록되지 않았습니다.");
            }
        }
    }
}
