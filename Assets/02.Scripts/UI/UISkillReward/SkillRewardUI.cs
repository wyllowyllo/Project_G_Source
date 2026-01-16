using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SkillRewardUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private Image backgroundDimmer;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private SkillRewardItem itemPrefab;
    [SerializeField] private TMPro.TextMeshProUGUI titleText;
    [SerializeField] private TMPro.TextMeshProUGUI subText;

    [Header("중앙 확장 패널")]
    [SerializeField] private RectTransform centerPanel;
    [SerializeField] private float targetPanelHeight = 500f;

    [Header("애니메이션 설정")]
    [SerializeField] private float panelExpandDuration = 0.6f;
    [SerializeField] private float itemDelayBetween = 0.2f;
    [SerializeField] private AnimationCurve panelExpandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("자동 닫기 설정")]
    [Tooltip("UI가 나타난 후 자동으로 닫히기까지의 시간 (초)")]
    [SerializeField] private float autoCloseDelay = 2f;

    [Tooltip("자동 닫기 기능 사용")]
    [SerializeField] private bool useAutoClose = true;

    [Header("사운드 (옵션)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip itemAppearSound;
    [SerializeField] private AudioSource audioSource;

    private List<SkillRewardItem> activeItems = new List<SkillRewardItem>();
    private bool isShowing = false;
    private bool isInitialized = false;
    private Coroutine autoCloseCoroutine;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;

        // 초기 상태로 숨기기
        HideRewardPanelImmediate();

        // 타이틀 텍스트 초기화
        if (titleText != null)
        {
            titleText.gameObject.SetActive(false);
        }

        if (subText != null)
        {
            subText.gameObject.SetActive(false);
        }

        isInitialized = true;
    }

    public void ShowRewards(SkillRewardData[] rewards)
    {
        if (!isInitialized)
        {
            Initialize();
        }

        if (isShowing)
        {
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        // 기존 자동 닫기 코루틴 취소
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        StartCoroutine(ShowRewardsCoroutine(rewards));
    }

    private IEnumerator ShowRewardsCoroutine(SkillRewardData[] rewards)
    {
        isShowing = true;

        // 패널 활성화
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
        }

        yield return StartCoroutine(ExpandCenterPanel());

        if (titleText != null)
        {
            StartCoroutine(ShowTitleText());
        }

        if (subText != null)
        {
            StartCoroutine(ShowSubText());
        }

        PlaySound(openSound);

        for (int i = 0; i < rewards.Length; i++)
        {
            CreateAndShowItem(rewards[i], i * itemDelayBetween);

            if (itemAppearSound != null)
            {
                StartCoroutine(PlaySoundDelayed(itemAppearSound, i * itemDelayBetween));
            }
        }

        // 모든 애니메이션 완료 대기
        float totalAnimationTime = panelExpandDuration + rewards.Length * itemDelayBetween + 1f;
        yield return new WaitForSeconds(totalAnimationTime);

        isShowing = false;

        // 자동 닫기 시작
        if (useAutoClose)
        {
            autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine());
        }
    }

    private IEnumerator AutoCloseCoroutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        CloseRewardUI();
    }

    private IEnumerator ExpandCenterPanel()
    {
        if (centerPanel == null) yield break;

        float elapsed = 0f;
        float startHeight = 0f;

        // 시작: 높이 0
        centerPanel.sizeDelta = new Vector2(centerPanel.sizeDelta.x, startHeight);

        // 높이 증가
        while (elapsed < panelExpandDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / panelExpandDuration;
            float curveValue = panelExpandCurve.Evaluate(t);

            float currentHeight = Mathf.Lerp(startHeight, targetPanelHeight, curveValue);
            centerPanel.sizeDelta = new Vector2(centerPanel.sizeDelta.x, currentHeight);

            yield return null;
        }

        // 최종 높이 설정
        centerPanel.sizeDelta = new Vector2(centerPanel.sizeDelta.x, targetPanelHeight);
    }

    private IEnumerator ShowSubText()
    {
        if (subText == null) yield break;

        subText.gameObject.SetActive(true);

        Color subtextColor = subText.color;
        subtextColor.a = 0f;
        subText.color = subtextColor;

        yield return new WaitForSeconds(0.5f);

        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            subtextColor.a = Mathf.Lerp(0f, 1f, t);
            subText.color = subtextColor;

            yield return null;
        }
    }

    private IEnumerator ShowTitleText()
    {
        if (titleText == null) yield break;

        titleText.gameObject.SetActive(true);

        titleText.transform.localScale = Vector3.zero;
        Color textColor = titleText.color;
        textColor.a = 0f;
        titleText.color = textColor;

        yield return new WaitForSeconds(0.05f);

        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveValue = panelExpandCurve.Evaluate(t);

            titleText.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, curveValue);

            textColor.a = Mathf.Lerp(0f, 1f, curveValue);
            titleText.color = textColor;

            yield return null;
        }

        titleText.transform.localScale = Vector3.one;
        textColor.a = 1f;
        titleText.color = textColor;
    }

    private void CreateAndShowItem(SkillRewardData data, float delay)
    {
        if (itemPrefab == null || itemContainer == null)
        {
            return;
        }

        SkillRewardItem item = Instantiate(itemPrefab, itemContainer);
        item.Initialize(data);
        item.PlayShowAnimation(delay);

        activeItems.Add(item);
    }

    public void CloseRewardUI()
    {
        // 자동 닫기 코루틴 취소
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        if (!gameObject.activeInHierarchy)
        {
            HideRewardPanelImmediate();
            return;
        }

        StartCoroutine(CloseRewardUICoroutine());
    }

    private IEnumerator CloseRewardUICoroutine()
    {
        isShowing = false;

        if (titleText != null)
        {
            StartCoroutine(HideTitleText());
        }

        if (subText != null)
        {
            StartCoroutine(HideSubText());
        }

        List<SkillRewardItem> itemsToRemove = new List<SkillRewardItem>(activeItems);

        foreach (var item in itemsToRemove)
        {
            if (item != null)
            {
                item.Cleanup();

                SkillRewardItem currentItem = item;
                item.PlayHideAnimation(() =>
                {
                    if (currentItem != null)
                    {
                        Destroy(currentItem.gameObject);
                    }
                });
            }
        }

        yield return new WaitForSeconds(panelExpandDuration * 0.5f);
        activeItems.Clear();

        yield return StartCoroutine(CollapseCenterPanel());

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    private IEnumerator HideTitleText()
    {
        if (titleText == null || !titleText.gameObject.activeSelf) yield break;

        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startScale = titleText.transform.localScale;
        Color textColor = titleText.color;
        float startAlpha = textColor.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 스케일 감소
            titleText.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // 페이드 아웃
            textColor.a = Mathf.Lerp(startAlpha, 0f, t);
            titleText.color = textColor;

            yield return null;
        }

        titleText.gameObject.SetActive(false);
    }

    private IEnumerator HideSubText()
    {
        if (subText == null || !subText.gameObject.activeSelf) yield break;

        float elapsed = 0f;
        float duration = 0.3f;
        Color subtextColor = subText.color;
        float startAlpha = subtextColor.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            subtextColor.a = Mathf.Lerp(startAlpha, 0f, t);
            subText.color = subtextColor;

            yield return null;
        }

        subText.gameObject.SetActive(false);
    }

    private IEnumerator CollapseCenterPanel()
    {
        if (centerPanel == null) yield break;

        float elapsed = 0f;
        float duration = panelExpandDuration * 0.5f;
        float startHeight = centerPanel.sizeDelta.y;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float currentHeight = Mathf.Lerp(startHeight, 0f, t);
            centerPanel.sizeDelta = new Vector2(centerPanel.sizeDelta.x, currentHeight);

            yield return null;
        }

        centerPanel.sizeDelta = new Vector2(centerPanel.sizeDelta.x, 0f);
    }

    private void HideRewardPanelImmediate()
    {
        // 모든 아이템 즉시 삭제
        foreach (var item in activeItems)
        {
            if (item != null)
            {
                item.Cleanup();
                Destroy(item.gameObject);
            }
        }
        activeItems.Clear();

        // 패널 비활성화
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        // 배경 초기화
        if (backgroundDimmer != null)
        {
            Color c = backgroundDimmer.color;
            c.a = 0f;
            backgroundDimmer.color = c;
        }

        // 중앙 패널 높이 0으로
        if (centerPanel != null)
        {
            centerPanel.sizeDelta = new Vector2(centerPanel.sizeDelta.x, 0f);
        }

        if (titleText != null)
        {
            titleText.gameObject.SetActive(false);
        }

        if (subText != null)
        {
            subText.gameObject.SetActive(false);
        }

        isShowing = false;
    }

    // 사운드 재생
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // 지연 사운드 재생
    private IEnumerator PlaySoundDelayed(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySound(clip);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        foreach (var item in activeItems)
        {
            if (item != null)
            {
                item.Cleanup();
            }
        }

        activeItems.Clear();
    }
}