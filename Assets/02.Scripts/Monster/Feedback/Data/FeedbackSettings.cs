using UnityEngine;

namespace Monster.Feedback.Data
{
    // 피드백 강도별 설정을 관리하는 ScriptableObject
    // 몬스터 타입별로 다른 설정 적용 가능 (일반, 엘리트, 보스 등)
    [CreateAssetMenu(fileName = "FeedbackSettings", menuName = "Monster/Feedback Settings")]
    public class FeedbackSettings : ScriptableObject
    {
        [Header("일반 타격")]
        [SerializeField] private HitstopConfig _normalHitstop = HitstopConfig.Default;
        [SerializeField] private CameraShakeConfig _normalShake = CameraShakeConfig.Default;
        [SerializeField] private HitFlashConfig _normalFlash = HitFlashConfig.Default;
        [SerializeField] private ScreenEffectConfig _normalScreen = ScreenEffectConfig.Default;

        [Header("크리티컬 타격")]
        [SerializeField] private HitstopConfig _criticalHitstop = new HitstopConfig
        {
            Enabled = true,
            Duration = 0.06f,
            TimeScale = 0f
        };
        [SerializeField] private CameraShakeConfig _criticalShake = new CameraShakeConfig
        {
            Enabled = true,
            Force = 1f,
            Duration = 0.15f,
            Direction = new Vector3(0f, -1f, 0f)
        };
        [SerializeField] private HitFlashConfig _criticalFlash = new HitFlashConfig
        {
            Enabled = true,
            FlashColor = new Color(1f, 0.3f, 0.3f),
            FlashIntensity = 1.5f,
            FlashDuration = 0.15f,
            FlashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
        };
        [SerializeField] private ScreenEffectConfig _criticalScreen = new ScreenEffectConfig
        {
            Enabled = true,
            VignetteIntensity = 0.3f,
            ColorTint = Color.white,
            Duration = 0.15f
        };

        [Header("사망")]
        [SerializeField] private HitstopConfig _deathHitstop = new HitstopConfig
        {
            Enabled = true,
            Duration = 0.1f,
            TimeScale = 0f
        };
        [SerializeField] private CameraShakeConfig _deathShake = new CameraShakeConfig
        {
            Enabled = true,
            Force = 2f,
            Duration = 0.2f,
            Direction = new Vector3(0f, -1f, 0f)
        };
        [SerializeField] private HitFlashConfig _deathFlash = new HitFlashConfig
        {
            Enabled = true,
            FlashColor = new Color(1f, 0.2f, 0.2f),
            FlashIntensity = 2f,
            FlashDuration = 0.2f,
            FlashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
        };
        [SerializeField] private ScreenEffectConfig _deathScreen = new ScreenEffectConfig
        {
            Enabled = true,
            VignetteIntensity = 0.5f,
            ColorTint = new Color(1f, 0.9f, 0.9f),
            Duration = 0.25f
        };

        public HitstopConfig GetHitstopConfig(FeedbackIntensity intensity)
        {
            return intensity switch
            {
                FeedbackIntensity.Critical or FeedbackIntensity.Heavy => _criticalHitstop,
                FeedbackIntensity.Death => _deathHitstop,
                _ => _normalHitstop
            };
        }

        public CameraShakeConfig GetCameraShakeConfig(FeedbackIntensity intensity)
        {
            return intensity switch
            {
                FeedbackIntensity.Critical or FeedbackIntensity.Heavy => _criticalShake,
                FeedbackIntensity.Death => _deathShake,
                _ => _normalShake
            };
        }

        public HitFlashConfig GetHitFlashConfig(FeedbackIntensity intensity)
        {
            return intensity switch
            {
                FeedbackIntensity.Critical or FeedbackIntensity.Heavy => _criticalFlash,
                FeedbackIntensity.Death => _deathFlash,
                _ => _normalFlash
            };
        }

        public ScreenEffectConfig GetScreenEffectConfig(FeedbackIntensity intensity)
        {
            return intensity switch
            {
                FeedbackIntensity.Critical or FeedbackIntensity.Heavy => _criticalScreen,
                FeedbackIntensity.Death => _deathScreen,
                _ => _normalScreen
            };
        }
    }
}
