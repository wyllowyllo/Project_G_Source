using System;
using UnityEngine;

namespace Monster.Feedback.Data
{
    // 히트스탑 설정
    [Serializable]
    public struct HitstopConfig
    {
        [Tooltip("히트스탑 활성화 여부")]
        public bool Enabled;

        [Tooltip("히트스탑 지속 시간 (초)")]
        [Range(0f, 0.5f)]
        public float Duration;

        [Tooltip("히트스탑 중 타임스케일 (0 = 완전 정지)")]
        [Range(0f, 0.1f)]
        public float TimeScale;

        public static HitstopConfig Default => new HitstopConfig
        {
            Enabled = true,
            Duration = 0.04f,
            TimeScale = 0f
        };
    }

    // 카메라 쉐이크 설정
    [Serializable]
    public struct CameraShakeConfig
    {
        [Tooltip("카메라 쉐이크 활성화 여부")]
        public bool Enabled;

        [Tooltip("쉐이크 강도")]
        [Range(0f, 5f)]
        public float Force;

        [Tooltip("쉐이크 지속 시간 (초)")]
        [Range(0f, 1f)]
        public float Duration;

        [Tooltip("쉐이크 방향 (정규화됨)")]
        public Vector3 Direction;

        public static CameraShakeConfig Default => new CameraShakeConfig
        {
            Enabled = true,
            Force = 0.5f,
            Duration = 0.1f,
            Direction = new Vector3(0f, -1f, 0f)
        };
    }

    // 히트 플래시 설정
    [Serializable]
    public struct HitFlashConfig
    {
        [Tooltip("히트 플래시 활성화 여부")]
        public bool Enabled;

        [Tooltip("플래시 색상")]
        public Color FlashColor;

        [Tooltip("플래시 강도")]
        [Range(0f, 2f)]
        public float FlashIntensity;

        [Tooltip("플래시 지속 시간 (초)")]
        [Range(0f, 0.5f)]
        public float FlashDuration;

        [Tooltip("플래시 페이드 커브")]
        public AnimationCurve FlashCurve;

        public static HitFlashConfig Default => new HitFlashConfig
        {
            Enabled = true,
            FlashColor = Color.white,
            FlashIntensity = 1f,
            FlashDuration = 0.1f,
            FlashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
        };
    }

    // 환경 카메라 쉐이크 설정 (활공, 달리기 등 지속적인 흔들림)
    [Serializable]
    public struct AmbientShakeConfig
    {
        [Tooltip("환경 쉐이크 활성화 여부")]
        public bool Enabled;

        [Tooltip("쉐이크 강도")]
        [Range(0f, 1f)]
        public float Intensity;

        [Tooltip("쉐이크 주파수 (낮을수록 느리고 부드러움)")]
        [Range(0.1f, 5f)]
        public float Frequency;

        [Tooltip("페이드 인 시간 (초)")]
        [Range(0f, 2f)]
        public float FadeInDuration;

        [Tooltip("페이드 아웃 시간 (초)")]
        [Range(0f, 2f)]
        public float FadeOutDuration;

        [Tooltip("슬로모션 영향 무시 여부")]
        public bool UnscaledTime;

        public static AmbientShakeConfig Default => new AmbientShakeConfig
        {
            Enabled = true,
            Intensity = 0.15f,
            Frequency = 1.5f,
            FadeInDuration = 0.5f,
            FadeOutDuration = 0.3f,
            UnscaledTime = true
        };

        public static AmbientShakeConfig Glide => new AmbientShakeConfig
        {
            Enabled = true,
            Intensity = 0.12f,
            Frequency = 1.2f,
            FadeInDuration = 0.8f,
            FadeOutDuration = 0.4f,
            UnscaledTime = true
        };
    }

    // 화면 효과 설정
    [Serializable]
    public struct ScreenEffectConfig
    {
        [Tooltip("화면 효과 활성화 여부")]
        public bool Enabled;

        [Tooltip("비네트 강도")]
        [Range(0f, 1f)]
        public float VignetteIntensity;

        [Tooltip("화면 색상 틴트")]
        public Color ColorTint;

        [Tooltip("효과 지속 시간 (초)")]
        [Range(0f, 1f)]
        public float Duration;

        public static ScreenEffectConfig Default => new ScreenEffectConfig
        {
            Enabled = false,
            VignetteIntensity = 0.3f,
            ColorTint = Color.white,
            Duration = 0.2f
        };
    }
}
