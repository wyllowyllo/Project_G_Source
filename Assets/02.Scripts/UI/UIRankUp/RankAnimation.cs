using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class RankAnimation : MonoBehaviour
{
    [Header("UI 요소")]
    [Tooltip("마름모 형태의 랭크 배경 이미지")]
    public GameObject rankUpPanel;
    public Image diamondBackground;

    [Tooltip("랭크 텍스트 (B, A, S 등)")]
    public TextMeshProUGUI rankText;

    [Tooltip("랭크 강화 메시지 텍스트")]
    public TextMeshProUGUI messageText;

    [Tooltip("마름모 테두리 이미지")]
    public Image diamondBorder;

    [Header("메시지 설정")]
    [Tooltip("랭크 업 메시지 (기본값)")]
    public string rankUpMessage = "랭크 강화!";

    [Tooltip("애니메이션 시작 시 UI 자동 활성화")]
    public bool autoActivateUI = true;

    [Tooltip("애니메이션 종료 시 UI 자동 비활성화")]
    public bool autoDeactivateUI = false;

    [Tooltip("UI 비활성화 딜레이 (초)")]
    public float deactivateDelay = 2f;

    [Header("파티클 시스템")]
    [Tooltip("파티클 프리팹")]
    public GameObject particlePrefab;

    [Tooltip("별 파티클 프리팹")]
    public GameObject starParticlePrefab;

    [Header("광선 설정")]
    [Tooltip("광선 프리팹")]
    public GameObject lightRayPrefab;

    [Tooltip("광선 개수")]
    [Range(8, 32)]
    public int rayCount = 16;

    [Header("색상 설정 (등급별)")]
    public RankColorScheme[] rankColorSchemes = new RankColorScheme[]
    {
        new RankColorScheme { rank = "D", primaryColor = new Color(0.545f, 0.451f, 0.333f), secondaryColor = new Color(0.419f, 0.325f, 0.271f) },
        new RankColorScheme { rank = "C", primaryColor = new Color(0.486f, 0.702f, 0.259f), secondaryColor = new Color(0.333f, 0.545f, 0.184f) },
        new RankColorScheme { rank = "B", primaryColor = new Color(0.784f, 1f, 0f), secondaryColor = new Color(0.620f, 0.792f, 0f) },
        new RankColorScheme { rank = "A", primaryColor = new Color(1f, 0.843f, 0f), secondaryColor = new Color(1f, 0.549f, 0f) },
        new RankColorScheme { rank = "S", primaryColor = new Color(1f, 0.431f, 0.780f), secondaryColor = new Color(0.749f, 0.243f, 1f) }
    };

    [Header("애니메이션 설정")]
    [Tooltip("애니메이션 지속 시간")]
    public float duration = 1.5f;

    [Tooltip("플래시 효과 사용")]
    public bool useFlash = true;

    [Tooltip("카메라 쉐이크 강도")]
    [Range(0f, 1f)]
    public float cameraShakeIntensity = 0.2f;

    [Header("이펙트 색상")]
    public Color cyanGlow = new Color(0f, 1f, 0.949f);  // 시안
    public Color purpleGlow = new Color(0.749f, 0.243f, 1f);  // 보라
    public Color flashColor = Color.white;

    [Header("사운드")]
    [Tooltip("랭크 강화 효과음")]
    public AudioClip rankUpSound;

    private AudioSource audioSource;
    private Camera mainCamera;
    private Vector3 originalCameraPos;
    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform diamondRect;

    [System.Serializable]
    public class RankColorScheme
    {
        public string rank;
        public Color primaryColor;
        public Color secondaryColor;
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        mainCamera = Camera.main;
        if (mainCamera != null)
            originalCameraPos = mainCamera.transform.position;

        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();

        if (diamondBackground != null)
            diamondRect = diamondBackground.GetComponent<RectTransform>();
    }

    /// <summary>
    /// 랭크 강화 애니메이션 재생
    /// </summary>
    /// <param name="newRank">새로운 랭크 문자</param>
    /// <param name="customMessage">커스텀 메시지 (선택사항)</param>
    public void PlayRankUpAnimation(string newRank, string customMessage = null)
    {
        // UI 활성화
        if (autoActivateUI && rankUpPanel != null)
        {
            rankUpPanel.SetActive(true);  // RankUpPanel 활성화!
        }

        // 텍스트 설정
        if (rankText != null)
        {
            rankText.text = newRank;
            rankText.gameObject.SetActive(true);

            // 랭크 색상 적용
            RankColorScheme colorScheme = GetColorScheme(newRank);
            rankText.color = colorScheme.primaryColor;

            // 텍스트 그림자/아웃라인 강화 (더 잘 보이게)
            var outline = rankText.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = new Color(0, 0, 0, 0.8f);
                outline.effectDistance = new Vector2(3, -3);
            }
        }

        if (messageText != null)
        {
            messageText.text = string.IsNullOrEmpty(customMessage) ? rankUpMessage : customMessage;
            messageText.gameObject.SetActive(true);

            // 메시지 색상 (흰색으로 선명하게)
            messageText.color = Color.white;

            // 텍스트 그림자/아웃라인 강화
            var outline = messageText.GetComponent<Outline>();
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
        if (rankUpSound != null && audioSource != null)
            audioSource.PlayOneShot(rankUpSound);

        // 등급별 색상 가져오기
        RankColorScheme colorScheme = GetColorScheme(rank);

        // 1. 플래시 효과 (50ms)
        if (useFlash)
            StartCoroutine(CreateFlash());

        yield return new WaitForSeconds(0.05f);

        // 2. 십자가 광선 (100ms 후)
        StartCoroutine(CreateCrossBeam());
        yield return new WaitForSeconds(0.1f);

        // 3. 마름모 충격파 (150ms 간격으로 5개)
        for (int i = 0; i < 5; i++)
        {
            CreateDiamondShockwave(colorScheme);
            yield return new WaitForSeconds(0.08f);
        }

        // 4. 광선 폭발 (동시)
        StartCoroutine(CreateLightRays(colorScheme));

        // 5. 파티클 폭발 (50ms 후)
        yield return new WaitForSeconds(0.05f);
        StartCoroutine(CreateParticleExplosion(colorScheme));

        // 6. 별 파티클 (50ms 후)
        yield return new WaitForSeconds(0.05f);
        StartCoroutine(CreateStarExplosion());

        // 7. 글로우 링 (100ms 후)
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < 3; i++)
        {
            CreateGlowRing(colorScheme);
            yield return new WaitForSeconds(0.15f);
        }

        // 8. 마름모 애니메이션 (처음부터)
        StartCoroutine(AnimateDiamond());

        // 9. 카메라 쉐이크
        if (mainCamera != null && cameraShakeIntensity > 0)
            StartCoroutine(CameraShake(0.3f, cameraShakeIntensity));

        // 10. 자동 비활성화 (옵션)
        if (autoDeactivateUI)
        {
            yield return new WaitForSeconds(deactivateDelay);
            gameObject.SetActive(false);
        }

        if (autoActivateUI && rankUpPanel != null)
        {
            yield return new WaitForSeconds(deactivateDelay);
            rankUpPanel.SetActive(false);  // RankUpPanel 활성화!
        }
    }

    #region Effect Creation Methods

    /// <summary>
    /// 플래시 효과
    /// </summary>
    private IEnumerator CreateFlash()
    {
        GameObject flash = new GameObject("Flash");
        flash.transform.SetParent(canvas.transform);
        flash.transform.SetAsLastSibling();

        Image img = flash.AddComponent<Image>();
        img.color = flashColor;

        RectTransform rect = img.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 0.6f * (1f - elapsed / duration);
            img.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        Destroy(flash);
    }

    /// <summary>
    /// 십자가 광선 효과
    /// </summary>
    private IEnumerator CreateCrossBeam()
    {
        // 수평 빔
        GameObject horizontal = CreateBeam("HorizontalBeam", true);
        RectTransform hRect = horizontal.GetComponent<RectTransform>();
        Image hImg = horizontal.GetComponent<Image>();

        // 수직 빔
        GameObject vertical = CreateBeam("VerticalBeam", false);
        RectTransform vRect = vertical.GetComponent<RectTransform>();
        Image vImg = vertical.GetComponent<Image>();

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 수평 빔 애니메이션
            if (t < 0.3f)
            {
                float progress = t / 0.3f;
                hRect.sizeDelta = new Vector2(Mathf.Lerp(0f, 2000f, progress), 8f);
                hImg.color = new Color(cyanGlow.r, cyanGlow.g, cyanGlow.b, 1f);
            }
            else
            {
                float fadeProgress = (t - 0.3f) / 0.7f;
                hImg.color = new Color(cyanGlow.r, cyanGlow.g, cyanGlow.b, 1f - fadeProgress);
            }

            // 수직 빔 애니메이션
            if (t < 0.3f)
            {
                float progress = t / 0.3f;
                vRect.sizeDelta = new Vector2(8f, Mathf.Lerp(0f, 1500f, progress));
                vImg.color = new Color(cyanGlow.r, cyanGlow.g, cyanGlow.b, 1f);
            }
            else
            {
                float fadeProgress = (t - 0.3f) / 0.7f;
                vImg.color = new Color(cyanGlow.r, cyanGlow.g, cyanGlow.b, 1f - fadeProgress);
            }

            yield return null;
        }

        Destroy(horizontal);
        Destroy(vertical);
    }

    private GameObject CreateBeam(string name, bool isHorizontal)
    {
        GameObject beam = new GameObject(name);
        beam.transform.SetParent(canvas.transform);
        beam.transform.position = transform.position;

        Image img = beam.AddComponent<Image>();
        RectTransform rect = img.rectTransform;

        // 그라데이션 텍스처 생성
        Texture2D texture = new Texture2D(isHorizontal ? 100 : 10, isHorizontal ? 10 : 100);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float alpha;
                if (isHorizontal)
                {
                    float distFromCenter = Mathf.Abs(x - 50f) / 50f;
                    alpha = 1f - distFromCenter;
                }
                else
                {
                    float distFromCenter = Mathf.Abs(y - 50f) / 50f;
                    alpha = 1f - distFromCenter;
                }
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply();

        img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        img.color = cyanGlow;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = Vector2.zero;

        return beam;
    }

    /// <summary>
    /// 마름모 충격파
    /// </summary>
    private void CreateDiamondShockwave(RankColorScheme colorScheme)
    {
        GameObject shockwave = new GameObject("DiamondShockwave");
        shockwave.transform.SetParent(canvas.transform);
        shockwave.transform.position = transform.position;
        shockwave.transform.rotation = Quaternion.Euler(0f, 0f, 45f);

        Image img = shockwave.AddComponent<Image>();
        RectTransform rect = img.rectTransform;

        // 마름모 테두리 텍스처
        Texture2D texture = CreateDiamondBorderTexture(100);
        img.sprite = Sprite.Create(texture, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
        img.color = colorScheme.primaryColor;

        rect.sizeDelta = new Vector2(180f, 180f);

        StartCoroutine(AnimateShockwave(shockwave, rect, img));
    }

    private IEnumerator AnimateShockwave(GameObject obj, RectTransform rect, Image img)
    {
        float duration = 1.0f;
        float elapsed = 0f;
        Vector2 startSize = new Vector2(180f, 180f);
        Vector2 endSize = new Vector2(800f, 800f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rect.sizeDelta = Vector2.Lerp(startSize, endSize, t);

            Color col = img.color;
            col.a = 1f - t;
            img.color = col;

            yield return null;
        }

        Destroy(obj);
    }

    /// <summary>
    /// 광선 폭발
    /// </summary>
    private IEnumerator CreateLightRays(RankColorScheme colorScheme)
    {
        Color[] rayColors = { cyanGlow, colorScheme.primaryColor, purpleGlow };

        for (int i = 0; i < rayCount; i++)
        {
            float angle = (360f / rayCount) * i;
            float height = Random.Range(400f, 600f);
            Color rayColor = rayColors[i % rayColors.Length];

            CreateLightRay(angle, height, rayColor);

            if (i % 4 == 0)
                yield return null;
        }
    }

    private void CreateLightRay(float angle, float height, Color color)
    {
        GameObject ray = new GameObject("LightRay");
        ray.transform.SetParent(canvas.transform);
        ray.transform.position = transform.position;
        ray.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Image img = ray.AddComponent<Image>();
        RectTransform rect = img.rectTransform;

        // 그라데이션 텍스처
        Texture2D texture = new Texture2D(10, 100);
        for (int y = 0; y < 100; y++)
        {
            float alpha = y < 20 ? 0f : (y < 70 ? (y - 20f) / 50f : 1f - (y - 70f) / 30f);
            for (int x = 0; x < 10; x++)
            {
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply();

        img.sprite = Sprite.Create(texture, new Rect(0, 0, 10, 100), new Vector2(0.5f, 0f));
        img.color = color;

        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(Random.Range(4f, 8f), 0f);

        StartCoroutine(AnimateLightRay(ray, rect, img, height));
    }

    private IEnumerator AnimateLightRay(GameObject ray, RectTransform rect, Image img, float targetHeight)
    {
        float duration = 1.2f;
        float elapsed = 0f;
        float delay = Random.Range(0f, 0.1f);

        yield return new WaitForSeconds(delay);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (t < 0.3f)
            {
                float progress = t / 0.3f;
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, Mathf.Lerp(0f, targetHeight, progress));
                img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
            }
            else
            {
                float fadeProgress = (t - 0.3f) / 0.7f;
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, targetHeight * (1f + fadeProgress * 0.2f));
                img.color = new Color(img.color.r, img.color.g, img.color.b, 1f - fadeProgress);
            }

            yield return null;
        }

        Destroy(ray);
    }

    /// <summary>
    /// 파티클 폭발
    /// </summary>
    private IEnumerator CreateParticleExplosion(RankColorScheme colorScheme)
    {
        int particleCount = 60;
        Color[] particleColors = { colorScheme.primaryColor, cyanGlow, Color.white, colorScheme.secondaryColor };

        for (int i = 0; i < particleCount; i++)
        {
            float angle = (Mathf.PI * 2f * i) / particleCount;
            float velocity = Random.Range(200f, 400f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Color color = particleColors[Random.Range(0, particleColors.Length)];

            CreateParticle(direction * velocity, color, Random.Range(0.8f, 1.5f));

            if (i % 10 == 0)
                yield return null;
        }
    }

    private void CreateParticle(Vector2 velocity, Color color, float lifetime)
    {
        GameObject particle = new GameObject("Particle");
        particle.transform.SetParent(canvas.transform);
        particle.transform.position = transform.position;

        Image img = particle.AddComponent<Image>();
        img.color = color;

        RectTransform rect = img.rectTransform;
        float size = Random.Range(6f, 12f);
        rect.sizeDelta = new Vector2(size, size);

        // 원형 텍스처
        Texture2D texture = new Texture2D(10, 10);
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(5, 5));
                texture.SetPixel(x, y, dist < 5f ? Color.white : Color.clear);
            }
        }
        texture.Apply();
        img.sprite = Sprite.Create(texture, new Rect(0, 0, 10, 10), new Vector2(0.5f, 0.5f));

        StartCoroutine(AnimateParticle(particle, velocity, lifetime, img));
    }

    private IEnumerator AnimateParticle(GameObject particle, Vector2 velocity, float lifetime, Image img)
    {
        float elapsed = 0f;
        Vector3 startPos = particle.transform.position;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // 이동
            particle.transform.position = startPos + (Vector3)velocity * t * Time.deltaTime * 60f;

            // 페이드 아웃
            Color col = img.color;
            col.a = 1f - t;
            img.color = col;

            // 스케일 축소
            particle.transform.localScale = Vector3.one * (1f - t * 0.5f);

            yield return null;
        }

        Destroy(particle);
    }

    /// <summary>
    /// 별 폭발
    /// </summary>
    private IEnumerator CreateStarExplosion()
    {
        int starCount = 30;
        string[] starChars = { "✦", "✧", "⭐", "✨" };

        for (int i = 0; i < starCount; i++)
        {
            float angle = (Mathf.PI * 2f * i) / starCount + Random.Range(-0.5f, 0.5f);
            float velocity = Random.Range(150f, 300f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            CreateStarParticle(direction * velocity, starChars[Random.Range(0, starChars.Length)], Random.Range(1.0f, 1.5f));

            if (i % 6 == 0)
                yield return null;
        }
    }

    private void CreateStarParticle(Vector2 velocity, string starChar, float lifetime)
    {
        GameObject star = new GameObject("StarParticle");
        star.transform.SetParent(canvas.transform);
        star.transform.position = transform.position;

        TextMeshProUGUI text = star.AddComponent<TextMeshProUGUI>();
        text.text = starChar;
        text.fontSize = Random.Range(24f, 36f);
        text.color = new Color(0.784f, 1f, 0f); // 노란-초록
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = text.rectTransform;
        rect.sizeDelta = new Vector2(50f, 50f);

        StartCoroutine(AnimateStarParticle(star, velocity, lifetime, text));
    }

    private IEnumerator AnimateStarParticle(GameObject star, Vector2 velocity, float lifetime, TextMeshProUGUI text)
    {
        float elapsed = 0f;
        Vector3 startPos = star.transform.position;
        float rotationSpeed = Random.Range(360f, 720f);

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // 이동
            star.transform.position = startPos + (Vector3)velocity * t * Time.deltaTime * 60f;

            // 회전
            star.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            // 페이드 아웃
            Color col = text.color;
            col.a = 1f - t;
            text.color = col;

            // 스케일
            star.transform.localScale = Vector3.one * (1f - t * 0.5f);

            yield return null;
        }

        Destroy(star);
    }

    /// <summary>
    /// 글로우 링
    /// </summary>
    private void CreateGlowRing(RankColorScheme colorScheme)
    {
        GameObject ring = new GameObject("GlowRing");
        ring.transform.SetParent(canvas.transform);
        ring.transform.position = transform.position;

        Image img = ring.AddComponent<Image>();
        RectTransform rect = img.rectTransform;

        // 원형 테두리 텍스처
        Texture2D texture = new Texture2D(100, 100);
        for (int y = 0; y < 100; y++)
        {
            for (int x = 0; x < 100; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(50, 50));
                if (dist > 42f && dist < 50f)
                    texture.SetPixel(x, y, Color.white);
                else
                    texture.SetPixel(x, y, Color.clear);
            }
        }
        texture.Apply();

        img.sprite = Sprite.Create(texture, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
        img.color = colorScheme.primaryColor;

        rect.sizeDelta = new Vector2(200f, 200f);

        StartCoroutine(AnimateGlowRing(ring, rect, img));
    }

    private IEnumerator AnimateGlowRing(GameObject ring, RectTransform rect, Image img)
    {
        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float size = Mathf.Lerp(200f, 1200f, t);
            rect.sizeDelta = new Vector2(size, size);

            Color col = img.color;
            col.a = 1f - t;
            img.color = col;

            yield return null;
        }

        Destroy(ring);
    }

    /// <summary>
    /// 마름모 애니메이션
    /// </summary>
    private IEnumerator AnimateDiamond()
    {
        if (diamondBackground == null) yield break;

        Transform diamondTransform = diamondBackground.transform;
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
        if (rankText != null)
        {
            for (int i = 0; i < 2; i++)
            {
                yield return StartCoroutine(PulseText(rankText.transform, 0.4f));
            }
        }

        // 메시지 텍스트도 펄스
        if (messageText != null)
        {
            StartCoroutine(PulseText(messageText.transform, 0.5f));
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

    /// <summary>
    /// 카메라 쉐이크
    /// </summary>
    private IEnumerator CameraShake(float duration, float intensity)
    {
        if (mainCamera == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = intensity * (1f - elapsed / duration);

            mainCamera.transform.position = originalCameraPos + Random.insideUnitSphere * strength;

            yield return null;
        }

        mainCamera.transform.position = originalCameraPos;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 등급에 맞는 색상 스킴 가져오기
    /// </summary>
    private RankColorScheme GetColorScheme(string rank)
    {
        foreach (var scheme in rankColorSchemes)
        {
            if (scheme.rank == rank)
                return scheme;
        }

        // 기본값 (B등급)
        return rankColorSchemes.Length >= 3 ? rankColorSchemes[2] : rankColorSchemes[0];
    }

    /// <summary>
    /// 마름모 테두리 텍스처 생성
    /// </summary>
    private Texture2D CreateDiamondBorderTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        int center = size / 2;
        int borderThickness = 4;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // 마름모 거리 계산
                int dx = Mathf.Abs(x - center);
                int dy = Mathf.Abs(y - center);
                int dist = dx + dy;

                // 테두리 영역
                if (dist > center - borderThickness && dist <= center)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();
        return texture;
    }

    #endregion
}