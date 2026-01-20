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
    [SerializeField] private PauseButtonType buttonType;

    [Header("Hover Scale")]
    [SerializeField] private Transform buttonScale;
    [SerializeField] private float rateScale = 1.2f;

    private Vector3 defaultScale;

    private void Start()
    {
        if (buttonScale != null)
        {
            defaultScale = buttonScale.localScale;
        }
    }

    public void OnClick()
    {
        switch (buttonType)
        {
            case PauseButtonType.Continue:
                if (PauseManager.Instance != null)
                {
                    PauseManager.Instance.Resume();
                }
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
        if (buttonScale != null)
        {
            buttonScale.localScale = defaultScale * rateScale;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonScale != null)
        {
            buttonScale.localScale = defaultScale;
        }
    }
}
