using UnityEngine;

namespace Skill
{
    public class SkillVFXHandler : MonoBehaviour
    {
        [Header("VFX Prefabs")]
        [SerializeField] private GameObject _sphereVFX;
        [SerializeField] private GameObject _boxVFX;
        [SerializeField] private GameObject _coneVFX;

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
            GameObject prefab = GetVFXPrefab(request.AreaType);
            if (prefab == null) return;

            GameObject vfx = Instantiate(prefab, request.Origin, prefab.transform.rotation);

            if (_scaleToRange)
            {
                ApplyScale(vfx, request);
            }
        }

        private GameObject GetVFXPrefab(SkillAreaType areaType)
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
