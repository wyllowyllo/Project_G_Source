using System.Collections;
using UnityEngine;

/// <summary>
/// 랭크업 이펙트 재생을 담당하는 컨트롤러
/// 단일 책임: 파티클, 사운드, 카메라 흔들림 등 이펙트 재생
/// </summary>
public class RankEffectsController : MonoBehaviour
{
    [Header("파티클 시스템")]
    [Tooltip("랭크 업 파티클 시스템")]
    [SerializeField] private ParticleSystem _rankUpParticleSystem;

    [Header("광선 설정")]
    [Tooltip("광선 프리팹")]
    [SerializeField] private GameObject _lightRayPrefab;

    [Tooltip("광선 개수")]
    [Range(8, 32)]
    [SerializeField] private int _rayCount = 16;

    [Header("이펙트 설정")]
    [Tooltip("플래시 효과 사용")]
    [SerializeField] private bool _useFlash = true;

    [SerializeField] private Color _flashColor = Color.white;

    [Header("카메라 쉐이크")]
    [Tooltip("카메라 쉐이크 강도")]
    [Range(0f, 1f)]
    [SerializeField] private float _cameraShakeIntensity = 0.2f;

    [Header("사운드")]
    [Tooltip("랭크 강화 효과음")]
    [SerializeField] private AudioClip _rankUpSound;

    private AudioSource _audioSource;
    private Camera _mainCamera;
    private Vector3 _originalCameraPos;

    private void Awake()
    {
        // AudioSource 초기화
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        // 메인 카메라 참조
        _mainCamera = Camera.main;
        if (_mainCamera != null)
            _originalCameraPos = _mainCamera.transform.position;
    }

    /// <summary>
    /// 모든 이펙트 재생
    /// </summary>
    public void PlayEffects()
    {
        PlayParticles();
        PlaySound();
        
        if (_cameraShakeIntensity > 0f)
        {
            StartCoroutine(CameraShake());
        }
    }

    /// <summary>
    /// 파티클 시스템 재생
    /// </summary>
    public void PlayParticles()
    {
        if (_rankUpParticleSystem != null)
        {
            _rankUpParticleSystem.Stop();
            _rankUpParticleSystem.Clear();
            _rankUpParticleSystem.Play();
        }
    }

    /// <summary>
    /// 사운드 재생
    /// </summary>
    public void PlaySound()
    {
        if (_rankUpSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_rankUpSound);
        }
    }

    /// <summary>
    /// 카메라 쉐이크 효과
    /// </summary>
    private IEnumerator CameraShake()
    {
        if (_mainCamera == null) yield break;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = _cameraShakeIntensity * (1f - elapsed / duration);

            Vector3 offset = Random.insideUnitSphere * strength;
            _mainCamera.transform.position = _originalCameraPos + offset;

            yield return null;
        }

        _mainCamera.transform.position = _originalCameraPos;
    }

    /// <summary>
    /// 이펙트 설정 변경
    /// </summary>
    public void ApplyEffectSettings(float cameraShake, int rayCount, bool useFlash)
    {
        _cameraShakeIntensity = cameraShake;
        _rayCount = rayCount;
        _useFlash = useFlash;
    }

    /// <summary>
    /// 카메라 쉐이크 강도 설정
    /// </summary>
    public void SetCameraShakeIntensity(float intensity)
    {
        _cameraShakeIntensity = Mathf.Clamp01(intensity);
    }

    /// <summary>
    /// 플래시 색상 설정
    /// </summary>
    public void SetFlashColor(Color color)
    {
        _flashColor = color;
    }

    private void OnDestroy()
    {
        // 카메라 위치 복원
        if (_mainCamera != null)
        {
            _mainCamera.transform.position = _originalCameraPos;
        }
    }
}
