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
    public Vector2 offset = new Vector2(20f, 0f);
    public float fadeSpeed = 5f;
    
    private bool isVisible = false;
    
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
        gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (isVisible && canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
        }
        else if (!isVisible && canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
            
            if (canvasGroup.alpha < 0.01f)
            {
                gameObject.SetActive(false);
            }
        }
    }
    
    public void ShowTooltip(SkillData skillData, Vector3 slotPosition)
    {
        if (skillData == null) return;
        
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
        
        // Position tooltip
        PositionTooltip(slotPosition);
        
        // Show
        gameObject.SetActive(true);
        isVisible = true;
    }
    
    public void HideTooltip()
    {
        isVisible = false;
    }
    
    private void PositionTooltip(Vector3 slotPosition)
    {
        if (tooltipRect == null) return;
        
        // Convert world position to screen position
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, slotPosition);
        
        // Add offset
        screenPos += offset;
        
        // Convert screen position to local position in canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipRect.parent as RectTransform,
            screenPos,
            Camera.main,
            out Vector2 localPos
        );
        
        tooltipRect.localPosition = localPos;
        
        // Keep tooltip within screen bounds
        ClampToScreen();
    }
    
    private void ClampToScreen()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        tooltipRect.GetWorldCorners(corners);
        
        Vector3 localPos = tooltipRect.localPosition;
        
        // Check right edge
        if (RectTransformUtility.WorldToScreenPoint(Camera.main, corners[2]).x > Screen.width)
        {
            localPos.x -= (RectTransformUtility.WorldToScreenPoint(Camera.main, corners[2]).x - Screen.width);
        }
        
        // Check left edge
        if (RectTransformUtility.WorldToScreenPoint(Camera.main, corners[0]).x < 0)
        {
            localPos.x -= RectTransformUtility.WorldToScreenPoint(Camera.main, corners[0]).x;
        }
        
        // Check top edge
        if (RectTransformUtility.WorldToScreenPoint(Camera.main, corners[1]).y > Screen.height)
        {
            localPos.y -= (RectTransformUtility.WorldToScreenPoint(Camera.main, corners[1]).y - Screen.height);
        }
        
        // Check bottom edge
        if (RectTransformUtility.WorldToScreenPoint(Camera.main, corners[0]).y < 0)
        {
            localPos.y -= RectTransformUtility.WorldToScreenPoint(Camera.main, corners[0]).y;
        }
        
        tooltipRect.localPosition = localPos;
    }
}
