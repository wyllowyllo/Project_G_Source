using UnityEngine;
using Progression;
using TMPro;

public class LevelUpUIManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerProgression _playerProgression;
    [SerializeField] private LevelUpGradientAnimation _levelUpAnimation;      // 이미지 애니메이션
    [SerializeField] private LevelUpTextAnimation _textAnimation;             // 텍스트 애니메이션

    [Header("레벨 텍스트 설정")]
    [SerializeField] private string _levelTextFormat = "LEVEL {0}";
    [SerializeField] private string _messageText = "레벨이 올랐습니다!";

    [Header("Debug")]
    [SerializeField] private bool _enableDebugKeys = true;
    [SerializeField] private KeyCode _testLevelUpKey = KeyCode.L;

    private void OnEnable()
    {
        if (_playerProgression != null)
        {
            _playerProgression.OnLevelUp += HandleLevelUp;
        }
    }

    private void OnDisable()
    {
        if (_playerProgression != null)
        {
            _playerProgression.OnLevelUp -= HandleLevelUp;
        }
    }

    private void Start()
    {
        if (_playerProgression == null)
        {
            _playerProgression = FindObjectOfType<PlayerProgression>();
        }

        // LevelUpGradientAnimation 자동 찾기
        if (_levelUpAnimation == null)
        {
            _levelUpAnimation = FindObjectOfType<LevelUpGradientAnimation>();
        }

        // LevelUpTextAnimation 자동 찾기
        if (_textAnimation == null)
        {
            _textAnimation = FindObjectOfType<LevelUpTextAnimation>();
        }
    }

    private void HandleLevelUp(int previousLevel, int newLevel)
    {
        if (_levelUpAnimation != null)
        {
            _levelUpAnimation.PlayLevelUpAnimation();
        }

        // 텍스트 애니메이션 재생
        if (_textAnimation != null)
        {
            string levelText = string.Format(_levelTextFormat, newLevel);
            _textAnimation.PlayTextAnimation(levelText, _messageText);
        }
    }
}