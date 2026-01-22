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

public class PauseMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private PauseButtonType _buttonType;

    [Header("Hover Scale")]
    [SerializeField] private Transform _buttonScale;
    [SerializeField] private float _rateScale = 1.2f;

    private Vector3 _defaultScale;

    private void Start()
    {
        if (_buttonScale != null)
        {
            _defaultScale = _buttonScale.localScale;
        }
    }

    public void OnClick()
    {
        switch (_buttonType)
        {
            case PauseButtonType.Continue:
                SoundManager.Instance.PlayUISfx(SoundManager.EUISfx.PauseMenuButtonClick);
                if (PauseManager.Instance != null)
                {
                    PauseManager.Instance.Resume();
                }
                break;

            case PauseButtonType.MainMenu:
                SoundManager.Instance.PlayUISfx(SoundManager.EUISfx.PauseMenuButtonClick);
                Time.timeScale = 1f;
                SceneLoader.LoadScene("MainScene");
                break;

            case PauseButtonType.GameEnd:
                SoundManager.Instance.PlayUISfx(SoundManager.EUISfx.PauseMenuGameEndClick);
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
