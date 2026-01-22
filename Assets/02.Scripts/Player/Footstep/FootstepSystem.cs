using UnityEngine;

namespace Player
{
    public class FootstepSystem : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private FootstepSoundData _soundData;

        [Header("Foot Tracking")]
        [Tooltip("왼발 본 Transform (Humanoid: LeftFoot)")]
        [SerializeField] private Transform _leftFoot;
        [Tooltip("오른발 본 Transform (Humanoid: RightFoot)")]
        [SerializeField] private Transform _rightFoot;
        [Tooltip("지면 판정 높이 (캐릭터 기준 로컬 Y)")]
        [SerializeField] private float _groundThreshold = 0.15f;
        [Tooltip("발소리 최소 간격 (같은 발)")]
        [SerializeField] private float _minStepInterval = 0.2f;

        [Header("Raycast Settings")]
        [SerializeField] private float _raycastDistance = 0.5f;
        [SerializeField] private LayerMask _groundLayer;

        [Header("Water Detection")]
        [Tooltip("물 레이어 (위로 레이캐스트해서 물 속인지 확인)")]
        [SerializeField] private LayerMask _waterLayer;
        [Tooltip("물 감지 위로 레이캐스트 거리")]
        [SerializeField] private float _waterCheckDistance = 2f;

        [Header("Audio Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float _baseVolume = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        private AudioSource _audioSource;
        private GroundType _currentGroundType = GroundType.Default;

        private bool _leftFootWasGrounded;
        private bool _rightFootWasGrounded;
        private float _lastLeftStepTime;
        private float _lastRightStepTime;

        private Animator _animator;

        private void Awake()
        {
            InitializeAudioSource();
            _animator = GetComponent<Animator>();

            if (_soundData != null)
            {
                _soundData.Initialize();
            }
        }

        private void Start()
        {
            TryAutoAssignFeet();
        }

        private void TryAutoAssignFeet()
        {
            if (_animator == null || !_animator.isHuman) return;

            if (_leftFoot == null)
            {
                _leftFoot = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            }

            if (_rightFoot == null)
            {
                _rightFoot = _animator.GetBoneTransform(HumanBodyBones.RightFoot);
            }

            if (_debugMode)
            {
                Debug.Log($"[FootstepSystem] 발 본 자동 할당 - 왼발: {_leftFoot?.name}, 오른발: {_rightFoot?.name}");
            }
        }

        private void InitializeAudioSource()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = _baseVolume;
        }

        private void Update()
        {
            if (_soundData == null) return;

            TrackFoot(_leftFoot, ref _leftFootWasGrounded, ref _lastLeftStepTime, "왼발");
            TrackFoot(_rightFoot, ref _rightFootWasGrounded, ref _lastRightStepTime, "오른발");
        }

        private void TrackFoot(Transform foot, ref bool wasGrounded, ref float lastStepTime, string footName)
        {
            if (foot == null) return;

            float footLocalY = transform.InverseTransformPoint(foot.position).y;
            bool isGrounded = footLocalY < _groundThreshold;

            if (isGrounded && !wasGrounded)
            {
                if (Time.time - lastStepTime >= _minStepInterval)
                {
                    lastStepTime = Time.time;
                    OnFootStep(foot.position);

                    if (_debugMode)
                    {
                        Debug.Log($"[FootstepSystem] {footName} 착지 - Y: {footLocalY:F3}, GroundType: {_currentGroundType}");
                    }
                }
            }

            wasGrounded = isGrounded;
        }

        private void OnFootStep(Vector3 footPosition)
        {
            UpdateCurrentGroundType(footPosition);
            PlayFootstepSound();
        }

        private void UpdateCurrentGroundType(Vector3 footPosition)
        {
            if (IsInWater())
            {
                _currentGroundType = GroundType.Water;

                if (_debugMode)
                {
                    Debug.Log("[FootstepSystem] 물 속 감지 - GroundType: Water");
                }
                return;
            }

            Vector3 rayOrigin = footPosition + Vector3.up * 0.1f;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, _raycastDistance, _groundLayer))
            {
                _currentGroundType = GetGroundTypeFromHit(hit);

                if (_debugMode)
                {
                    Debug.Log($"[FootstepSystem] 지면 감지 - 오브젝트: {hit.collider.name}, GroundType: {_currentGroundType}");
                }
            }
            else
            {
                _currentGroundType = GroundType.Default;
            }
        }

        private bool IsInWater()
        {
            if (_waterLayer == 0) return false;

            return Physics.Raycast(transform.position, Vector3.up, _waterCheckDistance, _waterLayer);
        }

        private GroundType GetGroundTypeFromHit(RaycastHit hit)
        {
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain != null)
            {
                return GetGroundTypeFromTerrain(terrain, hit.point);
            }

            PhysicsMaterial physicsMaterial = hit.collider.sharedMaterial;
            if (physicsMaterial != null)
            {
                GroundType type = _soundData.GetGroundType(physicsMaterial);
                if (type != GroundType.Default)
                {
                    return type;
                }
            }

            MeshRenderer meshRenderer = hit.collider.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.sharedMaterial != null)
            {
                return _soundData.GetGroundType(meshRenderer.sharedMaterial);
            }

            return GroundType.Default;
        }

        private GroundType GetGroundTypeFromTerrain(Terrain terrain, Vector3 worldPosition)
        {
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPosition = terrain.transform.position;

            int mapX = Mathf.RoundToInt((worldPosition.x - terrainPosition.x) / terrainData.size.x * terrainData.alphamapWidth);
            int mapZ = Mathf.RoundToInt((worldPosition.z - terrainPosition.z) / terrainData.size.z * terrainData.alphamapHeight);

            mapX = Mathf.Clamp(mapX, 0, terrainData.alphamapWidth - 1);
            mapZ = Mathf.Clamp(mapZ, 0, terrainData.alphamapHeight - 1);

            float[,,] alphaMap = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
            TerrainLayer[] layers = terrainData.terrainLayers;

            int dominantLayerIndex = 0;
            float maxWeight = 0f;

            for (int i = 0; i < layers.Length; i++)
            {
                if (alphaMap[0, 0, i] > maxWeight)
                {
                    maxWeight = alphaMap[0, 0, i];
                    dominantLayerIndex = i;
                }
            }

            if (layers.Length > dominantLayerIndex)
            {
                return _soundData.GetGroundType(layers[dominantLayerIndex]);
            }

            return GroundType.Default;
        }

        private void PlayFootstepSound()
        {
            AudioClip clip = _soundData.GetRandomClip(_currentGroundType);

            if (clip == null)
            {
                if (_debugMode)
                {
                    Debug.LogWarning($"[FootstepSystem] AudioClip이 null - GroundType: {_currentGroundType}");
                }
                return;
            }

            float volumeVariation = Random.Range(-_soundData.VolumeVariation, _soundData.VolumeVariation);
            float pitchVariation = Random.Range(-_soundData.PitchVariation, _soundData.PitchVariation);

            _audioSource.volume = Mathf.Clamp01(_baseVolume + volumeVariation);
            _audioSource.pitch = 1f + pitchVariation;
            _audioSource.PlayOneShot(clip);

            if (_debugMode)
            {
                Debug.Log($"[FootstepSystem] 사운드 재생 - Clip: {clip.name}, GroundType: {_currentGroundType}");
            }
        }

        public GroundType GetCurrentGroundType()
        {
            return _currentGroundType;
        }

        private void OnDrawGizmosSelected()
        {
            if (_leftFoot != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(_leftFoot.position, 0.05f);
                Gizmos.DrawLine(_leftFoot.position, _leftFoot.position + Vector3.down * _raycastDistance);
            }

            if (_rightFoot != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_rightFoot.position, 0.05f);
                Gizmos.DrawLine(_rightFoot.position, _rightFoot.position + Vector3.down * _raycastDistance);
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * _waterCheckDistance);

            float thresholdY = transform.position.y + _groundThreshold;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                new Vector3(transform.position.x, thresholdY, transform.position.z),
                new Vector3(1f, 0.01f, 1f)
            );
        }
    }
}
