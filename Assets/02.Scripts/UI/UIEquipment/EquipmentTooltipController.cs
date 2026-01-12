using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Equipment
{
    [RequireComponent(typeof(DroppedEquipment))]
    public class EquipmentTooltipController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas _tooltipCanvas;
        [SerializeField] private GameObject _tooltipPanel;
        [SerializeField] private TextMeshProUGUI _gradeText;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _attackText;
        [SerializeField] private TextMeshProUGUI _defenseText;
        [SerializeField] private Image _backgroundImage;

        [Header("Billboard Settings")]
        [SerializeField] private bool _enableBillboard = true;
        [SerializeField] private Vector3 _tooltipOffset = new Vector3(0, 1.5f, 0);

        [Header("Display Settings")]
        [SerializeField] private float _showDistance = 5f;
        [SerializeField] private bool _alwaysShow = false;

        [Header("Grade Colors")]
        [SerializeField] private Color _normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        [SerializeField] private Color _rareColor = new Color(0.3f, 0.6f, 1f, 1f);
        [SerializeField] private Color _uniqueColor = new Color(0.6f, 0.3f, 1f, 1f);
        [SerializeField] private Color _legendaryColor = new Color(1f, 0.5f, 0.1f, 1f);

        [Header("Background Colors")]
        [SerializeField] private Color _normalBgColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        [SerializeField] private Color _rareBgColor = new Color(0.1f, 0.2f, 0.3f, 0.9f);
        [SerializeField] private Color _uniqueBgColor = new Color(0.2f, 0.1f, 0.3f, 0.9f);
        [SerializeField] private Color _legendaryBgColor = new Color(0.3f, 0.2f, 0.1f, 0.9f);

        private DroppedEquipment _droppedEquipment;
        private Camera _mainCamera;
        private Transform _playerTransform;

        private void Awake()
        {
            _droppedEquipment = GetComponent<DroppedEquipment>();
            
            if (_tooltipCanvas != null)
            {
                _tooltipCanvas.renderMode = RenderMode.WorldSpace;
                _tooltipCanvas.worldCamera = Camera.main;
            }

            if (_tooltipPanel != null && !_alwaysShow)
            {
                _tooltipPanel.SetActive(false);
            }
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }

            if (_droppedEquipment != null && _droppedEquipment.EquipmentData != null)
            {
                UpdateTooltip(_droppedEquipment.EquipmentData);
            }
        }

        private void LateUpdate()
        {
            if (_enableBillboard && _tooltipCanvas != null && _mainCamera != null)
            {
                _tooltipCanvas.transform.LookAt(
                    _tooltipCanvas.transform.position + _mainCamera.transform.rotation * Vector3.forward,
                    _mainCamera.transform.rotation * Vector3.up
                );
            }

            if (!_alwaysShow && _playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, _playerTransform.position);
                bool shouldShow = distance <= _showDistance;

                if (_tooltipPanel != null && _tooltipPanel.activeSelf != shouldShow)
                {
                    _tooltipPanel.SetActive(shouldShow);
                }
            }

            if (_tooltipCanvas != null)
            {
                _tooltipCanvas.transform.position = transform.position + _tooltipOffset;
            }
        }

        public void UpdateTooltip(EquipmentData equipmentData)
        {
            if (equipmentData == null)
            {
                Debug.Log("[EquipmentTooltip] EquipmentData is null");
                return;
            }

            if (_gradeText != null)
            {
                string gradeKorean = GetGradeKorean(equipmentData.Grade);
                _gradeText.text = $"등급: {gradeKorean}";
                _gradeText.color = GetGradeColor(equipmentData.Grade);
            }

            if (_itemNameText != null)
            {
                _itemNameText.text = $"장비이름: {equipmentData.EquipmentName}";
            }

            if (_attackText != null)
            {
                _attackText.text = $"공격력: {equipmentData.AttackBonus:F0}";
            }

            if (_defenseText != null)
            {
                _defenseText.text = $"방어력: {equipmentData.DefenseBonus:F0}";
            }

            if (_backgroundImage != null)
            {
                _backgroundImage.color = GetBackgroundColor(equipmentData.Grade);
            }

            // 아이템 아이콘 (TODO: EquipmentData에 Sprite 필드 추가 시 구현)
            if (_itemIcon != null)
            {
                _itemIcon.color = GetGradeColor(equipmentData.Grade);
            }
        }

        private string GetGradeKorean(EquipmentGrade grade)
        {
            return grade switch
            {
                EquipmentGrade.Normal => "노말",
                EquipmentGrade.Rare => "에픽",
                EquipmentGrade.Unique => "유니크",
                EquipmentGrade.Legendary => "레전더리",
                _ => "알 수 없음"
            };
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

        private Color GetBackgroundColor(EquipmentGrade grade)
        {
            return grade switch
            {
                EquipmentGrade.Normal => _normalBgColor,
                EquipmentGrade.Rare => _rareBgColor,
                EquipmentGrade.Unique => _uniqueBgColor,
                EquipmentGrade.Legendary => _legendaryBgColor,
                _ => Color.gray
            };
        }

        public void ShowTooltip()
        {
            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(true);
            }
        }

        public void HideTooltip()
        {
            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(false);
            }
        }

        public void SetShowDistance(float distance)
        {
            _showDistance = distance;
        }

        public void SetTooltipOffset(Vector3 offset)
        {
            _tooltipOffset = offset;
        }

    }
}
