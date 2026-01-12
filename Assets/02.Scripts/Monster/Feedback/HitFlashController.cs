using System.Collections;
using Monster.Feedback.Data;
using UnityEngine;

namespace Monster.Feedback
{
    // MaterialPropertyBlock을 사용하여 몬스터 히트 플래시 효과 제공
    // GPU 인스턴싱을 유지하면서 개별 머티리얼 프로퍼티 변경
    public class HitFlashController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Renderer[] _renderers;

        [Header("Shader Properties")]
        [SerializeField] private string _emissionColorProperty = "_EmissionColor";
        [SerializeField] private string _baseColorProperty = "_BaseColor";

        [Header("Fallback (Non-emission shaders)")]
        [SerializeField] private bool _useTintFallback = true;
        [SerializeField] private string _tintColorProperty = "_Color";

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = false;

        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _flashCoroutine;

        private int _emissionColorId;
        private int _baseColorId;
        private int _tintColorId;

        private Color[] _originalBaseColors;
        private bool _supportsEmission;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();

            _emissionColorId = Shader.PropertyToID(_emissionColorProperty);
            _baseColorId = Shader.PropertyToID(_baseColorProperty);
            _tintColorId = Shader.PropertyToID(_tintColorProperty);

            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>();
            }

            CacheOriginalColors();
            CheckEmissionSupport();
        }

        private void CacheOriginalColors()
        {
            _originalBaseColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].sharedMaterial != null)
                {
                    var mat = _renderers[i].sharedMaterial;
                    if (mat.HasProperty(_baseColorId))
                    {
                        _originalBaseColors[i] = mat.GetColor(_baseColorId);
                    }
                    else if (mat.HasProperty(_tintColorId))
                    {
                        _originalBaseColors[i] = mat.GetColor(_tintColorId);
                    }
                    else
                    {
                        _originalBaseColors[i] = Color.white;
                    }
                }
            }
        }

        private void CheckEmissionSupport()
        {
            _supportsEmission = false;
            foreach (var renderer in _renderers)
            {
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    if (renderer.sharedMaterial.HasProperty(_emissionColorId))
                    {
                        _supportsEmission = true;
                        break;
                    }
                }
            }

            if (_enableDebugLogs)
            {
                Debug.Log($"[HitFlashController] Emission support: {_supportsEmission}");
            }
        }

        public void TriggerFlash(HitFlashConfig config)
        {
            if (!config.Enabled) return;

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(FlashCoroutine(config));
        }

        private IEnumerator FlashCoroutine(HitFlashConfig config)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[HitFlashController] Starting flash: Color={config.FlashColor}, Duration={config.FlashDuration}s");
            }

            float elapsed = 0f;

            while (elapsed < config.FlashDuration)
            {
                float t = elapsed / config.FlashDuration;
                float curveValue = config.FlashCurve?.Evaluate(t) ?? t;

                if (_supportsEmission)
                {
                    // Emission 기반 플래시 (HDR 지원)
                    Color emissionColor = Color.Lerp(
                        config.FlashColor * config.FlashIntensity,
                        Color.black,
                        curveValue
                    );
                    ApplyEmissionColor(emissionColor);
                }
                else if (_useTintFallback)
                {
                    // Base color 블렌딩 폴백
                    ApplyColorTint(config.FlashColor, 1f - curveValue);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 원래 상태로 복원
            ResetColors();
            _flashCoroutine = null;

            if (_enableDebugLogs)
            {
                Debug.Log("[HitFlashController] Flash ended");
            }
        }

        private void ApplyEmissionColor(Color color)
        {
            foreach (var renderer in _renderers)
            {
                if (renderer == null) continue;

                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(_emissionColorId, color);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private void ApplyColorTint(Color tintColor, float intensity)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;

                Color blendedColor = Color.Lerp(_originalBaseColors[i], tintColor, intensity);

                _renderers[i].GetPropertyBlock(_propertyBlock);

                if (_renderers[i].sharedMaterial.HasProperty(_baseColorId))
                {
                    _propertyBlock.SetColor(_baseColorId, blendedColor);
                }
                else if (_renderers[i].sharedMaterial.HasProperty(_tintColorId))
                {
                    _propertyBlock.SetColor(_tintColorId, blendedColor);
                }

                _renderers[i].SetPropertyBlock(_propertyBlock);
            }
        }

        private void ResetColors()
        {
            if (_supportsEmission)
            {
                ApplyEmissionColor(Color.black);
            }
            else if (_useTintFallback)
            {
                // PropertyBlock 클리어
                foreach (var renderer in _renderers)
                {
                    if (renderer == null) continue;
                    renderer.SetPropertyBlock(null);
                }
            }
        }

        private void OnDisable()
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                ResetColors();
                _flashCoroutine = null;
            }
        }

        // 런타임에 Renderer 배열 설정
        public void SetRenderers(Renderer[] renderers)
        {
            _renderers = renderers;
            CacheOriginalColors();
            CheckEmissionSupport();
        }
    }
}
