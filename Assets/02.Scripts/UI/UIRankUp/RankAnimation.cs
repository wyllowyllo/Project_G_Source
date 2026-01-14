using System.Collections;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public float deactivateDelay = 4f;

    [Header("파티클 시스템")]
    [Tooltip("랭크 업 파티클 시스템")]
    public ParticleSystem RankUpParticleSystem;

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

        // 파티클 시스템만 재생!
        if (RankUpParticleSystem != null)
        {
            RankUpParticleSystem.Stop();
            RankUpParticleSystem.Clear();
            RankUpParticleSystem.Play();
        }

        // 마름모와 텍스트 애니메이션
        StartCoroutine(AnimateDiamond());

        // 자동 비활성화
        if (autoDeactivateUI)
        {
            yield return new WaitForSeconds(deactivateDelay);
            rankUpPanel.SetActive(false);
            rankText.gameObject.SetActive(false);
            messageText.gameObject.SetActive(false);
        }
    }

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
        rect.anchorMin = Vector3.zero;
        rect.anchorMax = Vector3.one;
        rect.sizeDelta = Vector3.zero;

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

    /// 마름모 애니메이션
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

    /// 등급에 맞는 색상 가져오기
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
}