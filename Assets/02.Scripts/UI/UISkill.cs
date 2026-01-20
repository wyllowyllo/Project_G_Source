using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Skill;
using Progression;

public class UISkill : MonoBehaviour
{
    [SerializeField] private string _skillName;
    [SerializeField] private float _maxCooldownTime;
    [SerializeField] private TextMeshProUGUI _textSkillData;
    [SerializeField] private TextMeshProUGUI _textCooldownTime;
    [SerializeField] private Image _imageCooldownTime;
    [SerializeField] private Image _imageCooldownComplete;

    private Transform _skillButtonScale;
    private Vector3 _skillDefaultScale;
    private float _skillSizeRate = 0.8f;
    [SerializeField] private float _sizeChangeDuration = 0.1f;

    private float _currentCooldownTime;
    private bool _isCooldown;

    private SkillCaster _caster;
    private SkillSlot _boundSlot;

    private void Awake()
    {
        SetCooldownIs(false);
        SetUseSkillText();

        _skillButtonScale = transform;
        _skillDefaultScale = _skillButtonScale.localScale;
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

        StartCoroutine(nameof(AnimateButtonSize));

        while (_currentCooldownTime > 0)
        {
            _currentCooldownTime -= Time.deltaTime;
            _imageCooldownTime.fillAmount = _currentCooldownTime / _maxCooldownTime;
            _textCooldownTime.text = _currentCooldownTime.ToString("F1");

            yield return null;
        }

        SetCooldownIs(false);
        StartCoroutine(nameof(ImageCooldownComplete));
    }

    private IEnumerator AnimateButtonSize()
    {
        float elapsed = 0f;
        Vector3 targetScale = _skillDefaultScale * _skillSizeRate;

        while (elapsed < _sizeChangeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / _sizeChangeDuration;
            _skillButtonScale.localScale = Vector3.Lerp(_skillDefaultScale, targetScale, t);
            yield return null;
        }

        _skillButtonScale.localScale = targetScale;

        elapsed = 0f;

        // 다시 커지는 애니메이션
        while (elapsed < _sizeChangeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / _sizeChangeDuration;
            _skillButtonScale.localScale = Vector3.Lerp(targetScale, _skillDefaultScale, t);
            yield return null;
        }

        _skillButtonScale.localScale = _skillDefaultScale;
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

    public void BindToSkillCaster(SkillCaster caster, SkillSlot slot)
    {
        if (_caster != null)
            _caster.OnSkillUsed -= HandleSkillUsed;

        _caster = caster;
        _boundSlot = slot;

        if (_caster != null)
            _caster.OnSkillUsed += HandleSkillUsed;
    }

    private void OnDestroy()
    {
        if (_caster != null)
            _caster.OnSkillUsed -= HandleSkillUsed;
    }

    private void HandleSkillUsed(SkillSlot slot, float cooldown)
    {
        if (slot == _boundSlot)
            StartCooldown(cooldown);
    }

    public void StartCooldown(float cooldownTime)
    {
        _maxCooldownTime = cooldownTime;
        StartCoroutine(nameof(OnCooldownTime), cooldownTime);
    }

}
