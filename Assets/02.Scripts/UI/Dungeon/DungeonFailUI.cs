using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DungeonFailUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private GameObject _failPanel;
    [SerializeField] private Image _failImage;
    [SerializeField] private TextMeshProUGUI _dungeonNameText;
    [SerializeField] private TextMeshProUGUI _returnMessageText;

    [Header("애니메이션 설정")]
    [SerializeField] private float _slideSpeed = 1000f;
    [SerializeField] private float _displayDuration = 2f;
    [SerializeField] private float _fadeSpeed = 1f;

    [Header("도시 이동 메세지")]
    [SerializeField] private float _returnMessageFadeSpeed = 0.7f;
    [SerializeField] private float _returnMessageDisplayDuration = 2f;

    [Header("위치 설정")]
    [SerializeField] private float _startOffsetX = 1518f;
    [SerializeField] private float _completeTargetOffsetX = 518f;
    [SerializeField] private float _backgroundTargetOffsetX = 0f;

    [Header("배경 및 텍스트")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private float _backgroundSlideSpeed = 800f;

    private RectTransform _failRectTransform;
    private RectTransform _backgroundRectTransform;
    private RectTransform _dungeonNameRectTransform;
    private CanvasGroup _canvasGroup;
    private CanvasGroup _returnMessageCanvasGroup;
    private CanvasGroup _failCanvasGroup;
    private CanvasGroup _backgroundCanvasGroup;
    private CanvasGroup _dungeonNameCanvasGroup;
    private Vector2 _startPosition;
    private Vector2 _targetPosition;

    [SerializeField] private string _returnMessage = "3초 후 도시로 이동합니다.";
    [SerializeField] private string _countdownMessageFormat = "{0}초 후 도시로 이동합니다.";
    [SerializeField] private float _countdownInterval = 1f; // 카운트다운 간격 (초)

    public event Action OnAnimationComplete;

    private void Awake()
    {
        InitializeComponents();

        // 초기에는 패널 비활성화
        if (_failPanel != null)
        {
            _failPanel.SetActive(false);
        }
    }

    private void InitializeComponents()
    {
        if (_failImage != null)
        {
            _failRectTransform = _failImage.GetComponent<RectTransform>();
        }

        if (_failPanel != null)
        {
            _canvasGroup = _failPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = _failPanel.AddComponent<CanvasGroup>();
            }
        }
    }

    public void ShowDungeonFail(string dungeonName = "")
    {
        StartCoroutine(DungeonFailAnimation(dungeonName));
    }

    private IEnumerator DungeonFailAnimation(string dungeonName)
    {
        InitializeUI(dungeonName);

        // 인트로 애니메이션 (배경과 텍스트)
        yield return StartCoroutine(AnimateIntro());

        // Complete 이미지 애니메이션
        yield return StartCoroutine(AnimateCompleteImage());

        yield return new WaitForSeconds(_displayDuration);

        yield return StartCoroutine(AnimateOutro());

        yield return StartCoroutine(AnimateReturnMessage());

        if (_failPanel != null)
        {
            _failPanel.SetActive(false);
        }

        OnAnimationComplete?.Invoke();
    }

    // UI 요소 초기화 및 초기 상태 설정
    private void InitializeUI(string dungeonName)
    {
        // 패널 활성화
        if (_failPanel != null)
        {
            _failPanel.SetActive(true);
            _canvasGroup.alpha = 1f;
        }

        // 던전 이름 설정
        if (_dungeonNameText != null && !string.IsNullOrEmpty(dungeonName))
        {
            _dungeonNameText.text = dungeonName;
        }

        if (_returnMessageText != null)
        {
            _returnMessageText.text = _returnMessage;
        }

        // Complete 이미지 CanvasGroup 초기화
        InitializeCanvasGroup(_failImage.gameObject, ref _failCanvasGroup);

        // Background 이미지 초기화
        if (_backgroundImage != null)
        {
            _backgroundRectTransform = _backgroundImage.GetComponent<RectTransform>();
            InitializeCanvasGroup(_backgroundImage.gameObject, ref _backgroundCanvasGroup);
        }

        // 던전 이름 텍스트 초기화
        if (_dungeonNameText != null)
        {
            _dungeonNameRectTransform = _dungeonNameText.GetComponent<RectTransform>();
            InitializeCanvasGroup(_dungeonNameText.gameObject, ref _dungeonNameCanvasGroup);
        }

        // 도시 이동 메시지 초기화
        if (_returnMessageText != null)
        {
            InitializeCanvasGroup(_returnMessageText.gameObject, ref _returnMessageCanvasGroup);
        }
    }

    // CanvasGroup을 추가하고 알파값 0으로 초기화
    private void InitializeCanvasGroup(GameObject gameObject, ref CanvasGroup canvasGroup)
    {
        if (gameObject == null) return;

        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;
    }

    // 배경과 텍스트가 나타나는 인트로 애니메이션
    private IEnumerator AnimateIntro()
    {
        // 배경 및 텍스트 슬라이딩 시작 (병렬 실행)
        Coroutine backgroundAnim = StartCoroutine(
            SlideAndFadeIn(_backgroundRectTransform, _backgroundCanvasGroup, _backgroundSlideSpeed, _backgroundTargetOffsetX)
        );

        Coroutine textAnim = StartCoroutine(
            SlideAndFadeIn(_dungeonNameRectTransform, _dungeonNameCanvasGroup, _backgroundSlideSpeed, _backgroundTargetOffsetX)
        );

        // Complete 이미지 나타나기 전 짧은 대기
        yield return new WaitForSeconds(0.7f);
    }

    // Complete 이미지가 나타나는 애니메이션
    private IEnumerator AnimateCompleteImage()
    {
        if (_failRectTransform == null || _failCanvasGroup == null)
        {
            yield break;
        }

        // 시작 위치와 목표 위치 설정
        _startPosition = _failRectTransform.anchoredPosition;
        _startPosition.x = _startOffsetX;
        _targetPosition = _startPosition;
        _targetPosition.x = _completeTargetOffsetX;

        _failRectTransform.anchoredPosition = _startPosition;

        // 슬라이드 애니메이션
        float elapsedTime = 0f;
        float duration = Mathf.Abs(_startOffsetX - _completeTargetOffsetX) / _slideSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Ease-out 효과 적용
            t = 1f - Mathf.Pow(1f - t, 3f);

            // 위치 업데이트
            _failRectTransform.anchoredPosition = Vector2.Lerp(_startPosition, _targetPosition, t);

            // 페이드 인
            _failCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        // 최종 위치 및 알파값 보정
        _failRectTransform.anchoredPosition = _targetPosition;
        _failCanvasGroup.alpha = 1f;
    }

    // 모든 요소가 사라지는 아웃트로 페이드 아웃 애니메이션
    private IEnumerator AnimateOutro()
    {
        float fadeElapsed = 0f;

        while (fadeElapsed < _fadeSpeed)
        {
            fadeElapsed += Time.deltaTime;
            float alpha = 1f - (fadeElapsed / _fadeSpeed);

            // 모든 UI 요소 페이드 아웃
            SetAlpha(_canvasGroup, alpha);
            SetAlpha(_failCanvasGroup, alpha);
            SetAlpha(_backgroundCanvasGroup, alpha);
            SetAlpha(_dungeonNameCanvasGroup, alpha);

            yield return null;
        }

        // 최종 알파값 보정
        SetAlpha(_canvasGroup, 0f);
        SetAlpha(_failCanvasGroup, 0f);
        SetAlpha(_backgroundCanvasGroup, 0f);
        SetAlpha(_dungeonNameCanvasGroup, 0f);
    }

    private IEnumerator AnimateReturnMessage()
    {
        if (_returnMessageCanvasGroup == null || _returnMessageText == null)
        {
            yield break;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }


        // 페이드 인
        float fadeElapsed = 0f;
        while (fadeElapsed < _returnMessageFadeSpeed)
        {
            fadeElapsed += Time.deltaTime;
            float alpha = fadeElapsed / _returnMessageFadeSpeed;
            _returnMessageCanvasGroup.alpha = alpha;
            yield return null;
        }
        _returnMessageCanvasGroup.alpha = 1f;

        // 카운트다운 (3 -> 2 -> 1)
        for (int countdown = 3; countdown > 0; countdown--)
        {
            _returnMessageText.text = string.Format(_countdownMessageFormat, countdown);
            yield return new WaitForSeconds(_countdownInterval);
        }

        // 페이드 아웃
        fadeElapsed = 0f;
        while (fadeElapsed < _returnMessageFadeSpeed)
        {
            fadeElapsed += Time.deltaTime;
            float alpha = 1f - (fadeElapsed / _returnMessageFadeSpeed);
            _returnMessageCanvasGroup.alpha = alpha;
            yield return null;
        }
        _returnMessageCanvasGroup.alpha = 0f;
    }

    // CanvasGroup의 알파값을 안전하게 설정
    private void SetAlpha(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }

    // UI 요소를 슬라이딩하면서 페이드 인하는 코루틴
    private IEnumerator SlideAndFadeIn(RectTransform rectTransform, CanvasGroup canvasGroup, float speed, float targetX)
    {
        if (rectTransform == null || canvasGroup == null)
        {
            yield break;
        }

        // 시작 위치 설정 (오른쪽 밖)
        Vector2 startPos = rectTransform.anchoredPosition;
        startPos.x = _startOffsetX;
        rectTransform.anchoredPosition = startPos;

        Vector2 targetPos = startPos;
        targetPos.x = targetX;

        canvasGroup.alpha = 0f;

        float elapsedTime = 0f;
        float duration = Mathf.Abs(_startOffsetX - targetX) / speed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Ease-out 효과
            t = 1f - Mathf.Pow(1f - t, 3f);

            // 위치 업데이트
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
        canvasGroup.alpha = 1f;
    }

    // 외부에서 던전 클리어를 트리거하는 예시 메서드
    public void OnDungeonFailed()
    {
        ShowDungeonFail("뭉글뭉글 언덕");
    }
}
