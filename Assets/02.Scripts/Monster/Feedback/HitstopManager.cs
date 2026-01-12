using System.Collections;
using Monster.Feedback.Data;
using UnityEngine;

namespace Monster.Feedback
{
    // Time.timeScale을 조작하여 히트스탑(프레임 프리즈) 효과 제공
    // Hades 스타일의 짧고 강렬한 프레임 정지로 타격감 강화
    public class HitstopManager : MonoBehaviour
    {
        private static HitstopManager _instance;
        public static HitstopManager Instance => _instance;

        [Header("Settings")]
        [SerializeField] private bool _pauseAudioDuringHitstop = false;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = false;

        private Coroutine _activeHitstop;
        private float _originalTimeScale = 1f;
        private float _originalFixedDeltaTime;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            _originalFixedDeltaTime = Time.fixedDeltaTime;
        }

        public void TriggerHitstop(HitstopConfig config)
        {
            if (!config.Enabled || config.Duration <= 0) return;

            // 이미 진행 중인 히트스탑이 있으면 취소하고 새로 시작
            if (_activeHitstop != null)
            {
                StopCoroutine(_activeHitstop);
                RestoreTimeScale();
            }

            _activeHitstop = StartCoroutine(HitstopCoroutine(config));
        }

        private IEnumerator HitstopCoroutine(HitstopConfig config)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[HitstopManager] Starting hitstop: Duration={config.Duration}s, TimeScale={config.TimeScale}");
            }

            _originalTimeScale = Time.timeScale;
            Time.timeScale = config.TimeScale;

            // FixedDeltaTime도 조정하여 물리 시뮬레이션 동기화
            Time.fixedDeltaTime = _originalFixedDeltaTime * config.TimeScale;

            if (_pauseAudioDuringHitstop)
            {
                AudioListener.pause = true;
            }

            // unscaled time 사용하여 히트스탑 지속
            yield return new WaitForSecondsRealtime(config.Duration);

            RestoreTimeScale();
            _activeHitstop = null;

            if (_enableDebugLogs)
            {
                Debug.Log("[HitstopManager] Hitstop ended");
            }
        }

        private void RestoreTimeScale()
        {
            Time.timeScale = _originalTimeScale;
            Time.fixedDeltaTime = _originalFixedDeltaTime;

            if (_pauseAudioDuringHitstop)
            {
                AudioListener.pause = false;
            }
        }

        // 외부에서 히트스탑 강제 종료
        public void CancelHitstop()
        {
            if (_activeHitstop != null)
            {
                StopCoroutine(_activeHitstop);
                RestoreTimeScale();
                _activeHitstop = null;
            }
        }

        public bool IsHitstopActive => _activeHitstop != null;

        private void OnDestroy()
        {
            if (_instance == this)
            {
                RestoreTimeScale();
                _instance = null;
            }
        }

        private void OnDisable()
        {
            if (_activeHitstop != null)
            {
                RestoreTimeScale();
            }
        }
    }
}
