using Common;
using Core;
using UnityEngine;
using UnityEngine.EventSystems;

public enum PauseButtonType
{
    Continue,
    MainMenu,
    GameEnd,
}

public class PauseBtnType : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private PauseButtonType _currentType;
    [SerializeField] private Transform _buttonScale;
    private Vector3 _defaultScale;
    private float _rateScale = 1.2f;

    [SerializeField] private GameObject _pausePanel;

    private bool _isPaused = false;

    public void Start()
    {
        if (_buttonScale != null)
        {
            _defaultScale = _buttonScale.localScale;
        }

        if (_pausePanel != null)
        {
            _pausePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (_isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        _isPaused = true;
        Time.timeScale = 0f;

        if (_pausePanel != null)
        {
            _pausePanel.SetActive(true);
        }

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.UnlockCursor();
        }
    }

    private void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        if (_pausePanel != null)
        {
            _pausePanel.SetActive(false);
        }

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.LockCursor();
        }
    }

    public void OnBtnClick()
    {
        switch (_currentType)
        {
            case PauseButtonType.Continue:
                ResumeGame();
                break;

            case PauseButtonType.MainMenu:
                Time.timeScale = 1f;
                SceneLoader.LoadScene("MainScene");
                break;

            case PauseButtonType.GameEnd:
                Time.timeScale = 1f;
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                Debug.Log("게임종료");
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_buttonScale != null)
        {
            _buttonScale.localScale = _defaultScale * _rateScale;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_buttonScale != null)
        {
            _buttonScale.localScale = _defaultScale;
        }
    }
}