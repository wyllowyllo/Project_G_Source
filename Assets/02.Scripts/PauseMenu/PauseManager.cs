using Common;
using System;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Options")]
    [SerializeField] private bool pauseTimeScale = true;

    private bool IsPaused = false;

    public static event Action<bool> OnPauseStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetPaused(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void Pause()
    {
        SetPaused(true);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;

        OnPauseStateChanged?.Invoke(paused);

        if (CursorManager.Instance != null)
        {
            if (paused)
            {
                CursorManager.Instance.UnlockCursor();
            }
            else
            {
                CursorManager.Instance.LockCursor();
            }
        }

        if (pauseTimeScale)
        { 
            Time.timeScale = paused ? 0f : 1f;
        }

        if (pausePanel != null)
        { 
            pausePanel.SetActive(paused);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this && Time.timeScale == 0f)
        { 
            Time.timeScale = 1f;
        }
    }
}
