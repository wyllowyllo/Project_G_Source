using Pool.Core;
using UnityEngine;

namespace Skill
{
    [System.Serializable]
    public class VFXConfig
    {
        public GameObject prefab;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
    }

    public class SkillVFXHandler : MonoBehaviour
    {
        [Header("VFX Configs")]
        [SerializeField] private VFXConfig _sphereVFX;
        [SerializeField] private VFXConfig _boxVFX;
        [SerializeField] private VFXConfig _coneVFX;

        [Header("Settings")]
        [SerializeField] private bool _scaleToRange = true;

        private SkillHitbox _skillHitbox;

        private void Awake()
        {
            _skillHitbox = GetComponent<SkillHitbox>();
        }

        private void OnEnable()
        {
            if (_skillHitbox != null)
            {
                _skillHitbox.OnVFXRequested += HandleVFXRequest;
            }
        }

        private void OnDisable()
        {
            if (_skillHitbox != null)
            {
                _skillHitbox.OnVFXRequested -= HandleVFXRequest;
            }
        }

        private void HandleVFXRequest(SkillVFXRequest request)
        {
            VFXConfig config = GetVFXConfig(request.AreaType);
            if (config?.prefab == null) return;

            Vector3 finalPosition = request.Origin + request.Rotation * config.positionOffset;
            Quaternion finalRotation = request.Rotation * Quaternion.Euler(config.rotationOffset);
            GameObject vfx = PoolSpawner.Spawn(config.prefab, finalPosition, finalRotation);

            if (_scaleToRange && vfx != null)
            {
                ApplyScale(vfx, request);
            }
        }

        private VFXConfig GetVFXConfig(SkillAreaType areaType)
        {
            return areaType switch
            {
                SkillAreaType.Sphere => _sphereVFX,
                SkillAreaType.Box => _boxVFX,
                SkillAreaType.Cone => _coneVFX,
                _ => null
            };
        }

        private void ApplyScale(GameObject vfx, SkillVFXRequest request)
        {
            Vector3 scale = request.AreaType switch
            {
                SkillAreaType.Sphere => Vector3.one * request.Range * 2f,
                SkillAreaType.Box => new Vector3(request.BoxWidth, request.BoxHeight, request.Range),
                SkillAreaType.Cone => new Vector3(request.Range, request.ConeHeight, request.Range),
                _ => Vector3.one
            };

            vfx.transform.localScale = scale;
        }
    }
}
