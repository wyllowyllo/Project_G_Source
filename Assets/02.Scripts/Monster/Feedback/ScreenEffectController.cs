using System.Collections;
using Monster.Feedback.Data;
using UnityEngine;

namespace Monster.Feedback
{
    // 화면 효과 컨트롤러 (URP Volume 없이 동작)
    // 크리티컬 히트나 사망 시 화면 피드백 제공
    // URP Volume 사용 시 별도로 연결 필요
    public class ScreenEffectController : MonoBehaviour
    {
        private static ScreenEffectController _instance;
        public static ScreenEffectController Instance => _instance;

        [Header("Canvas Overlay (Optional)")]
        [SerializeField] private CanvasGroup _flashOverlay;
        [SerializeField] private UnityEngine.UI.Image _flashImage;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = false;

        private Coroutine _activeEffect;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        public void TriggerScreenEffect(ScreenEffectConfig config)
        {
            if (!config.Enabled) return;

            if (_activeEffect != null)
            {
                StopCoroutine(_activeEffect);
            }

            _activeEffect = StartCoroutine(ScreenEffectCoroutine(config));
        }

        private IEnumerator ScreenEffectCoroutine(ScreenEffectConfig config)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[ScreenEffectController] Starting effect: Duration={config.Duration}s");
            }

            // Canvas 오버레이가 있으면 사용
            if (_flashOverlay != null && _flashImage != null)
            {
                yield return CanvasFlashCoroutine(config);
            }
            else
            {
                // 오버레이 없으면 대기만
                yield return new WaitForSecondsRealtime(config.Duration);
            }

            _activeEffect = null;

            if (_enableDebugLogs)
            {
                Debug.Log("[ScreenEffectController] Effect ended");
            }
        }

        private IEnumerator CanvasFlashCoroutine(ScreenEffectConfig config)
        {
            float elapsed = 0f;
            float halfDuration = config.Duration * 0.5f;

            Color flashColor = config.ColorTint;
            flashColor.a = config.VignetteIntensity;

            // Ramp up
            while (elapsed < halfDuration)
            {
                float t = elapsed / halfDuration;
                t = t * t;
                _flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, flashColor.a * t);
                _flashOverlay.alpha = t;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Ramp down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                float t = 1f - (elapsed / halfDuration);
                t = t * t;
                _flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, flashColor.a * t);
                _flashOverlay.alpha = t;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _flashOverlay.alpha = 0f;
        }

        public void CancelEffect()
        {
            if (_activeEffect != null)
            {
                StopCoroutine(_activeEffect);
                if (_flashOverlay != null)
                {
                    _flashOverlay.alpha = 0f;
                }
                _activeEffect = null;
            }
        }

        public bool IsEffectActive => _activeEffect != null;

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnDisable()
        {
            if (_flashOverlay != null)
            {
                _flashOverlay.alpha = 0f;
            }
        }
    }
}
