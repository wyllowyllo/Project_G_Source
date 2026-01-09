using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class UISkill : MonoBehaviour
{
    [SerializeField] private string _skillName;
    [SerializeField] private float _maxCooldownTime;
    [SerializeField] private TextMeshProUGUI _textSkillData;
    [SerializeField] private TextMeshProUGUI _textCooldownTime;
    [SerializeField] private Image _imageCooldownTime;
    [SerializeField] private Image _imageCooldownComplete;

    private float _currentCooldownTime;
    private bool _isCooldown;

    private void Awake()
    {
        SetCooldownIs(false);
        SetUseSkillText();
    }

    public void UseSkill()
    {
        // 이미 스킬을 사용해서 재사용 대기 시간이 남아있으면 종료
        if (_isCooldown)
        {
            StopCoroutine(nameof(IsUseSkillText));
            StartCoroutine(IsUseSkillText());
            return;
        }

        // 재사용 대기 처리하는 코루틴
        StartCoroutine(nameof(OnCooldownTime), _maxCooldownTime);
    }

    private IEnumerator OnCooldownTime(float cooldownTime)
    {
        
        _currentCooldownTime = _maxCooldownTime;
        SetCooldownIs(true);

        while(_currentCooldownTime > 0)
        {
            _currentCooldownTime -= Time.deltaTime;
            _imageCooldownTime.fillAmount = _currentCooldownTime / _maxCooldownTime;
            _textCooldownTime.text = _currentCooldownTime.ToString("F1");

            yield return null;
        }

        SetCooldownIs(false);
        StartCoroutine(ImageCooldownComplete());
    }

    private void SetUseSkillText()
    {
        Color color = _textSkillData.color;
        color.a = 0f;
        _textSkillData.color = color;
    }

    private IEnumerator IsUseSkillText()
    {
        _textSkillData.text = "스킬 재사용 대기 중 입니다.";

        Color color = _textSkillData.color;
        color.a = 1f;
        _textSkillData.color = color;

        yield return new WaitForSeconds(0.5f);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            color.a = alpha;
            _textSkillData.color = color;

            yield return null;
        }

        color.a = 0f;
        _textSkillData.color = color;
    }

    private IEnumerator ImageCooldownComplete()
    {
        Color color = Color.white;
        color.a = 1f;

        _imageCooldownComplete.color = color;
        _imageCooldownComplete.gameObject.SetActive(true);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            _imageCooldownComplete.color = color;
            yield return null;
        }

        color.a = 0f;
        _imageCooldownComplete.color = color;
        _imageCooldownComplete.gameObject.SetActive(false);
    }

    private void SetCooldownIs(bool boolean)
    {
        _isCooldown = boolean;
        _textCooldownTime.enabled = boolean;
        _imageCooldownTime.enabled = boolean;
    }
}
