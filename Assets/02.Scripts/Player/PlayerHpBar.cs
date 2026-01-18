using Combat.Core;
using Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class PlayerHpBar : MonoBehaviour, ICloneDisableable
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

        [Header("Hp Color")]
        [SerializeField] private Color32 _highHpColor = new Color32(0, 191, 5, 255);
        [SerializeField] private Color32 _mediumHpColor = new Color32(255, 204, 0, 255);
        [SerializeField] private Color32 _lowHpColor = new Color32(255, 77, 77, 255);

        [SerializeField] private PlayerProgression _playerProgression;

        private bool _backHpHit = false;
        private float _targetHp;
        private float _backHpDelayTimer;

        private const int MaxLevel = 30;

        private void OnEnable()
        {
            if(_playerProgression != null)
            {
                _playerProgression.OnLevelUp += HandleLevelUp;
            }
            if (_playerCombatant != null)
            {
                _playerCombatant.OnDamaged += HandleDamaged;
                _playerCombatant.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_playerProgression != null)
            {
                _playerProgression.OnLevelUp -= HandleLevelUp;
            }

            if (_playerCombatant != null)
            {
                _playerCombatant.OnDamaged -= HandleDamaged;
                _playerCombatant.OnDeath -= HandleDeath;
            }
        }

        private void Start()
        {
            if (!ValidateDependencies())
            {
                enabled = false;
                return;
            }

            _level = _firstLevel;
            InitializeHpBar();
        }

        private bool ValidateDependencies()
        {
            if (_playerCombatant == null || HpSlider == null || BackSlider == null)
            {
                return false;
            }
            return true;
        }

        private void Update()
        {
            UpdateHpBarSmooth();
            UpdateBackSlider();
        }
        public void OnCloneDisable()
        {
            // UI 요소들 숨기기, 이벤트 구독 해제
            if (_playerProgression != null)
            {
                _playerProgression.OnLevelUp -= HandleLevelUp;
            }
            if (_playerCombatant != null)
            {
                _playerCombatant.OnDamaged -= HandleDamaged;
                _playerCombatant.OnDeath -= HandleDeath;
            }
        }

        private void HandleLevelUp(int previousLevel, int newLevel)
        {
            _level = newLevel;

            if (_level > MaxLevel)
            {
                _level = MaxLevel;
            }

            _levelText.text = $"Lv.{_level}";
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
            else
            {
                BackSlider.value = HpSlider.value;
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
                newColor = _highHpColor;
            }
            else if (HpSlider.value > 0.3f)
            {
                newColor = _mediumHpColor;
            }
            else
            {
                newColor = _lowHpColor;
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
}