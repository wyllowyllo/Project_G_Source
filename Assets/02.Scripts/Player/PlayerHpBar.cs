using Combat.Core;
using Combat.Damage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpBar : MonoBehaviour
{

    [SerializeField] private Combatant _playerCombatant; 

    public Slider HpSlider;
    public Slider BackSlider;
    [SerializeField] private Image _hpFillImage;
    [SerializeField] private Image _backHpFillImage;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _hpText;

    [SerializeField] private int _level = 1;
    [SerializeField] private float _smoothSpeed = 5f;
    [SerializeField] private float _smoothBackSpeed = 2f; 
    [SerializeField] private float _backHpDelay = 0.3f;
    [SerializeField] private int _firstLevel = 1;

    [SerializeField] private UIExp _uiExp;

    private bool _backHpHit = false;
    private float _targetHp;
    private float _backHpDelayTimer;

    private int _maxLevel = 30;

    private void OnEnable()
    {
        if(_uiExp != null)
        {
            _uiExp.OnLevelUp += HandleLevelUp;
        }
        if (_playerCombatant != null)
        {
            _playerCombatant.OnDamaged += HandleDamaged;
            _playerCombatant.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if(_uiExp != null)
        {
            _uiExp.OnLevelUp -= HandleLevelUp;
        }

        if (_playerCombatant != null)
        {
            _playerCombatant.OnDamaged -= HandleDamaged;
            _playerCombatant.OnDeath -= HandleDeath;
        }
    }

    private void Start()
    {
        _level = _firstLevel;

        if (_playerCombatant != null)
        {
            InitializeHpBar();

            HpSlider.value = 0.5f;
            BackSlider.value = 0.5f;
            Debug.Log($"HpSlider value set to: {HpSlider.value}");
        }
    }

    private void Update()
    {
        if (_playerCombatant == null)
        {
            return;
        }

        UpdateHpBarSmooth();
        UpdateBackSlider();
    }

    private void HandleLevelUp()
    {
        _level++;
        _levelText.text = $"Lv.{_level}";

        if(_level > _maxLevel)
        {
            _level = _maxLevel;
        }
    }

    private void InitializeHpBar()
    {
        _targetHp = _playerCombatant.CurrentHealth / _playerCombatant.MaxHealth;
        HpSlider.value = _targetHp;
        BackSlider.value = _targetHp;
        UpdateHpText();
        UpdateHpColor();
    }

    private void HandleDamaged(DamageInfo damageInfo)
    {
        Debug.Log($"Player took {damageInfo.Amount} damage! (Critical: {damageInfo.IsCritical})");

     
        _targetHp = _playerCombatant.CurrentHealth / _playerCombatant.MaxHealth;

        _backHpDelayTimer = _backHpDelay;
        _backHpHit = true;

        UpdateHpText();
        UpdateHpColor();

        if (damageInfo.IsCritical)
        {
            FlashCritical();
        }
    }

    private void HandleDeath()
    {
        Debug.Log("Player Died!");
        GameOver();
    }

    private void UpdateHpBarSmooth()
    {
        HpSlider.value = Mathf.Lerp(HpSlider.value, _targetHp, Time.deltaTime * _smoothSpeed);
    }

    private void UpdateBackSlider()
    {
        if (_backHpHit)
        {
            _backHpDelayTimer -= Time.deltaTime;

            if (_backHpDelayTimer <= 0)
            {
                BackSlider.value = Mathf.Lerp(BackSlider.value, _targetHp, Time.deltaTime * _smoothBackSpeed);


                if (Mathf.Abs(BackSlider.value - HpSlider.value) < 0.01f)
                {
                    _backHpHit = false;
                    BackSlider.value = HpSlider.value;
                }
            }
        }
    }

    private void UpdateHpText()
    {
        _levelText.text = $"Lv.{_level}";
        _hpText.text = $"{Mathf.CeilToInt(_playerCombatant.CurrentHealth)} / {Mathf.CeilToInt(_playerCombatant.MaxHealth)}";
    }

    private void UpdateHpColor()
    {
        Color newColor;

        if (HpSlider.value > 0.5f)
        {
            newColor = new Color32(0, 191, 5, 255);
        }
        else if (HpSlider.value > 0.3f)
        {
            newColor = new Color(1f, 0.8f, 0f);
        }
        else
        {
            newColor = new Color(1f, 0.3f, 0.3f);
        }

        _hpFillImage.color = newColor;

        if (_backHpFillImage != null)
        {
            _backHpFillImage.color = newColor;
        }
    }

    private void FlashCritical()
    {
        StartCoroutine(FlashCoroutine());
    }

    private System.Collections.IEnumerator FlashCoroutine()
    {
        Color originalColor = _hpFillImage.color;
        _hpFillImage.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        _hpFillImage.color = originalColor;
    }

    private void GameOver()
    {
        GameManager.Instance.TriggerGameOver();
    }

    public void UpdateHealthUI()
    {
        if (_playerCombatant == null) return;

        _targetHp = _playerCombatant.CurrentHealth / _playerCombatant.MaxHealth;
        UpdateHpText();
        UpdateHpColor();

    }
}