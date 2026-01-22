using System;
using UnityEngine;

namespace Player
{
    public enum GroundType
    {
        Default,
        Grass,
        Stone,
        Water,
        Sand,
        Snow,
        Dirt
    }

    [CreateAssetMenu(fileName = "FootstepSoundData", menuName = "Audio/Footstep Sound Data")]
    public class FootstepSoundData : ScriptableObject
    {
        [Serializable]
        public class SurfaceMapping
        {
            public PhysicsMaterial Material;
            public GroundType Type;
        }

        [Serializable]
        public class TerrainLayerMapping
        {
            public TerrainLayer Layer;
            public GroundType Type;
        }

        [Serializable]
        public class RenderMaterialMapping
        {
            public Material Material;
            public GroundType Type;
        }

        [Serializable]
        public class FootstepSounds
        {
            public GroundType Type;
            public AudioClip[] Clips;
        }

        [Header("Surface Mappings (PhysicsMaterial)")]
        [Tooltip("PhysicMaterial과 GroundType 매핑")]
        [SerializeField] private SurfaceMapping[] _surfaceMappings;

        [Header("Terrain Layer Mappings")]
        [Tooltip("TerrainLayer와 GroundType 매핑")]
        [SerializeField] private TerrainLayerMapping[] _terrainLayerMappings;

        [Header("Render Material Mappings")]
        [Tooltip("렌더링 Material과 GroundType 매핑")]
        [SerializeField] private RenderMaterialMapping[] _renderMaterialMappings;

        [Header("Footstep Sounds")]
        [SerializeField] private FootstepSounds[] _footstepSounds;

        [Header("Settings")]
        [SerializeField] private float _volumeVariation = 0.1f;
        [SerializeField] private float _pitchVariation = 0.1f;

        public float VolumeVariation => _volumeVariation;
        public float PitchVariation => _pitchVariation;

        private System.Collections.Generic.Dictionary<PhysicsMaterial, GroundType> _materialToTypeCache;
        private System.Collections.Generic.Dictionary<TerrainLayer, GroundType> _terrainLayerToTypeCache;
        private System.Collections.Generic.Dictionary<Material, GroundType> _renderMaterialToTypeCache;
        private System.Collections.Generic.Dictionary<GroundType, AudioClip[]> _typeToClipsCache;

        public void Initialize()
        {
            BuildMaterialCache();
            BuildTerrainLayerCache();
            BuildRenderMaterialCache();
            BuildClipsCache();
        }

        private void BuildMaterialCache()
        {
            _materialToTypeCache = new System.Collections.Generic.Dictionary<PhysicsMaterial, GroundType>();

            if (_surfaceMappings == null) return;

            foreach (var mapping in _surfaceMappings)
            {
                if (mapping.Material != null && !_materialToTypeCache.ContainsKey(mapping.Material))
                {
                    _materialToTypeCache[mapping.Material] = mapping.Type;
                }
            }
        }

        private void BuildTerrainLayerCache()
        {
            _terrainLayerToTypeCache = new System.Collections.Generic.Dictionary<TerrainLayer, GroundType>();

            if (_terrainLayerMappings == null) return;

            foreach (var mapping in _terrainLayerMappings)
            {
                if (mapping.Layer != null && !_terrainLayerToTypeCache.ContainsKey(mapping.Layer))
                {
                    _terrainLayerToTypeCache[mapping.Layer] = mapping.Type;
                }
            }
        }

        private void BuildRenderMaterialCache()
        {
            _renderMaterialToTypeCache = new System.Collections.Generic.Dictionary<Material, GroundType>();

            if (_renderMaterialMappings == null) return;

            foreach (var mapping in _renderMaterialMappings)
            {
                if (mapping.Material != null && !_renderMaterialToTypeCache.ContainsKey(mapping.Material))
                {
                    _renderMaterialToTypeCache[mapping.Material] = mapping.Type;
                }
            }
        }

        private void BuildClipsCache()
        {
            _typeToClipsCache = new System.Collections.Generic.Dictionary<GroundType, AudioClip[]>();

            if (_footstepSounds == null) return;

            foreach (var footstep in _footstepSounds)
            {
                if (!_typeToClipsCache.ContainsKey(footstep.Type))
                {
                    _typeToClipsCache[footstep.Type] = footstep.Clips;
                }
            }
        }

        public GroundType GetGroundType(PhysicsMaterial material)
        {
            if (material == null) return GroundType.Default;

            if (_materialToTypeCache == null)
            {
                BuildMaterialCache();
            }

            return _materialToTypeCache.TryGetValue(material, out var type) ? type : GroundType.Default;
        }

        public GroundType GetGroundType(TerrainLayer layer)
        {
            if (layer == null) return GroundType.Default;

            if (_terrainLayerToTypeCache == null)
            {
                BuildTerrainLayerCache();
            }

            return _terrainLayerToTypeCache.TryGetValue(layer, out var type) ? type : GroundType.Default;
        }

        public GroundType GetGroundType(Material material)
        {
            if (material == null) return GroundType.Default;

            if (_renderMaterialToTypeCache == null)
            {
                BuildRenderMaterialCache();
            }

            return _renderMaterialToTypeCache.TryGetValue(material, out var type) ? type : GroundType.Default;
        }

        public AudioClip GetRandomClip(GroundType type)
        {
            if (_typeToClipsCache == null)
            {
                BuildClipsCache();
            }

            if (!_typeToClipsCache.TryGetValue(type, out var clips) || clips == null || clips.Length == 0)
            {
                if (type != GroundType.Default && _typeToClipsCache.TryGetValue(GroundType.Default, out var defaultClips))
                {
                    clips = defaultClips;
                }
                else
                {
                    return null;
                }
            }

            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }
    }
}
