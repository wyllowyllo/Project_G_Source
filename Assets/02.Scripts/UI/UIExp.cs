using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Progression;

public class UIExp : MonoBehaviour
{
    [SerializeField] private PlayerProgression _playerProgression;
    [SerializeField] private Slider _expbar;

    [SerializeField] private Image _imageExpComplete;

    [SerializeField] private float _expAnimationDuration = 0.35f;
    [SerializeField] private Ease _expAnimationEase = Ease.OutCubic;

    [SerializeField] private TextMeshProUGUI _expText;

    private float _levelUpSequenceDelay = 2f;

    [Header("Exp Complete Color")]
    [SerializeField] private Color32 _expCompleteColor = new Color32(255, 251, 213, 255);

    private float _displayExp;
    private int _previousXp;
    private int _previousMaxXp;
    private int _displayMaxXp;

    private bool _isLevelingUp = false;

    private Tween _expTween;

    // y키로 경험치 테스트용
    private bool _enableDebugInput = true;
    private int _debugExpAmount = 200;

    private void Start()
    {
        if(_playerProgression == null)
        {
            _playerProgression = FindObjectOfType<PlayerProgression>();
            if(_playerProgression == null)
            {
                Debug.Log("PlayerProgression not found!");
                enabled = false;
                return;
            }
        }

        _playerProgression.OnLevelUp += OnPlayerLevelUp;

        _previousXp = _playerProgression.CurrentXp;
        _previousMaxXp = _playerProgression.XpToNextLevel;
        _displayExp = _previousXp;

        _displayMaxXp = _previousMaxXp;

        UpdateUI();
    }

    private void Update()
    {
        if (_enableDebugInput && Input.GetKeyDown(KeyCode.Y))
        {
            _playerProgression.AddExperience(_debugExpAmount);
        }

        CheckExpChange();

        UpdateUI();
    }

    private void CheckExpChange()
    {
        if (_isLevelingUp)
        {
            return;
        }

        int currentXp = _playerProgression.CurrentXp;
        int currentMaxXp = _playerProgression.XpToNextLevel;

        if (currentXp != _previousXp || currentMaxXp != _previousMaxXp)
        {
            _displayMaxXp = currentMaxXp;

            AnimateExpBar(currentXp);

            _previousXp = currentXp;
            _previousMaxXp = currentMaxXp;
        }
    }
    private void AnimateExpBar(int targetXp)
    {
        _expTween?.Kill();

        _expTween = DOTween.To(() => _displayExp, x => _displayExp = x, targetXp, _expAnimationDuration)
            .SetEase(_expAnimationEase);
    }

    private void UpdateUI()
    {
        int maxXp = _displayMaxXp;

        if (maxXp > 0)
        {
            _expbar.value = _displayExp / maxXp;
        }
        else
        {
            // 최대 레벨일 경우
            _expbar.value = 1f;
        }

        if (_expText != null)
        {
            if (_playerProgression.IsMaxLevel)
            {
                _expText.text = "MAX";
            }
            else
            {
                _expText.text = $"{Mathf.FloorToInt(_displayExp)}/{maxXp}";
            }
        }
    }

    private void OnPlayerLevelUp(int previousLevel, int newLevel)
    {
        if (_isLevelingUp) return;

        _isLevelingUp = true;
        StartCoroutine(LevelUpSequence());
    }

    private IEnumerator LevelUpSequence()
    {
        _displayMaxXp = _previousMaxXp;
        int maxXp = _previousMaxXp;
        if (maxXp > 0)
        {
            _expTween?.Kill();
            yield return DOTween.To(() => _displayExp, x => _displayExp = x, maxXp, _expAnimationDuration)
                .SetEase(_expAnimationEase)
                .WaitForCompletion();
        }

         StartCoroutine(nameof(ImageExpComplete_Coroutine));

        yield return new WaitForSeconds(_levelUpSequenceDelay);

        _previousXp = _playerProgression.CurrentXp;
        _previousMaxXp = _playerProgression.XpToNextLevel;

        _displayExp = 0;
        _displayMaxXp = _previousMaxXp;

        if (_previousXp > 0)
        {
            AnimateExpBar(_previousXp);
        }

        _isLevelingUp = false;
    }

    private IEnumerator ImageExpComplete_Coroutine()
    {
        Color color = _expCompleteColor;
        color.a = 1f;

        _imageExpComplete.color = color;
        _imageExpComplete.gameObject.SetActive(true);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            _imageExpComplete.color = color;
            yield return null;
        }

        color.a = 0f;
        _imageExpComplete.color = color;
        _imageExpComplete.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _expTween?.Kill();

        if (_playerProgression != null)
        {
            _playerProgression.OnLevelUp -= OnPlayerLevelUp;
        }
    }
}

