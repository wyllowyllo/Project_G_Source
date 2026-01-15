using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelUpTextAnimation : MonoBehaviour
{
    [Header("텍스트 참조")]
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _messageText;
    
    [Header("타이밍 설정")]
    [SerializeField] private float _textAppearDelay = 0.35f;
    [SerializeField] private float _textFadeInDuration = 0.6f;
    [SerializeField] private float _textDisplayDuration = 2.3f;
    [SerializeField] private float _textFadeOutDuration = 0.3f;
    
    [Header("추가 효과")]
    [SerializeField] private bool _useScaleEffect = true;
    [SerializeField] private float _startScale = 0.8f;
    [SerializeField] private float _endScale = 1.0f;

    private Dictionary<TextMeshProUGUI, Color> _originalColors = new Dictionary<TextMeshProUGUI, Color>();
    private bool _isPlaying = false;

    private Coroutine _textAnimationCoroutine;

    private void Awake()
    {
        if (_levelText != null)
        {
            _originalColors[_levelText] = _levelText.color;
        }

        if (_messageText != null)
        {
            _originalColors[_messageText] = _messageText.color;

        }
    }

    public void PlayTextAnimation(string levelText = null, string messageText = null)
    {
        if (_isPlaying && _textAnimationCoroutine != null)
        {
            StopCoroutine(_textAnimationCoroutine);
            _textAnimationCoroutine = null;
        }

        _textAnimationCoroutine = StartCoroutine(TextAnimationCoroutine(levelText, messageText));
    }

    private IEnumerator TextAnimationCoroutine(string levelText, string messageText)
    {
        _isPlaying = true;

        // 텍스트 설정
        if (!string.IsNullOrEmpty(levelText) && _levelText != null)
        {
            _levelText.text = levelText;
        }
        
        if (!string.IsNullOrEmpty(messageText) && _messageText != null)
        {
            _messageText.text = messageText;
        }

        // 초기 상태: 투명
        SetTextAlpha(0f);
        if (_useScaleEffect)
        {
            SetTextScale(_startScale);
        }

        // 딜레이 (이미지가 먼저 나타나도록)
        yield return new WaitForSeconds(_textAppearDelay);

        // 페이드 인
        float elapsed = 0f;
        while (elapsed < _textFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _textFadeInDuration;
            
            SetTextAlpha(t);
            
            if (_useScaleEffect)
            {
                float scale = Mathf.Lerp(_startScale, _endScale, t);
                SetTextScale(scale);
            }
            
            yield return null;
        }

        // 최종 상태
        SetTextAlpha(1f);
        if (_useScaleEffect)
        {
            SetTextScale(_endScale);
        }

        // 화면에 표시
        yield return new WaitForSeconds(_textDisplayDuration);

        // 페이드 아웃 (빠르게 사라짐!)
        elapsed = 0f;
        while (elapsed < _textFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _textFadeOutDuration;
            
            SetTextAlpha(1f - t);
            
            yield return null;
        }

        SetTextAlpha(0f);
        _isPlaying = false;
        _textAnimationCoroutine = null;
    }

    private void SetTextAlpha(float alpha)
    {
        foreach (var kvp in _originalColors)
        {
            var text = kvp.Key;
            var originalColor = kvp.Value;

            if (text != null)
            {
                Color color = originalColor;
                color.a = alpha;
                text.color = color;
            }
        }
    }

    private void SetTextScale(float scale)
    {
        var texts = new[] { _levelText, _messageText };

        foreach (var text in texts)
        {
            if (text != null)
            {
                text.transform.localScale = Vector3.one * scale;
            }
        }
    }

    public bool IsPlaying => _isPlaying;

    public void HideTextImmediately()
    {
        if (_textAnimationCoroutine != null)
        {
            StopCoroutine(_textAnimationCoroutine);
            _textAnimationCoroutine = null;
        }

        SetTextAlpha(0f);
        _isPlaying = false;
    }
}
