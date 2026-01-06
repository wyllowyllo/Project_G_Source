using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Codice.CM.Common.CmCallContext;

public class PlayerHpBarUI : MonoBehaviour
{
    private static PlayerHpBarUI _instance;
    public Slider HpSlider;

    [SerializeField] private Image _hpFillImage;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _hpText;

    [SerializeField] private int _maxHp = 100;
    [SerializeField] private int _currentHp = 100;
    [SerializeField] private int _level = 30;

    private float _smoothSpeed = 5f;

    private float _hp;

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateHpBar();
    }

    private void Update()
    {
        if(_currentHp < 0)
        {
            GameOver();
        }

        HandleHp();
    }

    private void UpdateHpBar()
    {
        HpSlider.value = (float)_currentHp / (float)_maxHp;

        _levelText.text = $"Lv.{_level}";
        _hpText.text = $"{_currentHp} / {_maxHp}";

        UpdateHpColor();

    }

    private void UpdateHpColor()
    {
        if(HpSlider.value > 0.5f)
        {
            _hpFillImage.color = new Color(0.6f, 1f, 0.4f);
        }
        else if(HpSlider.value > 0.2f)
        {
            _hpFillImage.color = new Color(1f, 0.8f, 0f);
        }
        else
        {
            _hpFillImage.color = new Color(1f, 0.3f, 0.3f);
        }
    }

    public void TakeDamage(int damage)
    {
        _currentHp = Mathf.Max(0, _currentHp - damage);
        UpdateHpBar();
    }

    public void Heal(int healAmount)
    {
        _currentHp = Mathf.Min(0, _currentHp + healAmount);
        UpdateHpBar();
    }

    private void HandleHp()
    {
        _hp = (float)_currentHp / (float)_maxHp;
        HpSlider.value = Mathf.Lerp(HpSlider.value, _hp, Time.deltaTime * _smoothSpeed);
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
    }
}
