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

        public SkillVFXRequest(
            SkillAreaType areaType,
            Vector3 origin,
            Quaternion rotation,
            float range,
            float angle = 0f,
            float coneHeight = 0f,
            float boxWidth = 0f,
            float boxHeight = 0f,
            int rank = 1)
        {
            AreaType = areaType;
            Origin = origin;
            Rotation = rotation;
            Range = range;
            Angle = angle;
            ConeHeight = coneHeight;
            BoxWidth = boxWidth;
            BoxHeight = boxHeight;
            Rank = rank;
        }

        public static SkillVFXRequest FromContext(SkillAreaContext context, Vector3 origin, Quaternion rotation, int rank = 1)
        {
            return new SkillVFXRequest(
                context.AreaType,
                origin,
                rotation,
                context.Range,
                context.Angle,
                context.ConeHeight,
                context.BoxWidth,
                context.BoxHeight,
                rank);
        }
    }
}
