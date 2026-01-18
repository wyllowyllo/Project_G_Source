using Combat.Core;
using UnityEngine;

namespace Skill
{
    public readonly struct SkillAreaContext
    {
        public SkillAreaType AreaType { get; }
        public float Range { get; }
        public float Angle { get; }
        public float ConeHeight { get; }
        public float BoxWidth { get; }
        public float BoxHeight { get; }
        public Vector3 PositionOffset { get; }
        public LayerMask EnemyLayer { get; }
        public CombatTeam AttackerTeam { get; }

        private SkillAreaContext(
            SkillAreaType areaType,
            float range,
            Vector3 positionOffset,
            LayerMask enemyLayer,
            CombatTeam attackerTeam,
            float angle = 0f,
            float coneHeight = 0f,
            float boxWidth = 0f,
            float boxHeight = 0f)
        {
            AreaType = areaType;
            Range = range;
            Angle = angle;
            ConeHeight = coneHeight;
            BoxWidth = boxWidth;
            BoxHeight = boxHeight;
            PositionOffset = positionOffset;
            EnemyLayer = enemyLayer;
            AttackerTeam = attackerTeam;
        }

        public static SkillAreaContext CreateSphere(
            float range,
            Vector3 positionOffset,
            LayerMask enemyLayer,
            CombatTeam attackerTeam)
        {
            return new SkillAreaContext(
                SkillAreaType.Sphere,
                range,
                positionOffset,
                enemyLayer,
                attackerTeam);
        }

        public static SkillAreaContext CreateBox(
            float range,
            float boxWidth,
            float boxHeight,
            Vector3 positionOffset,
            LayerMask enemyLayer,
            CombatTeam attackerTeam)
        {
            return new SkillAreaContext(
                SkillAreaType.Box,
                range,
                positionOffset,
                enemyLayer,
                attackerTeam,
                boxWidth: boxWidth,
                boxHeight: boxHeight);
        }

        public static SkillAreaContext CreateCone(
            float range,
            float angle,
            float coneHeight,
            Vector3 positionOffset,
            LayerMask enemyLayer,
            CombatTeam attackerTeam)
        {
            return new SkillAreaContext(
                SkillAreaType.Cone,
                range,
                positionOffset,
                enemyLayer,
                attackerTeam,
                angle: angle,
                coneHeight: coneHeight);
        }

        public static SkillAreaContext Create(
            PlayerSkillData skillData,
            SkillTierData tierData,
            LayerMask enemyLayer,
            CombatTeam attackerTeam)
        {
            return skillData.AreaType switch
            {
                SkillAreaType.Sphere => CreateSphere(
                    tierData.Range,
                    tierData.PositionOffset,
                    enemyLayer,
                    attackerTeam),

                SkillAreaType.Box => CreateBox(
                    tierData.Range,
                    tierData.BoxWidth,
                    tierData.BoxHeight,
                    tierData.PositionOffset,
                    enemyLayer,
                    attackerTeam),

                SkillAreaType.Cone => CreateCone(
                    tierData.Range,
                    tierData.Angle,
                    tierData.ConeHeight,
                    tierData.PositionOffset,
                    enemyLayer,
                    attackerTeam),

                _ => CreateSphere(
                    tierData.Range,
                    tierData.PositionOffset,
                    enemyLayer,
                    attackerTeam)
            };
        }
    }
}
