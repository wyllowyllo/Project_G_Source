using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 랭크업 UI 애니메이션을 담당하는 메인 컨트롤러
/// 단일 책임: 다이아몬드 및 텍스트 애니메이션 연출
/// </summary>
public class RankAnimator : MonoBehaviour
{
    [Header("필수 컴포넌트 참조")]
    [Tooltip("색상 관리 컨트롤러")]
    [SerializeField] private RankColorController _colorController;

    [Tooltip("이펙트 재생 컨트롤러")]
    [SerializeField] private RankEffectsController _effectsController;

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

    [Header("UI 활성화 설정")]
    [Tooltip("애니메이션 시작 시 UI 자동 활성화")]
    [SerializeField] private bool _autoActivateUI = true;

    [Tooltip("애니메이션 종료 시 UI 자동 비활성화")]
    [SerializeField] private bool _autoDeactivateUI = false;

    [Tooltip("UI 비활성화 딜레이 (초)")]
    [SerializeField] private float _deactivateDelay = 4f;

    [Header("애니메이션 설정")]
    [Tooltip("애니메이션 지속 시간")]
    [SerializeField] private float _duration = 1.5f;

    private void Awake()
    {
        // 컴포넌트 자동 참조
        if (_colorController == null)
            _colorController = GetComponent<RankColorController>();

        if (_effectsController == null)
            _effectsController = GetComponent<RankEffectsController>();

        // 필수 컴포넌트 체크
        if (_colorController == null)
        {
            Debug.LogError("[RankAnimator] RankColorController가 없습니다!");
        }

        if (_effectsController == null)
        {
            Debug.LogError("[RankAnimator] RankEffectsController가 없습니다!");
        }
    }

    /// <summary>
    /// 애니메이션 설정 적용
    /// </summary>
    public void ApplyAnimationSettings(float duration)
    {
        _duration = duration;
    }

    /// <summary>
    /// 랭크업 애니메이션 재생 (메인 진입점)
    /// </summary>
    public void PlayRankUpAnimation(string newRank, string customMessage = null)
    {
        // UI 활성화
        if (_autoActivateUI && _rankUpPanel != null)
        {
            _rankUpPanel.SetActive(true);
        }

        // UI 텍스트 및 색상 설정
        SetupUI(newRank, customMessage);

        // 애니메이션 시퀀스 시작
        StopAllCoroutines();
        StartCoroutine(PlayAnimationSequence(newRank));
    }

    /// <summary>
    /// UI 텍스트 및 색상 설정
    /// </summary>
    private void SetupUI(string newRank, string customMessage)
    {
        // 랭크 색상 스킴 가져오기
        var colorScheme = _colorController.GetColorScheme(newRank);

        // 랭크 텍스트 설정
        if (_rankText != null)
        {
            _rankText.text = newRank;
            _rankText.gameObject.SetActive(true);
            _rankText.color = colorScheme.primaryColor;

            // 텍스트 아웃라인 강화
            var outline = _rankText.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = new Color(0, 0, 0, 0.8f);
                outline.effectDistance = new Vector2(3, -3);
            }
        }

        // 메시지 텍스트 설정
        if (_messageText != null)
        {
            _messageText.text = string.IsNullOrEmpty(customMessage) ? _rankUpMessage : customMessage;
            _messageText.gameObject.SetActive(true);
            _messageText.color = Color.white;

            // 텍스트 아웃라인 강화
            var outline = _messageText.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = new Color(0, 0, 0, 0.8f);
                outline.effectDistance = new Vector2(2, -2);
            }
        }
    }

    /// <summary>
    /// 애니메이션 시퀀스 실행
    /// </summary>
    private IEnumerator PlayAnimationSequence(string rank)
    {
        // 이펙트 재생 (파티클, 사운드, 카메라 쉐이크)
        if (_effectsController != null)
        {
            _effectsController.PlayEffects();
        }

        // 마름모와 텍스트 애니메이션
        yield return StartCoroutine(AnimateDiamond());

        // 자동 비활성화
        if (_autoDeactivateUI)
        {
            yield return new WaitForSeconds(_deactivateDelay);
            DeactivateUI();
        }
    }

    /// <summary>
    /// 마름모 애니메이션
    /// </summary>
    private IEnumerator AnimateDiamond()
    {
        if (_diamondBackground == null) yield break;

        Transform diamondTransform = _diamondBackground.transform;
        Vector3 originalScale = diamondTransform.localScale;
        Vector3 originalRotation = diamondTransform.localEulerAngles;

        float duration = 0.8f;
        float elapsed = 0f;

        // 마름모 확대/회전 애니메이션 (Elastic Ease Out)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Elastic ease out
            float scale = 1f + 0.3f * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - 0.1f) * 5f * Mathf.PI);
            diamondTransform.localScale = originalScale * scale;

            // 회전
            float rotation = Mathf.Lerp(0f, 360f, t);
            diamondTransform.localEulerAngles = new Vector3(
                originalRotation.x, 
                originalRotation.y, 
                originalRotation.z + rotation
            );

            yield return null;
        }

        // 원래 상태로 복구
        diamondTransform.localScale = originalScale;
        diamondTransform.localEulerAngles = originalRotation;

        // 랭크 텍스트 펄스 (2회 반복)
        if (_rankText != null)
        {
            for (int i = 0; i < 2; i++)
            {
                yield return StartCoroutine(PulseText(_rankText.transform, 0.4f));
            }
        }

        // 메시지 텍스트 펄스
        if (_messageText != null)
        {
            yield return StartCoroutine(PulseText(_messageText.transform, 0.5f));
        }
    }

    /// <summary>
    /// 텍스트 펄스 애니메이션
    /// </summary>
    private IEnumerator PulseText(Transform textTransform, float duration)
    {
        Vector3 originalScale = textTransform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 사인파를 이용한 펄스 효과
            float scale = 1f + 0.2f * Mathf.Sin(t * Mathf.PI);
            textTransform.localScale = originalScale * scale;

            yield return null;
        }

        textTransform.localScale = originalScale;
    }

    /// <summary>
    /// UI 비활성화
    /// </summary>
    private void DeactivateUI()
    {
        if (_rankUpPanel != null)
            _rankUpPanel.SetActive(false);

        if (_rankText != null)
            _rankText.gameObject.SetActive(false);

        if (_messageText != null)
            _messageText.gameObject.SetActive(false);
    }
}
