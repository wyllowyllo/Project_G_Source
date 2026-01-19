using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class BtnType : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ButtonType _currentType;
    [SerializeField] private Transform _buttonScale;
    private Vector3 _defaultScale;
    private float _rateScale = 1.2f;

    public void Start()
    {
        _defaultScale = _buttonScale.localScale;
    }
    public void OnBtnClick()
    {
        switch (_currentType)
        {
            case ButtonType.Start:
                SceneLoader.LoadScene("TownScene");
                break;

            case ButtonType.End:
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
        _buttonScale.localScale = _defaultScale * _rateScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _buttonScale.localScale = _defaultScale;
    }
}
