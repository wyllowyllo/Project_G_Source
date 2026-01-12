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
