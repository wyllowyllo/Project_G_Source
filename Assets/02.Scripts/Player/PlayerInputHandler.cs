using System;
using UnityEngine;

namespace Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private bool _useMouseButton = true;

        [Header("Input Buffer")]
        [SerializeField] private float _bufferDuration = 1f;
        [SerializeField] private bool _enableBuffer = true;

        private bool _hasBufferedInput; // 콤보 다음 공격 예약확인 시
        public bool HasBufferedInput => _hasBufferedInput;

        private float _bufferInputTime; // 다음 콤보 공격 클릭 저장 유효시간

        private bool _isEnabled = true;
        public bool IsEnabled => _isEnabled;

        public event Action OnAttackInputPressed;

        private void Update()
        {
            if(!_isEnabled)
            {
                return;
            }

            ProcessInput();
            UpdateBuffer();
        }

        private void ProcessInput()
        {
            bool attackPressed = _useMouseButton ? Input.GetMouseButtonDown(0) : Input.GetKeyDown(KeyCode.Mouse0);

            if (attackPressed)
            {
                OnAttackInputPressed?.Invoke();
            }
        }

        private void UpdateBuffer()
        {
            if (!_hasBufferedInput || !_enableBuffer)
            {
                return;
            }

            _bufferInputTime -= Time.deltaTime;

            if(_bufferInputTime <= 0f)
            {
                ClearBuffer();
            }
        }

        public void BufferInput()
        {
            if(!_enableBuffer)
            {
                return;
            }

            _hasBufferedInput = true;
            _bufferInputTime = _bufferDuration;
        }

        public bool TryConsumeBuffer()
        {
            if(!_hasBufferedInput)
            {
                return false;
            }
        
            ClearBuffer();

            return true;
        }

        public void ClearBuffer()
        {
            _hasBufferedInput = false;
            _bufferInputTime = 0f;
        }

        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;

            if(!enabled)
            {
                ClearBuffer();
            }
        }
    }
}
