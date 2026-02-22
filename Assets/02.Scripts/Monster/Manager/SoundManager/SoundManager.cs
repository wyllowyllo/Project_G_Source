using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    #region Audio Enums
    public enum EDungeonBgm
    {
        
    }

    public enum EPlayerSfx
    {
        // 예시: Attack, Jump, Hit, Die
    }

    public enum EEnemySfx
    {
        // 예시: Attack, Hit, Die
    }

    public enum EBossSfx
    {
        // 예시: Skill1, Skill2, Roar
    }

    public enum EUISfx
    {
        // 예시: ButtonClick, WindowOpen, ItemGet
        MainButtonClick,
        MainHover,
        PauseMenuOpen,
        PauseMenuButtonClick,
        PauseMenuGameEndClick,
        EquipmentUIOpen,
        DungeonClear,
        DungeonFail,
    }

    public enum EPlayerSkillSfx
    {
        // 예시: FireBall, IceSpike, Heal
    }

    public enum ESfx
    {
        // 기타 효과음
        SkillUIClick,
        EquipmentGet,
    }
    #endregion

    [Header("BGM Settings")]
    [SerializeField] private AudioClip[] _dungeonBgms;
    [Range(0f, 1f)]
    [SerializeField] private float _bgmVolume = 0.7f;
    
    [Header("SFX Settings")]
    [SerializeField] private AudioClip[] _playerSfxs;
    [SerializeField] private AudioClip[] _enemySfxs;
    [SerializeField] private AudioClip[] _bossSfxs;
    [SerializeField] private AudioClip[] _uISfxs;
    [SerializeField] private AudioClip[] _playerSkillSfxs;
    [SerializeField] private AudioClip[] _sfxs;
    [Range(0f, 1f)]
    [SerializeField] private float _sfxVolume = 0.8f;
    
    [Header("AudioSource Pool Settings")]
    [SerializeField] private int _sfxPoolSize = 10;

    private AudioSource _bgmSource;
    private List<AudioSource> _sfxSourcePool = new List<AudioSource>();
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeAudioSources();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void InitializeAudioSources()
    {
        // BGM AudioSource 생성
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.volume = _bgmVolume;

        // SFX AudioSource 풀 생성
        for (int i = 0; i < _sfxPoolSize; i++)
        {
            AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.volume = _sfxVolume;
            _sfxSourcePool.Add(sfxSource);
        }
    }

    public void PlayBgm(EDungeonBgm bgm, bool restart = false)
    {
        AudioClip clip = GetClip(_dungeonBgms, (int)bgm, bgm.ToString());
        if (clip == null) return;

        if (_bgmSource.clip == clip && _bgmSource.isPlaying && !restart)
            return;

        StopFade();
        _bgmSource.clip = clip;
        _bgmSource.Play();
    }

    // BGM 재생 (페이드 인)
    public void PlayBgmWithFade(EDungeonBgm bgm, float fadeDuration = 1f)
    {
        AudioClip clip = GetClip(_dungeonBgms, (int)bgm, bgm.ToString());
        if (clip == null) return;

        if (_bgmSource.clip == clip && _bgmSource.isPlaying)
            return;

        StopFade();
        _fadeCoroutine = StartCoroutine(FadeBgm(clip, fadeDuration));
    }

    // BGM 정지
    public void StopBgm(bool immediate = true)
    {
        if (immediate)
        {
            StopFade();
            _bgmSource.Stop();
        }
        else
        {
            StopFade();
            _fadeCoroutine = StartCoroutine(FadeOutBgm(1f));
        }
    }

    // BGM 일시정지
    public void PauseBgm()
    {
        _bgmSource.Pause();
    }

    // BGM 재개
    public void ResumeBgm()
    {
        _bgmSource.UnPause();
    }

    // BGM 볼륨 설정
    public void SetBgmVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);
        _bgmSource.volume = _bgmVolume;
    }

    // 현재 BGM 볼륨 가져오기
    public float GetBgmVolume()
    {
        return _bgmVolume;
    }

    // 플레이어 효과음 재생
    public void PlayPlayerSfx(EPlayerSfx sfx)
    {
        PlaySfxFromArray(_playerSfxs, (int)sfx, sfx.ToString());
    }

    // 적 효과음 재생
    public void PlayEnemySfx(EEnemySfx sfx)
    {
        PlaySfxFromArray(_enemySfxs, (int)sfx, sfx.ToString());
    }

    // 보스 효과음 재생
    public void PlayBossSfx(EBossSfx sfx)
    {
        PlaySfxFromArray(_bossSfxs, (int)sfx, sfx.ToString());
    }

    // UI 효과음 재생
    public void PlayUISfx(EUISfx sfx)
    {
        PlaySfxFromArray(_uISfxs, (int)sfx, sfx.ToString());
    }

    // 플레이어 스킬 효과음 재생
    public void PlayPlayerSkillSfx(EPlayerSkillSfx sfx)
    {
        PlaySfxFromArray(_playerSkillSfxs, (int)sfx, sfx.ToString());
    }

    // 일반 효과음 재생
    public void PlaySfx(ESfx sfx)
    {
        PlaySfxFromArray(_sfxs, (int)sfx, sfx.ToString());
    }

    // AudioClip 직접 재생
    public void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;

        AudioSource availableSource = GetAvailableSfxSource();
        if (availableSource != null)
        {
            availableSource.PlayOneShot(clip);
        }
    }

    // 3D 공간상에서 효과음 재생
    public void PlaySfx3D(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioClip is null");
            return;
        }

        AudioSource.PlayClipAtPoint(clip, position, volume * _sfxVolume);
    }

    // SFX 볼륨 설정
    public void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        foreach (var source in _sfxSourcePool)
        {
            source.volume = _sfxVolume;
        }
    }

    // 현재 SFX 볼륨 가져오기
    public float GetSfxVolume()
    {
        return _sfxVolume;
    }

    // 모든 SFX 정지
    public void StopAllSfx()
    {
        foreach (var source in _sfxSourcePool)
        {
            source.Stop();
        }
    }

    private void PlaySfxFromArray(AudioClip[] clips, int index, string name)
    {
        AudioClip clip = GetClip(clips, index, name);
        if (clip == null) return;

        AudioSource availableSource = GetAvailableSfxSource();
        if (availableSource != null)
        {
            availableSource.PlayOneShot(clip);
        }
    }

    private AudioSource GetAvailableSfxSource()
    {
        // 재생 중이지 않은 AudioSource 찾기
        foreach (var source in _sfxSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // 모든 소스가 사용 중이면 풀 확장
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.loop = false;
        newSource.volume = _sfxVolume;
        _sfxSourcePool.Add(newSource);
        
        Debug.LogWarning($"SFX pool expanded to {_sfxSourcePool.Count} sources");
        return newSource;
    }

    private AudioClip GetClip(AudioClip[] clips, int index, string name)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"AudioClip array for {name} is null or empty");
            return null;
        }

        if (index < 0 || index >= clips.Length)
        {
            Debug.LogWarning($"Index {index} is out of range for {name} (array length: {clips.Length})");
            return null;
        }

        if (clips[index] == null)
        {
            Debug.LogWarning($"AudioClip for {name} at index {index} is null");
            return null;
        }

        return clips[index];
    }

    private void StopFade()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }
    }

    private IEnumerator FadeBgm(AudioClip newClip, float duration)
    {
        // 이전 BGM 페이드 아웃
        if (_bgmSource.isPlaying)
        {
            float startVolume = _bgmSource.volume;
            float elapsed = 0f;

            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2f));
                yield return null;
            }
        }

        // 새 BGM으로 교체
        _bgmSource.clip = newClip;
        _bgmSource.Play();

        // 새 BGM 페이드 인
        float targetVolume = _bgmVolume;
        float fadeInElapsed = 0f;

        while (fadeInElapsed < duration / 2f)
        {
            fadeInElapsed += Time.deltaTime;
            _bgmSource.volume = Mathf.Lerp(0f, targetVolume, fadeInElapsed / (duration / 2f));
            yield return null;
        }

        _bgmSource.volume = targetVolume;
        _fadeCoroutine = null;
    }

    private IEnumerator FadeOutBgm(float duration)
    {
        float startVolume = _bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        _bgmSource.Stop();
        _bgmSource.volume = _bgmVolume;
        _fadeCoroutine = null;
    }
}
