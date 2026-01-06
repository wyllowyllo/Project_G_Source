using System.Collections.Generic;
using Monster.AI;
using UnityEngine;

namespace Monster
{
    /// <summary>
    /// 몬스터의 상태를 관리하는 상태 머신.
    /// </summary>
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

        /// <summary>
        /// 상태를 등록합니다.
        /// </summary>
        public void RegisterState(EMonsterState stateType, IMonsterState state)
        {
            if (!_states.ContainsKey(stateType))
            {
                _states.Add(stateType, state);
            }
        }

        /// <summary>
        /// 초기 상태를 설정합니다.
        /// </summary>
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

        /// <summary>
        /// 매 프레임 현재 상태를 업데이트합니다.
        /// </summary>
        public void Update()
        {
            _currentState?.Update();
        }

        /// <summary>
        /// 상태를 전환합니다.
        /// </summary>
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
