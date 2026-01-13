using Combat.Core;
using UnityEngine;

namespace Skill
{
    public readonly struct SkillAreaContext
    {
        public SkillAreaType AreaType { get; }
        public float Range { get; }
        public float Angle { get; }
        public float BoxWidth { get; }
        public float BoxHeight { get; }
        public Vector3 PositionOffset { get; }
        public LayerMask EnemyLayer { get; }
        public CombatTeam AttackerTeam { get; }

        public SkillAreaContext(
            SkillAreaType areaType,
            float range,
            float angle,
            float boxWidth,
            float boxHeight,
            Vector3 positionOffset,
            LayerMask enemyLayer,
            CombatTeam attackerTeam)
        {
            AreaType = areaType;
            Range = range;
            Angle = angle;
            BoxWidth = boxWidth;
            BoxHeight = boxHeight;
            PositionOffset = positionOffset;
            EnemyLayer = enemyLayer;
            AttackerTeam = attackerTeam;
        }

        public static SkillAreaContext Create(
            PlayerSkillData skillData,
            SkillTierData tierData,
            LayerMask enemyLayer,
            CombatTeam attackerTeam)
        {
            return new SkillAreaContext(
                skillData.AreaType,
                tierData.Range,
                tierData.Angle,
                tierData.BoxWidth,
                tierData.BoxHeight,
                tierData.PositionOffset,
                enemyLayer,
                attackerTeam
            );
        }
    }
}
