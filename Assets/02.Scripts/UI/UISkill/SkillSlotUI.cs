using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SkillSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image iconImage;
    public Image glowImage;
    public TextMeshProUGUI levelText;
    public Image borderImage;

    [Header("Visual Settings")]
    public Color normalBorderColor = new Color(0.39f, 1f, 0.59f, 0.3f);
    public Color hoverBorderColor = new Color(0.39f, 1f, 0.59f, 0.8f);
    public float hoverScale = 1.05f;
    public float animationDuration = 0.3f;

    [Header("Lock UI")]


    private SkillData skillData;
    private SkillTooltip tooltip;
    private Vector3 originalScale;
    private bool isAnimating = false;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;

        if (glowImage != null)
        {
            glowImage.color = new Color(glowImage.color.r, glowImage.color.g, glowImage.color.b, 0);
        }
    }

    public void Initialize(SkillData data, SkillTooltip tooltipRef)
    {
        skillData = data;
        tooltip = tooltipRef;

        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
        }

        if (levelText != null)
        {
            levelText.text = $"Lv.{data.level}";
        }

        if (borderImage != null)
        {
            borderImage.color = normalBorderColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillData != null && tooltip != null)
        {
            // RectTransform을 직접 전달
            tooltip.ShowTooltip(skillData, rectTransform);
        }

        if (!isAnimating)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateHover(true));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.HideTooltip();
        }

        if (!isAnimating)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateHover(false));
        }
    }

    private System.Collections.IEnumerator AnimateHover(bool isHovering)
    {
        isAnimating = true;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = isHovering ? originalScale * hoverScale : originalScale;

        Color startBorderColor = borderImage != null ? borderImage.color : Color.white;
        Color targetBorderColor = isHovering ? hoverBorderColor : normalBorderColor;

        float startGlowAlpha = glowImage != null ? glowImage.color.a : 0;
        float targetGlowAlpha = isHovering ? 0.7f : 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            t = Mathf.SmoothStep(0, 1, t);

            // Scale animation
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            // Border color animation
            if (borderImage != null)
            {
                borderImage.color = Color.Lerp(startBorderColor, targetBorderColor, t);
            }

            // Glow animation
            if (glowImage != null)
            {
                Color glowColor = glowImage.color;
                glowColor.a = Mathf.Lerp(startGlowAlpha, targetGlowAlpha, t);
                glowImage.color = glowColor;
            }

            yield return null;
        }

        // Ensure final values
        transform.localScale = targetScale;
        if (borderImage != null)
        {
            borderImage.color = targetBorderColor;
        }
        if (glowImage != null)
        {
            Color glowColor = glowImage.color;
            glowColor.a = targetGlowAlpha;
            glowImage.color = glowColor;
        }

        isAnimating = false;
    }

    public void PlaySlideInAnimation(float delay)
    {
        StartCoroutine(SlideInAnimation(delay));
    }

    private System.Collections.IEnumerator SlideInAnimation(float delay)
    {
        // Initial state
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0;
        Vector3 startPos = transform.localPosition + new Vector3(50, 0, 0);
        Vector3 targetPos = transform.localPosition;
        transform.localPosition = startPos;

        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0, 1, t);

            canvasGroup.alpha = t;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        canvasGroup.alpha = 1;
        transform.localPosition = targetPos;
    }
}