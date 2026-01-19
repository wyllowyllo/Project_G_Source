using UnityEngine;
using Progression;

namespace Skill
{
    public enum SkillAreaType
    {
        Box,
        Sphere,
        Cone
    }

    [System.Serializable]
    public class SkillTierData
    {
        [Header("Combat")]
        [SerializeField] private float _damageMultiplier = 1.5f;
        [SerializeField] private float _range = 5f;
        [SerializeField] private float _cooldown = 8f;

        [Header("Area")]
        [Tooltip("범위 시작 위치 오프셋 (로컬 좌표)")]
        [SerializeField] private Vector3 _positionOffset;
        [Tooltip("Cone 타입 전용: 부채꼴 각도")]
        [SerializeField] private float _angle = 180f;
        [Tooltip("Cone 타입 전용: 수직 높이")]
        [SerializeField] private float _coneHeight = 2f;
        [Tooltip("Box 타입 전용: 좌우 너비")]
        [SerializeField] private float _boxWidth = 2f;
        [Tooltip("Box 타입 전용: 상하 높이")]
        [SerializeField] private float _boxHeight = 2f;

        [Header("Movement")]
        [SerializeField] private bool _allowMovement;

        [Header("Presentation")]
        [SerializeField] private GameObject _effectPrefab;
        [SerializeField] private AudioClip _skillSound;

        [Header("Camera")]
        [Tooltip("스킬 카메라 연출 설정 (없으면 연출 없음)")]
        [SerializeField] private SkillCameraConfig _cameraConfig;

        [Tooltip("애니메이션 길이 (초). 카메라 연출 타이밍 계산에 사용")]
        [SerializeField] private float _animationDuration = 1f;

        public float DamageMultiplier => _damageMultiplier;
        public float Range => _range;
        public float Cooldown => _cooldown;
        public Vector3 PositionOffset => _positionOffset;
        public float Angle => _angle;
        public float ConeHeight => _coneHeight;
        public float BoxWidth => _boxWidth;
        public float BoxHeight => _boxHeight;
        public bool AllowMovement => _allowMovement;
        public GameObject EffectPrefab => _effectPrefab;
        public AudioClip SkillSound => _skillSound;
        public SkillCameraConfig CameraConfig => _cameraConfig;
        public float AnimationDuration => _animationDuration;
    }

    [CreateAssetMenu(fileName = "PlayerSkill", menuName = "Combat/Player Skill")]
    public class PlayerSkillData : ScriptableObject
    {
        [Header("Meta")]
        [SerializeField] private string _skillName;
        [SerializeField] private SkillSlot _slot;
        [SerializeField] private Sprite _icon;

        [Header("Area")]
        [SerializeField] private SkillAreaType _areaType;

        [Header("Tiers (0: Base, 1: Enhanced)")]
        [SerializeField] private SkillTierData[] _tiers;

        public string SkillName => _skillName;
        public SkillSlot Slot => _slot;
        public Sprite Icon => _icon;
        public SkillAreaType AreaType => _areaType;

        public SkillTierData GetTier(int level) =>
            _tiers[Mathf.Clamp(level, 0, _tiers.Length - 1)];

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_tiers == null || _tiers.Length == 0)
            {
                Debug.LogWarning($"[{name}] No tiers configured");
            }
        }
#endif
    }
}
