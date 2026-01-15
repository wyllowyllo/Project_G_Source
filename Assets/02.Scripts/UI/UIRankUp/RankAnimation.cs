using System.Collections;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankAnimation : MonoBehaviour
{
    [Header("UI 요소")]
    [Tooltip("마름모 형태의 랭크 배경 이미지")]
    [SerializeField] private GameObject _rankUpPanel;
    [SerializeField] private Image _diamondBackground;

    [Tooltip("랭크 텍스트 (B, A, S 등)")]
    [SerializeField] private TextMeshProUGUI _rankText;

    [Tooltip("랭크 강화 메시지 텍스트")]
    [SerializeField] private TextMeshProUGUI _messageText;

    [Tooltip("마름모 테두리 이미지")]
    [SerializeField] private Image _diamondBorder;

    [Header("메시지 설정")]
    [Tooltip("랭크 업 메시지 (기본값)")]
    [SerializeField] private string _rankUpMessage = "랭크 강화!";

    [Tooltip("애니메이션 시작 시 UI 자동 활성화")]
    [SerializeField] private bool _autoActivateUI = true;

    [Tooltip("애니메이션 종료 시 UI 자동 비활성화")]
    [SerializeField] private bool _autoDeactivateUI = false;

    [Tooltip("UI 비활성화 딜레이 (초)")]
    [SerializeField] private float _deactivateDelay = 4f;

    [Header("파티클 시스템")]
    [Tooltip("랭크 업 파티클 시스템")]
    [SerializeField] private ParticleSystem _rankUpParticleSystem;

    [Header("광선 설정")]
    [Tooltip("광선 프리팹")]
    [SerializeField] private GameObject _lightRayPrefab;

    [Tooltip("광선 개수")]
    [Range(8, 32)]
    [SerializeField] private int _rayCount = 16;

    [Header("색상 설정 (등급별)")]
    [SerializeField] private _rankColorScheme[] _rankColorSchemes = new _rankColorScheme[]
    {
        new _rankColorScheme { rank = "D", primaryColor = new Color(0.545f, 0.451f, 0.333f), secondaryColor = new Color(0.419f, 0.325f, 0.271f) },
        new _rankColorScheme { rank = "C", primaryColor = new Color(0.486f, 0.702f, 0.259f), secondaryColor = new Color(0.333f, 0.545f, 0.184f) },
        new _rankColorScheme { rank = "B", primaryColor = new Color(0.784f, 1f, 0f), secondaryColor = new Color(0.620f, 0.792f, 0f) },
        new _rankColorScheme { rank = "A", primaryColor = new Color(1f, 0.843f, 0f), secondaryColor = new Color(1f, 0.549f, 0f) },
        new _rankColorScheme { rank = "S", primaryColor = new Color(1f, 0.431f, 0.780f), secondaryColor = new Color(0.749f, 0.243f, 1f) }
    };

    [Header("애니메이션 설정")]
    [Tooltip("애니메이션 지속 시간")]
    [SerializeField] private float _duration = 1.5f;

    [Tooltip("플래시 효과 사용")]
    [SerializeField] private bool _useFlash = true;

    [Tooltip("카메라 쉐이크 강도")]
    [Range(0f, 1f)]
    [SerializeField] private float _cameraShakeIntensity = 0.2f;

    [Header("이펙트 색상")]
    [SerializeField] private Color flashColor = Color.white;

    [Header("사운드")]
    [Tooltip("랭크 강화 효과음")]
    [SerializeField] private AudioClip _rankUpSound;

    private AudioSource _audioSource;
    private Camera _mainCamera;
    private Vector3 _originalCameraPos;
    private Canvas _canvas;
    private RectTransform _canvasRect;
    private RectTransform _diamondRect;

    [System.Serializable] private class _rankColorScheme
    {
        public string rank;
        public Color primaryColor;
        public Color secondaryColor;
    }

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _mainCamera = Camera.main;
        if (_mainCamera != null)
            _originalCameraPos = _mainCamera.transform.position;

        _canvas = GetComponentInParent<Canvas>();
        if (_canvas != null)
            _canvasRect = _canvas.GetComponent<RectTransform>();

        if (_diamondBackground != null)
            _diamondRect = _diamondBackground.GetComponent<RectTransform>();
    }

    public void ApplyAnimationSettings(float duration, float cameraShake, int rayCount)
    {
        _duration = duration;
        _cameraShakeIntensity = cameraShake;
        _rayCount = rayCount;
    }

    public void ApplyRankColors(string rank, Color primary, Color secondary)
    {
        foreach (var scheme in _rankColorSchemes)
        {
            if (scheme.rank == rank)
            {
                scheme.primaryColor = primary;
                scheme.secondaryColor = secondary;
                return;
            }
        }
    }

    public void PlayRankUpAnimation(string newRank, string customMessage = null)
    {
        // UI 활성화
        if (_autoActivateUI && _rankUpPanel != null)
        {
            _rankUpPanel.SetActive(true);  // RankUpPanel 활성화!
        }

        // 텍스트 설정
        if (_rankText != null)
        {
            _rankText.text = newRank;
            _rankText.gameObject.SetActive(true);

            // 랭크 색상 적용
            _rankColorScheme colorScheme = GetColorScheme(newRank);
            _rankText.color = colorScheme.primaryColor;

            // 텍스트 그림자/아웃라인 강화 (더 잘 보이게)
            var outline = _rankText.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = new Color(0, 0, 0, 0.8f);
                outline.effectDistance = new Vector2(3, -3);
            }
        }

        if (_messageText != null)
        {
            _messageText.text = string.IsNullOrEmpty(customMessage) ? _rankUpMessage : customMessage;
            _messageText.gameObject.SetActive(true);

            // 메시지 색상 (흰색으로 선명하게)
            _messageText.color = Color.white;

            // 텍스트 그림자/아웃라인 강화
            var outline = _messageText.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = new Color(0, 0, 0, 0.8f);
                outline.effectDistance = new Vector2(2, -2);
            }
        }

        StopAllCoroutines();
        StartCoroutine(PlayAnimationSequence(newRank));
    }

    private IEnumerator PlayAnimationSequence(string rank)
    {
        // 사운드 재생
        if (_rankUpSound != null && _audioSource != null)
            _audioSource.PlayOneShot(_rankUpSound);

        // 파티클 시스템만 재생!
        if (_rankUpParticleSystem != null)
        {
            _rankUpParticleSystem.Stop();
            _rankUpParticleSystem.Clear();
            _rankUpParticleSystem.Play();
        }

        // 마름모와 텍스트 애니메이션
        StartCoroutine(AnimateDiamond());

        // 자동 비활성화
        if (_autoDeactivateUI)
        {
            yield return new WaitForSeconds(_deactivateDelay);
            _rankUpPanel.SetActive(false);
            _rankText.gameObject.SetActive(false);
            _messageText.gameObject.SetActive(false);
        }
    }

    /// 마름모 애니메이션
    private IEnumerator AnimateDiamond()
    {
        if (_diamondBackground == null) yield break;

        Transform diamondTransform = _diamondBackground.transform;
        Vector3 originalScale = diamondTransform.localScale;
        Vector3 originalRotation = diamondTransform.localEulerAngles;

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Elastic ease out
            float scale = 1f + 0.3f * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - 0.1f) * 5f * Mathf.PI);
            diamondTransform.localScale = originalScale * scale;

            // 회전
            float rotation = Mathf.Lerp(0f, 360f, t);
            diamondTransform.localEulerAngles = new Vector3(originalRotation.x, originalRotation.y, originalRotation.z + rotation);

            yield return null;
        }

        diamondTransform.localScale = originalScale;
        diamondTransform.localEulerAngles = originalRotation;

        // 글자 펄스 (2회 반복)
        if (_rankText != null)
        {
            for (int i = 0; i < 2; i++)
            {
                yield return StartCoroutine(PulseText(_rankText.transform, 0.4f));
            }
        }

        // 메시지 텍스트도 펄스
        if (_messageText != null)
        {
            StartCoroutine(PulseText(_messageText.transform, 0.5f));
        }
    }

    private IEnumerator PulseText(Transform textTransform, float duration)
    {
        Vector3 originalScale = textTransform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float scale = 1f + 0.2f * Mathf.Sin(t * Mathf.PI);
            textTransform.localScale = originalScale * scale;

            yield return null;
        }

        textTransform.localScale = originalScale;
    }

    /// 등급에 맞는 색상 가져오기
    private _rankColorScheme GetColorScheme(string rank)
    {
        foreach (var scheme in _rankColorSchemes)
        {
            if (scheme.rank == rank)
                return scheme;
        }

        if (_rankColorSchemes == null || _rankColorSchemes.Length == 0) return new _rankColorScheme();
        // 기본값 (첫 번째 요소)
        return _rankColorSchemes[0];
    }
}