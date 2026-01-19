using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class BtnType : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ButtonType CurrentType;
    public Transform ButtonScale;
    private Vector3 _defaultScale;
    private float _rateScale = 1.2f;

    public void Start()
    {
        _defaultScale = ButtonScale.localScale;
    }
    public void OnBtnClick()
    {
        switch (CurrentType)
        {
            case ButtonType.Start:
                SceneLoader.LoadScene("TownScene");
                break;

            case ButtonType.End:
                Application.Quit();
                Debug.Log("게임종료");
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ButtonScale.localScale = _defaultScale * _rateScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ButtonScale.localScale = _defaultScale;
    }
}
