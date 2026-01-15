using UnityEngine;
using TMPro;
using System.Collections;

public class LevelUpTextAnimation : MonoBehaviour
{
    [Header("텍스트 참조")]
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _messageText;
    
    [Header("타이밍 설정")]
    [SerializeField] private float _textAppearDelay = 0.3f;
    [SerializeField] private float _textFadeInDuration = 0.5f;
    [SerializeField] private float _textDisplayDuration = 2.0f;
    [SerializeField] private float _textFadeOutDuration = 0.3f;
    
    [Header("추가 효과")]
    [SerializeField] private bool _useScaleEffect = true;
    [SerializeField] private float _startScale = 0.8f;
    [SerializeField] private float _endScale = 1.0f;
    
    private Color _levelTextOriginalColor;
    private Color _messageTextOriginalColor;
    private bool _isPlaying = false;

    private Coroutine _textAnimationCoroutine;

    private void Awake()
    {
        if (_levelText != null)
        {
            _levelTextOriginalColor = _levelText.color;
        }
        
        if (_messageText != null)
        {
            _messageTextOriginalColor = _messageText.color;
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
        if (_levelText != null)
        {
            Color color = _levelTextOriginalColor;
            color.a = alpha;
            _levelText.color = color;
        }
        
        if (_messageText != null)
        {
            Color color = _messageTextOriginalColor;
            color.a = alpha;
            _messageText.color = color;
        }
    }

    private void SetTextScale(float scale)
    {
        if (_levelText != null)
        {
            _levelText.transform.localScale = Vector3.one * scale;
        }
        
        if (_messageText != null)
        {
            _messageText.transform.localScale = Vector3.one * scale;
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
