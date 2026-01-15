using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTooltip : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI cooldownValueText;
    public RectTransform tooltipRect;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float horizontalSpacing = 40f; // 슬롯과 툴팁 사이 간격
    public float fadeSpeed = 2f;
    
    private bool isVisible = false;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (tooltipRect == null)
        {
            tooltipRect = GetComponent<RectTransform>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // Start invisible
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public void ShowTooltip(SkillData skillData, RectTransform slotTransform)
    {
        if (skillData == null || slotTransform == null) return;

        // Update text content
        if (skillNameText != null)
        {
            skillNameText.text = skillData.skillName;
        }

        if (levelText != null)
        {
            levelText.text = $"레벨 {skillData.level}";
        }

        if (descriptionText != null)
        {
            descriptionText.text = skillData.description;
        }

        if (cooldownValueText != null)
        {
            cooldownValueText.text = skillData.cooldown;
        }

        // Position tooltip to the left of slot
        PositionTooltipLeft(slotTransform);

        // Show with fade in
        gameObject.SetActive(true);
        isVisible = true;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeIn());
    }

    public void HideTooltip()
    {
        isVisible = false;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
          
        fadeCoroutine = StartCoroutine(FadeOut());
    }

    private void PositionTooltipLeft(RectTransform slotTransform)
    {
        if (tooltipRect == null || slotTransform == null) return;

        // Get canvas
        RectTransform canvasRect = tooltipRect.parent as RectTransform;

        // Convert slot position to canvas local space
        Vector2 slotLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, slotTransform.position),
            null,
            out slotLocalPos
        );

        // 툴팁 크기
        float tooltipWidth = tooltipRect.sizeDelta.x;
        float tooltipHeight = tooltipRect.sizeDelta.y;

        // 슬롯 크기
        float slotWidth = slotTransform.sizeDelta.x;
        float slotHeight = slotTransform.sizeDelta.y;

        // 툴팁 위치 계산:
        // 슬롯 왼쪽 끝 - 툴팁 반 너비 - 간격px
        Vector2 tooltipPos = new Vector2(slotLocalPos.x - (slotWidth * 0.5f) - (tooltipWidth * 0.5f) - horizontalSpacing,slotLocalPos.y);

        // 툴팁의 앵커가 중앙이므로 그대로 사용
        tooltipRect.anchoredPosition = tooltipPos;

        // Keep tooltip within screen bounds
        ClampToScreen();
    }

    private void ClampToScreen()
    {
        if (tooltipRect == null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        Vector2 tooltipSize = tooltipRect.sizeDelta;
        Vector2 currentPos = tooltipRect.anchoredPosition;

        // Calculate bounds (툴팁 앵커가 center이므로 절반씩 계산)
        float minX = -canvasSize.x / 2 + tooltipSize.x / 2;
        float maxX = canvasSize.x / 2 - tooltipSize.x / 2;
        float minY = -canvasSize.y / 2 + tooltipSize.y / 2;
        float maxY = canvasSize.y / 2 - tooltipSize.y / 2;

        // Clamp position
        currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);
        currentPos.y = Mathf.Clamp(currentPos.y, minY, maxY);

        tooltipRect.anchoredPosition = currentPos;
    }

    private System.Collections.IEnumerator FadeIn()
    {
        canvasGroup.blocksRaycasts = false;

        while (canvasGroup.alpha < 0.99f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private System.Collections.IEnumerator FadeOut()
    {
        canvasGroup.blocksRaycasts = false;

        while (canvasGroup.alpha > 0.01f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}