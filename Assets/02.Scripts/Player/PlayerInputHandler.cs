using System;
using UnityEngine;
using Progression;
using Skill;

namespace Player
{
    public class PlayerInputHandler : MonoBehaviour, ICloneDisableable
    {
        private bool _useMouseButton = true;

        [Header("Input Buffer")]
        [SerializeField] private float _bufferDuration = 1f;
        [SerializeField] private bool _enableBuffer = true;

        private bool _hasBufferedInput;
        public bool HasBufferedInput => _hasBufferedInput;

        private float _bufferInputTime;

        private bool _isEnabled = true;
        public bool IsEnabled => _isEnabled;

        public event Action OnAttackInputPressed;
        public event Action OnAimInputPressed;
        public event Action OnAimInputReleased;
        public bool IsAiming { get; private set; }

        [Header("Dodge Input")]
        [SerializeField] private KeyCode _dodgeKey = KeyCode.LeftShift;
        public event Action OnDodgeInputPressed;

        [Header("Skill Input")]
        [SerializeField] private KeyCode _qSkillKey = KeyCode.Q;
        [SerializeField] private KeyCode _eSkillKey = KeyCode.E;
        [SerializeField] private KeyCode _rSkillKey = KeyCode.R;
        public event Action<SkillSlot> OnSkillInputPressed;

        [Header("Aim Input")]
        [SerializeField] private bool _useMouseForAim = true;
        [SerializeField] private KeyCode _aimKey = KeyCode.F;

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

            bool aimDown = _useMouseForAim ? Input.GetMouseButtonDown(1) : Input.GetKeyDown(_aimKey);
            bool aimUp = _useMouseForAim ? Input.GetMouseButtonUp(1) : Input.GetKeyUp(_aimKey);

            if (aimDown)
            {
                IsAiming = true;
                OnAimInputPressed?.Invoke();
            }
            if (aimUp)
            {
                IsAiming = false;
                OnAimInputReleased?.Invoke();
            }

            if (Input.GetKeyDown(_dodgeKey))
            {
                OnDodgeInputPressed?.Invoke();
            }

            if (Input.GetKeyDown(_qSkillKey))
                OnSkillInputPressed?.Invoke(SkillSlot.Q);
            if (Input.GetKeyDown(_eSkillKey))
                OnSkillInputPressed?.Invoke(SkillSlot.E);
            if (Input.GetKeyDown(_rSkillKey))
                OnSkillInputPressed?.Invoke(SkillSlot.R);
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

        public void OnCloneDisable()
        {
            // 입력 비활성화
            SetEnabled(false);

            // 버퍼 초기화
            ClearBuffer();
        }
    }
}
