using System;
using UnityEngine;

namespace Equipment
{
    [CreateAssetMenu(fileName = "EquipmentGradeSettings", menuName = "Equipment/Grade Settings")]
    public class EquipmentGradeSettings : ScriptableObject
    {
        private const string RESOURCE_PATH = "EquipmentGradeSettings";

        private static EquipmentGradeSettings _instance;
        public static EquipmentGradeSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<EquipmentGradeSettings>(RESOURCE_PATH);
                }
                return _instance;
            }
        }

        [Serializable]
        public class GradeColorSet
        {
            public Color OutlineColor = Color.white;
            public Color TextColor = Color.white;
            public Color BackgroundColor = Color.gray;
        }

        [Header("Grade Colors")]
        [SerializeField] private GradeColorSet _normal = new()
        {
            OutlineColor = new Color(0.7f, 0.7f, 0.7f, 1f),
            TextColor = new Color(0.7f, 0.7f, 0.7f, 1f),
            BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.9f)
        };

        [SerializeField] private GradeColorSet _rare = new()
        {
            OutlineColor = new Color(0.3f, 0.6f, 1f, 1f),
            TextColor = new Color(0.3f, 0.6f, 1f, 1f),
            BackgroundColor = new Color(0.1f, 0.2f, 0.3f, 0.9f)
        };

        [SerializeField] private GradeColorSet _unique = new()
        {
            OutlineColor = new Color(0.6f, 0.3f, 1f, 1f),
            TextColor = new Color(0.6f, 0.3f, 1f, 1f),
            BackgroundColor = new Color(0.2f, 0.1f, 0.3f, 0.9f)
        };

        [SerializeField] private GradeColorSet _legendary = new()
        {
            OutlineColor = new Color(1f, 0.5f, 0.1f, 1f),
            TextColor = new Color(1f, 0.5f, 0.1f, 1f),
            BackgroundColor = new Color(0.3f, 0.2f, 0.1f, 0.9f)
        };

        public GradeColorSet GetColors(EquipmentGrade grade)
        {
            return grade switch
            {
                EquipmentGrade.Normal => _normal,
                EquipmentGrade.Rare => _rare,
                EquipmentGrade.Unique => _unique,
                EquipmentGrade.Legendary => _legendary,
                _ => _normal
            };
        }

        public Color GetOutlineColor(EquipmentGrade grade) => GetColors(grade).OutlineColor;
        public Color GetTextColor(EquipmentGrade grade) => GetColors(grade).TextColor;
        public Color GetBackgroundColor(EquipmentGrade grade) => GetColors(grade).BackgroundColor;
    }
}
