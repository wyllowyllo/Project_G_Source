using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIExp : MonoBehaviour
{
    [SerializeField] private Slider _expbar;
    [SerializeField] private float _expLerpSpeed = 5f;

    [SerializeField] private Image _imageExpComplete;

    [SerializeField] private TextMeshProUGUI _expText;

    private float _displayExp;
    private float _targetExp;

    private float _maxExp = 100;
    private float _curExp = 0;

    private void Start()
    {
        _expbar.value = (float)_curExp / (float)_maxExp;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Y))
        {
            _targetExp += 10;

            if (_targetExp >= 100)
            {
                StartCoroutine(ImageExpComplete_Coroutine());
                StartCoroutine(ImageExpCompleteMore_Coroutine());
                StartCoroutine(LevelUpExp_Coroutine());
            }
        }

        SmoothExpUpdate();
        Handle();
    }

    private void SmoothExpUpdate()
    {
        _displayExp = Mathf.Lerp(_displayExp, _targetExp, Time.deltaTime * _expLerpSpeed);

        _curExp = Mathf.RoundToInt(_displayExp);
    }

    private void Handle()
    {
        _expbar.value = _displayExp / _maxExp;
        if (_expText != null)
        {
            _expText.text = $"{Mathf.RoundToInt(_curExp)}/{Mathf.RoundToInt(_maxExp)}";
        }
    }

    private IEnumerator LevelUpExp_Coroutine()
    {
        yield return new WaitForSeconds(4f);

        _targetExp = 0;
        _displayExp = 0;
        _curExp = 0;
    }

    private IEnumerator ImageExpComplete_Coroutine()
    {
        Color color = new Color32(255, 247, 177, 255);
        color.a = 1f;

        _imageExpComplete.color = color;
        _imageExpComplete.gameObject.SetActive(true);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            _imageExpComplete.color = color;
            yield return null;
        }

        color.a = 0f;
        _imageExpComplete.color = color;
        _imageExpComplete.gameObject.SetActive(false);
    }

    private IEnumerator ImageExpCompleteMore_Coroutine()
    {
        yield return new WaitForSeconds(2f);
        Color color = new Color32(255, 247, 177, 255);
        color.a = 1f;

        _imageExpComplete.color = color;
        _imageExpComplete.gameObject.SetActive(true);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            _imageExpComplete.color = color;
            yield return null;
        }

        color.a = 0f;
        _imageExpComplete.color = color;
        _imageExpComplete.gameObject.SetActive(false);
    }


}
