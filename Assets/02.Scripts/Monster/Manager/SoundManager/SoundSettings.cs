using UnityEngine;

/// <summary>
/// 사운드 설정을 저장하고 불러오는 유틸리티 클래스
/// PlayerPrefs를 사용하여 볼륨 설정을 영구 저장합니다
/// </summary>
public class SoundSettings : MonoBehaviour
{
    private const string BGM_VOLUME_KEY = "Settings_BGM_Volume";
    private const string SFX_VOLUME_KEY = "Settings_SFX_Volume";
    private const string BGM_MUTE_KEY = "Settings_BGM_Mute";
    private const string SFX_MUTE_KEY = "Settings_SFX_Mute";

    private float _previousBgmVolume = 1f;
    private float _previousSfxVolume = 1f;

    private void Start()
    {
        LoadSettings();
    }

    /// <summary>
    /// 저장된 설정 불러오기
    /// </summary>
    public void LoadSettings()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("SoundManager instance not found");
            return;
        }

        // BGM 볼륨
        float bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.7f);
        SoundManager.Instance.SetBgmVolume(bgmVolume);
        _previousBgmVolume = bgmVolume;

        // SFX 볼륨
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);
        SoundManager.Instance.SetSfxVolume(sfxVolume);
        _previousSfxVolume = sfxVolume;

        // 음소거 상태
        bool bgmMuted = PlayerPrefs.GetInt(BGM_MUTE_KEY, 0) == 1;
        if (bgmMuted)
        {
            SoundManager.Instance.SetBgmVolume(0f);
        }

        bool sfxMuted = PlayerPrefs.GetInt(SFX_MUTE_KEY, 0) == 1;
        if (sfxMuted)
        {
            SoundManager.Instance.SetSfxVolume(0f);
        }

        Debug.Log($"Sound settings loaded - BGM: {bgmVolume}, SFX: {sfxVolume}");
    }

    /// <summary>
    /// 현재 설정 저장
    /// </summary>
    public void SaveSettings()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("SoundManager instance not found");
            return;
        }

        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, SoundManager.Instance.GetBgmVolume());
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, SoundManager.Instance.GetSfxVolume());
        PlayerPrefs.Save();

        Debug.Log("Sound settings saved");
    }

    /// <summary>
    /// BGM 볼륨 설정 및 저장
    /// </summary>
    public void SetAndSaveBgmVolume(float volume)
    {
        if (SoundManager.Instance == null) return;

        SoundManager.Instance.SetBgmVolume(volume);
        _previousBgmVolume = volume;
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, volume);
        PlayerPrefs.SetInt(BGM_MUTE_KEY, 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// SFX 볼륨 설정 및 저장
    /// </summary>
    public void SetAndSaveSfxVolume(float volume)
    {
        if (SoundManager.Instance == null) return;

        SoundManager.Instance.SetSfxVolume(volume);
        _previousSfxVolume = volume;
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        PlayerPrefs.SetInt(SFX_MUTE_KEY, 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// BGM 음소거 토글
    /// </summary>
    public void ToggleBgmMute()
    {
        if (SoundManager.Instance == null) return;

        bool isMuted = SoundManager.Instance.GetBgmVolume() == 0f;
        
        if (isMuted)
        {
            // 음소거 해제
            SoundManager.Instance.SetBgmVolume(_previousBgmVolume);
            PlayerPrefs.SetInt(BGM_MUTE_KEY, 0);
        }
        else
        {
            // 음소거
            _previousBgmVolume = SoundManager.Instance.GetBgmVolume();
            SoundManager.Instance.SetBgmVolume(0f);
            PlayerPrefs.SetInt(BGM_MUTE_KEY, 1);
        }
        
        PlayerPrefs.Save();
    }

    /// <summary>
    /// SFX 음소거 토글
    /// </summary>
    public void ToggleSfxMute()
    {
        if (SoundManager.Instance == null) return;

        bool isMuted = SoundManager.Instance.GetSfxVolume() == 0f;
        
        if (isMuted)
        {
            // 음소거 해제
            SoundManager.Instance.SetSfxVolume(_previousSfxVolume);
            PlayerPrefs.SetInt(SFX_MUTE_KEY, 0);
        }
        else
        {
            // 음소거
            _previousSfxVolume = SoundManager.Instance.GetSfxVolume();
            SoundManager.Instance.SetSfxVolume(0f);
            PlayerPrefs.SetInt(SFX_MUTE_KEY, 1);
        }
        
        PlayerPrefs.Save();
    }

    /// <summary>
    /// BGM이 음소거 상태인지 확인
    /// </summary>
    public bool IsBgmMuted()
    {
        return SoundManager.Instance != null && SoundManager.Instance.GetBgmVolume() == 0f;
    }

    /// <summary>
    /// SFX가 음소거 상태인지 확인
    /// </summary>
    public bool IsSfxMuted()
    {
        return SoundManager.Instance != null && SoundManager.Instance.GetSfxVolume() == 0f;
    }

    /// <summary>
    /// 설정 초기화 (기본값으로 복원)
    /// </summary>
    public void ResetToDefault()
    {
        SetAndSaveBgmVolume(0.7f);
        SetAndSaveSfxVolume(0.8f);
        PlayerPrefs.SetInt(BGM_MUTE_KEY, 0);
        PlayerPrefs.SetInt(SFX_MUTE_KEY, 0);
        PlayerPrefs.Save();

        Debug.Log("Sound settings reset to default");
    }
}
