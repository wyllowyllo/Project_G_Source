using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 레벨업 이미지를 가운데에서부터 양옆으로 그라데이션으로 펼치는 애니메이션
/// LevelUpGradient.shader를 사용합니다.
/// </summary>
public class LevelUpGradientAnimation : MonoBehaviour
{
    [Header("레벨업 UI 참조")]
    [SerializeField] private GameObject _levelUpPanel;
    [SerializeField] private Image _levelUpImage;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("애니메이션 설정")]
    [SerializeField] private float _expandDuration = 1.2f;  // 펼쳐지는 시간
    [SerializeField] private float _displayDuration = 2f;   // 화면에 보이는 시간
    [SerializeField] private float _fadeDuration = 0.5f;    // 사라지는 시간
    
    [Header("Scale 설정")]
    [SerializeField] private bool _useScaleAnimation = true;
    [SerializeField] private Vector3 _startScale = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] private Vector3 _endScale = new Vector3(1f, 1f, 1f);
    
    [Header("Shader 설정")]
    [SerializeField] private AnimationCurve _expandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("추가 효과")]
    [SerializeField] private bool _playSound = true;
    [SerializeField] private AudioClip _levelUpSound;

    private Material _imageMaterial;
    private RectTransform _rectTransform;
    private AudioSource _audioSource;
    private bool _isPlaying = false;

    // Shader 프로퍼티 ID (최적화)
    private static readonly int GradientProgressID = Shader.PropertyToID("_GradientProgress");
    private static readonly int GradientWidthID = Shader.PropertyToID("_GradientWidth");
    private static readonly int GradientPowerID = Shader.PropertyToID("_GradientPower");

    private void Awake()
    {
        InitializeComponents();
        SetupMaterial();
        
        // 시작 시 숨김
        if (_levelUpPanel != null)
        {
            _levelUpPanel.SetActive(false);
        }
    }

    private void InitializeComponents()
    {
        if (_levelUpImage != null)
        {
            _rectTransform = _levelUpImage.GetComponent<RectTransform>();
        }

        if (_canvasGroup == null && _levelUpPanel != null)
        {
            _canvasGroup = _levelUpPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = _levelUpPanel.AddComponent<CanvasGroup>();
            }
        }

        // AudioSource 설정
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null && _playSound && _levelUpSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }

    private void SetupMaterial()
    {
        if (_levelUpImage == null)
        {
            Debug.LogError("[LevelUpGradientAnimation] LevelUpImage가 할당되지 않았습니다!");
            return;
        }

        // Material 인스턴스 생성
        _imageMaterial = new Material(_levelUpImage.material);
        _levelUpImage.material = _imageMaterial;

        // 초기 Shader 값 설정
        _imageMaterial.SetFloat(GradientProgressID, 0f);
        _imageMaterial.SetFloat(GradientWidthID, 0.3f);
        _imageMaterial.SetFloat(GradientPowerID, 2f);

        Debug.Log($"[LevelUpGradientAnimation] Material 설정 완료 - Shader: {_imageMaterial.shader.name}");
    }

    /// <summary>
    /// 레벨업 애니메이션 재생
    /// </summary>
    public void PlayLevelUpAnimation()
    {
        if (_isPlaying)
        {
            Debug.LogWarning("[LevelUpGradientAnimation] 애니메이션이 이미 재생 중입니다.");
            return;
        }
        
        StartCoroutine(LevelUpAnimationCoroutine());
    }

    private IEnumerator LevelUpAnimationCoroutine()
    {
        _isPlaying = true;

        // 패널 활성화
        if (_levelUpPanel != null)
        {
            _levelUpPanel.SetActive(true);
        }

        // 사운드 재생
        if (_playSound && _audioSource != null && _levelUpSound != null)
        {
            _audioSource.PlayOneShot(_levelUpSound);
        }

        // 초기 상태 설정
        if (_useScaleAnimation && _rectTransform != null)
        {
            _rectTransform.localScale = _startScale;
        }
        
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }

        if (_imageMaterial != null)
        {
            _imageMaterial.SetFloat(GradientProgressID, 0f);
        }

        // 1단계: 가운데에서 옆으로 펼쳐지기
        float elapsed = 0f;
        
        while (elapsed < _expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _expandDuration);
            float curveValue = _expandCurve.Evaluate(t);

            // Shader 그라데이션 진행도 업데이트
            if (_imageMaterial != null)
            {
                _imageMaterial.SetFloat(GradientProgressID, curveValue);
            }

            // Scale 애니메이션 (선택적)
            if (_useScaleAnimation && _rectTransform != null)
            {
                _rectTransform.localScale = Vector3.Lerp(_startScale, _endScale, curveValue);
            }

            yield return null;
        }

        // 최종 상태
        if (_imageMaterial != null)
        {
            _imageMaterial.SetFloat(GradientProgressID, 1f);
        }
        
        if (_useScaleAnimation && _rectTransform != null)
        {
            _rectTransform.localScale = _endScale;
        }

        // 2단계: 화면에 표시
        yield return new WaitForSeconds(_displayDuration);

        // 3단계: 페이드아웃
        if (_canvasGroup != null)
        {
            elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _fadeDuration;

                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

                yield return null;
            }
        }

        // 패널 비활성화
        if (_levelUpPanel != null)
        {
            _levelUpPanel.SetActive(false);
        }

        _isPlaying = false;
    }

    private void OnDestroy()
    {
        // Material 인스턴스 정리
        if (_imageMaterial != null)
        {
            Destroy(_imageMaterial);
        }
    }

    // Public 메서드
    public void TriggerLevelUp()
    {
        PlayLevelUpAnimation();
    }

    public bool IsPlaying => _isPlaying;

    // 실시간으로 Shader 파라미터 조절 (인스펙터에서 테스트용)
    [ContextMenu("Test Animation")]
    public void TestAnimation()
    {
        PlayLevelUpAnimation();
    }

    public void SetGradientWidth(float width)
    {
        if (_imageMaterial != null)
        {
            _imageMaterial.SetFloat(GradientWidthID, width);
        }
    }

    public void SetGradientPower(float power)
    {
        if (_imageMaterial != null)
        {
            _imageMaterial.SetFloat(GradientPowerID, power);
        }
    }
}
