using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Equipment;

public class EquipmentSlotUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _borderImage;  // 등급 색상 표시용 (선택적)

    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _gradeText;
    [SerializeField] private GameObject _emptySlotIndicator;

    [Header("세팅")]
    [SerializeField] private EquipmentSlot _slotType;
    [SerializeField] private Sprite _defaultEmptySprite;
    [SerializeField] private Color _emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Header("등급 색상")]
    [SerializeField] private Color _normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private Color _rareColor = new Color(0.3f, 0.6f, 1f, 1f);
    [SerializeField] private Color _uniqueColor = new Color(0.6f, 0.3f, 1f, 1f);
    [SerializeField] private Color _legendaryColor = new Color(1f, 0.5f, 0.1f, 1f);

    private EquipmentData _currentEquipment;

    private void Start()
    {
    }

public void SetEquipment(EquipmentData equipment)
    {
        _currentEquipment = equipment;

        if (equipment != null)
        {
            // 아이콘 설정
            if (_iconImage != null)
            {
              
                // 장비 아이콘이 있으면 사용, 없으띠 기본 스프라이트
                if (equipment.Icon != null)
                {
                    Debug.Log("bb");
                    _iconImage.sprite = equipment.Icon;
                    _iconImage.color = Color.white;  // 원본 색상 유지
                }
                else
                {
                    // 아이콘이 없는 경우 기본 스프라이트 + 등급 색상
                    _iconImage.sprite = _defaultEmptySprite;
                    _iconImage.color = GetGradeColor(equipment.Grade);
                }
            }

            // 테두리/배경에 등급 색상 적용 (선택적)
            if (_borderImage != null)
            {
                _borderImage.color = GetGradeColor(equipment.Grade);
            }

            // 아이템 이름 표시
            if (_itemNameText != null)
            {
                _itemNameText.text = equipment.EquipmentName;
                _itemNameText.gameObject.SetActive(true);
            }

            // 등급 표시
            if (_gradeText != null)
            {
                _gradeText.text = equipment.Grade.ToString();
                _gradeText.color = GetGradeColor(equipment.Grade);
                _gradeText.gameObject.SetActive(true);
            }

            // 빈 슬롯 인디케이터 숨김
            if (_emptySlotIndicator != null)
            {
                _emptySlotIndicator.SetActive(false);
            }
        }
        else
        {
            SetEmpty();
        }
    }

public void SetEmpty()
    {
        _currentEquipment = null;

        if (_iconImage != null)
        {
            _iconImage.sprite = _defaultEmptySprite;
            _iconImage.color = _emptySlotColor;
        }

        // 테두리 색상도 초기화
        if (_borderImage != null)
        {
            _borderImage.color = _emptySlotColor;
        }

        if (_itemNameText != null)
        {
            _itemNameText.text = "";
            _itemNameText.gameObject.SetActive(false);
        }

        if (_gradeText != null)
        {
            _gradeText.text = "";
            _gradeText.gameObject.SetActive(false);
        }

        if (_emptySlotIndicator != null)
        {
            _emptySlotIndicator.SetActive(true);
        }
    }

    public EquipmentData GetCurrentEquipment()
    {
        return _currentEquipment;
    }

    public EquipmentSlot GetSlotType()
    {
        return _slotType;
    }

    public bool IsEmpty()
    {
        return _currentEquipment == null;
    }

    private Color GetGradeColor(EquipmentGrade grade)
    {
        return grade switch
        {
            EquipmentGrade.Normal => _normalColor,
            EquipmentGrade.Rare => _rareColor,
            EquipmentGrade.Unique => _uniqueColor,
            EquipmentGrade.Legendary => _legendaryColor,
            _ => Color.white
        };
    }
}
