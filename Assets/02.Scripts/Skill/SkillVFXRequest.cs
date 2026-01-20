using UnityEngine;

namespace Skill
{
    public readonly struct SkillVFXRequest
    {
        public SkillAreaType AreaType { get; }
        public Vector3 Origin { get; }
        public Quaternion Rotation { get; }
        public float Range { get; }
        public float Angle { get; }
        public float ConeHeight { get; }
        public float BoxWidth { get; }
        public float BoxHeight { get; }
        public int Rank { get; }
        public GameObject[] EffectPrefabs { get; }
        public Vector3 VFXPositionOffset { get; }
        public Vector3 VFXRotationOffset { get; }

        public SkillVFXRequest(
            SkillAreaType areaType,
            Vector3 origin,
            Quaternion rotation,
            float range,
            int rank,
            GameObject[] effectPrefabs,
            Vector3 vfxPositionOffset,
            Vector3 vfxRotationOffset,
            float angle = 0f,
            float coneHeight = 0f,
            float boxWidth = 0f,
            float boxHeight = 0f)
        {
            AreaType = areaType;
            Origin = origin;
            Rotation = rotation;
            Range = range;
            Rank = rank;
            EffectPrefabs = effectPrefabs;
            VFXPositionOffset = vfxPositionOffset;
            VFXRotationOffset = vfxRotationOffset;
            Angle = angle;
            ConeHeight = coneHeight;
            BoxWidth = boxWidth;
            BoxHeight = boxHeight;
        }

        public static SkillVFXRequest Create(
            PlayerSkillData skillData,
            SkillTierData tierData,
            Vector3 origin,
            Quaternion rotation,
            int rank)
        {
            return new SkillVFXRequest(
                skillData.AreaType,
                origin,
                rotation,
                tierData.Range,
                rank,
                tierData.EffectPrefabs,
                tierData.VFXPositionOffset,
                tierData.VFXRotationOffset,
                tierData.Angle,
                tierData.ConeHeight,
                tierData.BoxWidth,
                tierData.BoxHeight);
        }
    }
}
