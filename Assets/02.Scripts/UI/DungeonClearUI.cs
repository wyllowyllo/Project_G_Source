using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonClearUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private GameObject completePanel;
    [SerializeField] private Image completeImage;
    [SerializeField] private TextMeshProUGUI dungeonNameText;

    [Header("애니메이션 설정")]
    [SerializeField] private float slideSpeed = 1000f;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeSpeed = 2f;

    [Header("위치 설정")]
    [SerializeField] private float startOffsetX = 1518f;
    [SerializeField] private float completeTargetOffsetX = 518f; // Complete 이미지는 살짝 오른쪽
    [SerializeField] private float backgroundTargetOffsetX = 0f; // Background는 중앙

    [Header("배경 및 텍스트")]
    [SerializeField] private Image backgroundImage; // 배경 이미지
    [SerializeField] private float backgroundSlideSpeed = 800f; // 배경 슬라이드 속도

    private RectTransform completeRectTransform;
    private RectTransform backgroundRectTransform;
    private RectTransform dungeonNameRectTransform;
    private CanvasGroup canvasGroup;
    private CanvasGroup completeCanvasGroup; // Complete 이미지용 CanvasGroup
    private CanvasGroup backgroundCanvasGroup;
    private CanvasGroup dungeonNameCanvasGroup;
    private Vector2 startPosition;
    private Vector2 targetPosition;

    private void Awake()
    {
        // 컴포넌트 초기화
        if (completeImage != null)
        {
            completeRectTransform = completeImage.GetComponent<RectTransform>();
        }

        // CanvasGroup 가져오기 또는 추가
        if (completePanel != null)
        {
            canvasGroup = completePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = completePanel.AddComponent<CanvasGroup>();
            }
        }

        // 초기에는 패널 비활성화
        if (completePanel != null)
        {
            completePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 던전 클리어 시 호출되는 메서드
    /// </summary>
    /// <param name="dungeonName">던전 이름 (선택사항)</param>
    public void ShowDungeonClear(string dungeonName = "")
    {
        StartCoroutine(DungeonClearAnimation(dungeonName));
    }

    private IEnumerator DungeonClearAnimation(string dungeonName)
    {
        // 패널 활성화
        if (completePanel != null)
        {
            completePanel.SetActive(true);
            canvasGroup.alpha = 1f;
        }

        // 던전 이름 설정
        if (dungeonNameText != null && !string.IsNullOrEmpty(dungeonName))
        {
            dungeonNameText.text = dungeonName;
        }

        // Complete 이미지 CanvasGroup 초기화 및 알파값 0 설정
        if (completeImage != null)
        {
            completeCanvasGroup = completeImage.GetComponent<CanvasGroup>();
            if (completeCanvasGroup == null)
            {
                completeCanvasGroup = completeImage.gameObject.AddComponent<CanvasGroup>();
            }
            completeCanvasGroup.alpha = 0f; // 처음에는 보이지 않음
        }

        // RectTransform 및 CanvasGroup 초기화
        if (backgroundImage != null)
        {
            backgroundRectTransform = backgroundImage.GetComponent<RectTransform>();
            backgroundCanvasGroup = backgroundImage.GetComponent<CanvasGroup>();
            if (backgroundCanvasGroup == null)
            {
                backgroundCanvasGroup = backgroundImage.gameObject.AddComponent<CanvasGroup>();
            }
            backgroundCanvasGroup.alpha = 0f;
        }

        if (dungeonNameText != null)
        {
            dungeonNameRectTransform = dungeonNameText.GetComponent<RectTransform>();
            dungeonNameCanvasGroup = dungeonNameText.GetComponent<CanvasGroup>();
            if (dungeonNameCanvasGroup == null)
            {
                dungeonNameCanvasGroup = dungeonNameText.gameObject.AddComponent<CanvasGroup>();
            }
            dungeonNameCanvasGroup.alpha = 0f;
        }

        // 배경 및 텍스트 슬라이딩 시작 (오른쪽에서 왼쪽으로 + 페이드 인)
        StartCoroutine(SlideAndFadeIn(backgroundRectTransform, backgroundCanvasGroup, backgroundSlideSpeed, backgroundTargetOffsetX));
        StartCoroutine(SlideAndFadeIn(dungeonNameRectTransform, dungeonNameCanvasGroup, backgroundSlideSpeed, backgroundTargetOffsetX));

        // 2초 대기 후 Complete 이미지 슬라이딩 시작
        yield return new WaitForSeconds(0.5f);

        // Complete 이미지 시작 위치와 목표 위치 설정
        if (completeRectTransform != null)
        {
            startPosition = completeRectTransform.anchoredPosition;
            startPosition.x = startOffsetX; // 화면 오른쪽 밖에서 시작
            targetPosition = startPosition;
            targetPosition.x = completeTargetOffsetX; // 살짝 오른쪽으로 이동

            completeRectTransform.anchoredPosition = startPosition;
        }

        // Complete 이미지 슬라이드 애니메이션 (오른쪽에서 왼쪽으로 + 페이드 인)
        float elapsedTime = 0f;
        float duration = Mathf.Abs(startOffsetX - completeTargetOffsetX) / slideSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Ease-out 효과 적용
            t = 1f - Mathf.Pow(1f - t, 3f);

            if (completeRectTransform != null)
            {
                Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, t);
                completeRectTransform.anchoredPosition = newPosition;
            }

            // Complete 이미지 페이드 인 (0 -> 1)
            if (completeCanvasGroup != null)
            {
                completeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }

        // 최종 위치 및 알파값 보정
        if (completeRectTransform != null)
        {
            completeRectTransform.anchoredPosition = targetPosition;
        }

        if (completeCanvasGroup != null)
        {
            completeCanvasGroup.alpha = 1f;
        }

        // 화면에 표시
        yield return new WaitForSeconds(displayDuration);

        // 페이드 아웃
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeSpeed)
        {
            fadeElapsed += Time.deltaTime;
            float alpha = 1f - (fadeElapsed / fadeSpeed);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }

            if (completeCanvasGroup != null)
            {
                completeCanvasGroup.alpha = alpha;
            }

            if (backgroundCanvasGroup != null)
            {
                backgroundCanvasGroup.alpha = alpha;
            }

            if (dungeonNameCanvasGroup != null)
            {
                dungeonNameCanvasGroup.alpha = alpha;
            }

            yield return null;
        }

        // 패널 비활성화
        if (completePanel != null)
        {
            completePanel.SetActive(false);
        }
    }

    /// <summary>
    /// UI 요소를 슬라이딩하면서 페이드 인하는 코루틴
    /// </summary>
    private IEnumerator SlideAndFadeIn(RectTransform rectTransform, CanvasGroup canvasGroup, float speed, float targetX)
    {
        if (rectTransform == null || canvasGroup == null)
        {
            yield break;
        }

        // 시작 위치 설정 (오른쪽 밖)
        Vector2 startPos = rectTransform.anchoredPosition;
        startPos.x = startOffsetX;
        rectTransform.anchoredPosition = startPos;

        // 목표 위치
        Vector2 targetPos = startPos;
        targetPos.x = targetX;

        // 알파값 0으로 시작
        canvasGroup.alpha = 0f;

        // 슬라이딩 + 페이드 인
        float elapsedTime = 0f;
        float duration = Mathf.Abs(startOffsetX - targetX) / speed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Ease-out 효과
            t = 1f - Mathf.Pow(1f - t, 3f);

            // 위치 업데이트
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            // 알파값 업데이트 (0 -> 1)
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        // 최종 값 보정
        rectTransform.anchoredPosition = targetPos;
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 외부에서 던전 클리어를 트리거하는 예시 메서드
    /// </summary>
    public void OnDungeonCompleted()
    {
        ShowDungeonClear("몽글몽글 연덕");
    }
}