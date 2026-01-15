using UnityEngine;
using Progression;
using TMPro;

public class LevelUpUIManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerProgression _playerProgression;
    [SerializeField] private LevelUpGradientAnimation _levelUpAnimation; 
    [SerializeField] private LevelUpTextAnimation _textAnimation;       

    [SerializeField] private ParticleSystem _levelUpEffect;

    [Header("레벨 텍스트 설정")]
    [SerializeField] private string _levelTextFormat = "LEVEL {0}";
    [SerializeField] private string _messageText = "레벨이 올랐습니다!";

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

        if (_levelUpEffect != null)
        {
            _levelUpEffect.Stop();           // 먼저 멈추기
            _levelUpEffect.Clear();          // 파티클 클리어
            _levelUpEffect.Play();           // 다시 재생
        }
    }
}