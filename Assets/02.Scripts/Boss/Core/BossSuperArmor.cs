using System;
using UnityEngine;

namespace Boss.Core
{
    // 보스 슈퍼아머(포이즈) 시스템
    // 포이즈가 0이 되면 그로기 상태 진입
    public class BossSuperArmor
    {
        private readonly float _maxPoise;
        private float _currentPoise;
        private bool _isInfinite;

        public float MaxPoise => _maxPoise;
        public float CurrentPoise => _currentPoise;
        public float PoiseRatio => _maxPoise > 0 ? _currentPoise / _maxPoise : 0f;
        public bool IsBroken => _currentPoise <= 0f && !_isInfinite;
        public bool IsInfinite => _isInfinite;

        public event Action OnPoiseBroken;
        public event Action<float> OnPoiseChanged;

        public BossSuperArmor(float maxPoise)
        {
            _maxPoise = maxPoise;
            _currentPoise = maxPoise;
            _isInfinite = false;
        }

        // 포이즈 데미지 적용
        public void TakePoiseDamage(float damage)
        {
            if (_isInfinite || damage <= 0f)
                return;

            float previousPoise = _currentPoise;
            _currentPoise = Mathf.Max(0f, _currentPoise - damage);

            OnPoiseChanged?.Invoke(_currentPoise);

            if (previousPoise > 0f && _currentPoise <= 0f)
            {
                OnPoiseBroken?.Invoke();
            }
        }

        // 포이즈 완전 회복
        public void Recover()
        {
            _currentPoise = _maxPoise;
            OnPoiseChanged?.Invoke(_currentPoise);
        }

        // 포이즈 부분 회복
        public void Recover(float amount)
        {
            _currentPoise = Mathf.Min(_maxPoise, _currentPoise + amount);
            OnPoiseChanged?.Invoke(_currentPoise);
        }

        // 슈퍼아머 활성화 (특정 패턴 중 무한 포이즈)
        public void SetInfinite(bool infinite)
        {
            _isInfinite = infinite;
        }

        // 강제 포이즈 붕괴 (디버그/특수 상황용)
        public void ForceBroken()
        {
            if (_isInfinite)
                return;

            _currentPoise = 0f;
            OnPoiseChanged?.Invoke(_currentPoise);
            OnPoiseBroken?.Invoke();
        }
    }
}
