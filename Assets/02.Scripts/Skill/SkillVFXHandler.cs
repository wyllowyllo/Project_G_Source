using Pool.Core;
using UnityEngine;

namespace Skill
{
    [System.Serializable]
    public class VFXConfig
    {
        [Tooltip("기본 프리팹 (랭크 1)")]
        public GameObject prefab;
        [Tooltip("랭크별 프리팹 (인덱스 0 = 랭크 1, 인덱스 1 = 랭크 2, ...)")]
        public GameObject[] rankPrefabs;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
    }

    [System.Serializable]
    public class RankEnhancementConfig
    {
        [Header("Emission")]
        [Tooltip("랭크당 Emission 강도 증가율")]
        public float emissionIntensityPerRank = 0.3f;

        [Header("Particle")]
        [Tooltip("랭크당 파티클 밀도 증가율")]
        public float particleDensityPerRank = 0.2f;
        [Tooltip("랭크당 파티클 크기 증가율")]
        public float particleSizePerRank = 0.1f;

        [Header("Simulation")]
        [Tooltip("랭크당 시뮬레이션 속도 증가율")]
        public float simulationSpeedPerRank = 0.05f;

        [Header("Overlay VFX")]
        [Tooltip("오버레이가 적용되는 최소 랭크")]
        public int overlayMinRank = 4;
        [Tooltip("랭크당 오버레이 스케일 배율")]
        public float overlayScalePerRank = 0.15f;
        [Tooltip("Sphere 스킬용 오버레이")]
        public GameObject sphereOverlayPrefab;
        [Tooltip("Sphere 오버레이 위치 오프셋 (로컬)")]
        public Vector3 sphereOverlayOffset;
        [Tooltip("Box 스킬용 오버레이")]
        public GameObject boxOverlayPrefab;
        [Tooltip("Box 오버레이 위치 오프셋 (로컬)")]
        public Vector3 boxOverlayOffset;
        [Tooltip("Cone 스킬용 오버레이")]
        public GameObject coneOverlayPrefab;
        [Tooltip("Cone 오버레이 위치 오프셋 (로컬)")]
        public Vector3 coneOverlayOffset;
    }

    public class SkillVFXHandler : MonoBehaviour
    {
        [Header("VFX Configs")]
        [SerializeField] private VFXConfig _sphereVFX;
        [SerializeField] private VFXConfig _boxVFX;
        [SerializeField] private VFXConfig _coneVFX;

        [Header("Settings")]
        [SerializeField] private bool _scaleToRange = true;

        [Header("Rank Enhancement")]
        [SerializeField] private bool _enableRankEnhancement = true;
        [SerializeField] private RankEnhancementConfig _rankConfig = new RankEnhancementConfig();

        private SkillHitbox _skillHitbox;

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            _skillHitbox = GetComponent<SkillHitbox>();
        }

        private void OnEnable()
        {
            if (_skillHitbox != null)
                _skillHitbox.OnVFXRequested += HandleVFXRequest;
        }

        private void OnDisable()
        {
            if (_skillHitbox != null)
                _skillHitbox.OnVFXRequested -= HandleVFXRequest;
        }

        private void HandleVFXRequest(SkillVFXRequest request)
        {
            VFXConfig config = GetVFXConfig(request.AreaType);
            if (config?.prefab == null) return;

            int rank = request.Rank;
            GameObject prefabToSpawn = GetPrefabForRank(config, rank);
            if (prefabToSpawn == null) return;

            Vector3 finalPosition = request.Origin + request.Rotation * config.positionOffset;
            Quaternion finalRotation = request.Rotation * Quaternion.Euler(config.rotationOffset);
            GameObject vfx = PoolSpawner.Spawn(prefabToSpawn, finalPosition, finalRotation);

            if (vfx == null) return;

            if (_scaleToRange)
            {
                ApplyScale(vfx, request);
            }

            if (_enableRankEnhancement && rank > 1)
            {
                ApplyRankEnhancement(vfx, rank, request.AreaType, finalPosition, finalRotation);
            }
        }

        private GameObject GetPrefabForRank(VFXConfig config, int rank)
        {
            if (config.rankPrefabs == null || config.rankPrefabs.Length == 0)
                return config.prefab;

            int index = rank - 1;
            if (index < config.rankPrefabs.Length && config.rankPrefabs[index] != null)
                return config.rankPrefabs[index];

            // 해당 랭크 프리팹이 없으면 가장 높은 랭크 프리팹 사용
            for (int i = config.rankPrefabs.Length - 1; i >= 0; i--)
            {
                if (config.rankPrefabs[i] != null)
                    return config.rankPrefabs[i];
            }

            return config.prefab;
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

        private void ApplyRankEnhancement(GameObject vfx, int rank, SkillAreaType areaType, Vector3 position, Quaternion rotation)
        {
            int bonusRank = rank - 1;

            ApplyEmissionEnhancement(vfx, bonusRank);
            ApplyParticleEnhancement(vfx, bonusRank);
            ApplySimulationSpeedEnhancement(vfx, bonusRank);
            SpawnOverlayVFX(rank, bonusRank, areaType, position, rotation);
        }

        private void ApplyEmissionEnhancement(GameObject vfx, int bonusRank)
        {
            float intensityMultiplier = 1f + bonusRank * _rankConfig.emissionIntensityPerRank;

            var renderers = vfx.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.materials)
                {
                    if (!mat.HasProperty(EmissionColor)) continue;

                    Color baseColor = mat.GetColor(EmissionColor);
                    mat.SetColor(EmissionColor, baseColor * intensityMultiplier);
                }
            }
        }

        private void ApplyParticleEnhancement(GameObject vfx, int bonusRank)
        {
            float densityMultiplier = 1f + bonusRank * _rankConfig.particleDensityPerRank;
            float sizeMultiplier = 1f + bonusRank * _rankConfig.particleSizePerRank;

            var particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var emission = ps.emission;
                emission.rateOverTimeMultiplier *= densityMultiplier;

                var main = ps.main;
                main.startSizeMultiplier *= sizeMultiplier;
            }
        }

        private void ApplySimulationSpeedEnhancement(GameObject vfx, int bonusRank)
        {
            float speedMultiplier = 1f + bonusRank * _rankConfig.simulationSpeedPerRank;

            var particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                main.simulationSpeed *= speedMultiplier;
            }
        }

        private void SpawnOverlayVFX(int rank, int bonusRank, SkillAreaType areaType, Vector3 position, Quaternion rotation)
        {
            if (rank < _rankConfig.overlayMinRank) return;

            var (prefab, offset) = areaType switch
            {
                SkillAreaType.Sphere => (_rankConfig.sphereOverlayPrefab, _rankConfig.sphereOverlayOffset),
                SkillAreaType.Box => (_rankConfig.boxOverlayPrefab, _rankConfig.boxOverlayOffset),
                SkillAreaType.Cone => (_rankConfig.coneOverlayPrefab, _rankConfig.coneOverlayOffset),
                _ => (null, Vector3.zero)
            };

            if (prefab == null) return;

            Vector3 finalPosition = position + rotation * offset;
            GameObject overlay = PoolSpawner.Spawn(prefab, finalPosition, rotation);
            if (overlay == null) return;

            float scaleMultiplier = 1f + bonusRank * _rankConfig.overlayScalePerRank;
            overlay.transform.localScale *= scaleMultiplier;
        }
    }
}
