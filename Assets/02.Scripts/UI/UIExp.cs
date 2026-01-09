using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIExp : MonoBehaviour
{
    [SerializeField] private Slider _expbar;
    [SerializeField] private float _expLerpSpeed = 5f;

    [SerializeField] private Image _imageExpComplete;

    [SerializeField] private TextMeshProUGUI _expText;

    private float _displayExp;
    private float _targetExp;

    private float _maxExp = 100;
    private float _curExp = 0;

    private bool _isLevelingUp = false;

    private Tween _expTween;

    public System.Action OnLevelUp;

    private void Start()
    {
        _expbar.value = (float)_curExp / (float)_maxExp;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (_isLevelingUp)
            {
                return;
            }

            if (_targetExp > _maxExp)
            {
                return;
            }

            _targetExp += 10;

            if (_targetExp > _maxExp)
            { 
                _targetExp = _maxExp;
            }

            AnimatorExpBar();

            if (_targetExp >= _maxExp)
            {
                _isLevelingUp = true;
                StartCoroutine(ImageExpComplete_Coroutine());
                StartCoroutine(LevelUpExp_Coroutine());
            }
        }
        Handle();
    }

    private void AnimatorExpBar()
    {
        // 이전 트윈이 있다면 중단
        _expTween?.Kill();

        float duration = Mathf.Abs(_targetExp - _displayExp) / (_maxExp * _expLerpSpeed);
        _expTween = DOTween.To(() => _displayExp, x => _displayExp = x, _targetExp, duration).SetEase(Ease.OutQuad).OnUpdate(() =>
        {
            _curExp = Mathf.RoundToInt(_displayExp);
        });
    }

    private void Handle()
    {
        _expbar.value = _displayExp / _maxExp;
        if (_expText != null)
        {
            _expText.text = $"{Mathf.RoundToInt(_curExp)}/{Mathf.RoundToInt(_maxExp)}";
        }

        if(_expbar.value >= _maxExp)
        {
            _expbar.value = _maxExp;
        }
    }

    private IEnumerator LevelUpExp_Coroutine()
    {
        yield return new WaitForSeconds(2f);

        OnLevelUp?.Invoke();
        _targetExp = 0;
        _displayExp = 0;
        _curExp = 0;

        _expbar.value = 0;

        _isLevelingUp = false;
    }

    private IEnumerator ImageExpComplete_Coroutine()
    {
        Color color = new Color32(255, 251, 213, 255);
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
    }
}

