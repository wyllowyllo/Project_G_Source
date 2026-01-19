using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SkillRewardItem : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image glowImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private ParticleSystem glowParticles;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float expandDuration = 0.5f;
    [SerializeField] private float glowDuration = 1f;
    [SerializeField] private AnimationCurve expandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalScale;
    
    private Coroutine showCoroutine;
    private Coroutine glowCoroutine;
    private Coroutine rotateCoroutine;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        originalScale = rectTransform.localScale;
    }
    
    public void Initialize(SkillRewardData data)
    {
        // 데이터 설정
        if (iconImage != null && data.SkillIcon != null)
        {
            iconImage.sprite = data.SkillIcon;
        }
        
        if (skillNameText != null)
        {
            skillNameText.text = data.SkillName;
        }
        
        if (levelText != null)
        {
            levelText.text = $"Lv.{data.PreviousLevel} → Lv.{data.NewLevel}";
        }      
        
        // 초기 상태 설정
        rectTransform.localScale = Vector2.zero;
        canvasGroup.alpha = 0f;
        
        if (glowImage != null)
        {
            glowImage.enabled = false;
        }
    }
    
    public void PlayShowAnimation(float delay, System.Action onComplete = null)
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }
        showCoroutine = StartCoroutine(ShowAnimationCoroutine(delay, onComplete));
    }
    
    private IEnumerator ShowAnimationCoroutine(float delay, System.Action onComplete)
    {
        // 지연
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        
        float elapsed = 0f;
        Vector2 startScale = Vector2.zero;
        float startAlpha = 0f;
        
        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expandDuration;
            float curveValue = expandCurve.Evaluate(t);
            
            // 스케일 애니메이션
            rectTransform.localScale = Vector2.Lerp(startScale, originalScale, curveValue);
            
            // 페이드 애니메이션
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, curveValue);
            
            yield return null;
        }
        
        // 최종 값 설정
        rectTransform.localScale = originalScale;
        canvasGroup.alpha = 1f;
        
        // 빛나는 효과 시작
        PlayGlowEffect();
        
        onComplete?.Invoke();
    }
    
    // 빛나는 효과
    private void PlayGlowEffect()
    {
        if (glowImage != null)
        {
            glowImage.enabled = true;
            
            // 글로우 펄스 효과
            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
            }
            glowCoroutine = StartCoroutine(GlowPulseCoroutine());
        }
        
        // 파티클 효과
        if (glowParticles != null)
        {
            glowParticles.Play();
        }
    }
    
    /// 글로우 펄스 코루틴
    private IEnumerator GlowPulseCoroutine()
    {
        if (glowImage == null) yield break;
        
        Color glowColor = glowImage.color;
        
        while (true)
        {
            // 페이드 인
            float elapsed = 0f;
            float duration = glowDuration * 0.3f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                glowColor.a = Mathf.Lerp(0.3f, 1f, t);
                glowImage.color = glowColor;
                yield return null;
            }
            
            // 페이드 아웃
            elapsed = 0f;
            duration = glowDuration * 0.7f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                glowColor.a = Mathf.Lerp(1f, 0.3f, t);
                glowImage.color = glowColor;
                yield return null;
            }
        }
    }
    
    // 사라지는 애니메이션
    public void PlayHideAnimation(System.Action onComplete = null)
    {
        StartCoroutine(HideAnimationCoroutine(onComplete));
    }
    
    private IEnumerator HideAnimationCoroutine(System.Action onComplete)
    {
        float elapsed = 0f;
        float duration = expandDuration * 0.5f;
        Vector2 startScale = rectTransform.localScale;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 스케일 애니메이션
            rectTransform.localScale = Vector2.Lerp(startScale, Vector2.zero, t);
            
            // 페이드 애니메이션
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            
            yield return null;
        }
        
        // 최종 값 설정
        rectTransform.localScale = Vector2.zero;
        canvasGroup.alpha = 0f;
        
        onComplete?.Invoke();
    }
    
    public void Cleanup()
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }
        
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }
        
        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
        }
        
        if (glowParticles != null)
        {
            glowParticles.Stop();
        }
    }
    
    private void OnDestroy()
    {
        Cleanup();
    }
}
