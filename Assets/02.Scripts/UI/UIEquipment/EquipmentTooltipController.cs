using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Equipment
{
    /// <summary>
    /// 장비 아이템 위에 표시되는 툴팁 UI를 관리합니다.
    /// DroppedEquipment와 함께 사용됩니다.
    /// </summary>
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
            
            // Canvas 설정
            if (_tooltipCanvas != null)
            {
                _tooltipCanvas.renderMode = RenderMode.WorldSpace;
                _tooltipCanvas.worldCamera = Camera.main;
            }

            // 초기에는 숨김
            if (_tooltipPanel != null && !_alwaysShow)
            {
                _tooltipPanel.SetActive(false);
            }
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            
            // 플레이어 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }

            // 장비 데이터 표시
            if (_droppedEquipment != null && _droppedEquipment.EquipmentData != null)
            {
                UpdateTooltip(_droppedEquipment.EquipmentData);
            }
        }

        private void LateUpdate()
        {
            // 빌보드 효과 (카메라를 향하도록)
            if (_enableBillboard && _tooltipCanvas != null && _mainCamera != null)
            {
                _tooltipCanvas.transform.LookAt(
                    _tooltipCanvas.transform.position + _mainCamera.transform.rotation * Vector3.forward,
                    _mainCamera.transform.rotation * Vector3.up
                );
            }

            // 거리에 따른 표시/숨김
            if (!_alwaysShow && _playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, _playerTransform.position);
                bool shouldShow = distance <= _showDistance;

                if (_tooltipPanel != null && _tooltipPanel.activeSelf != shouldShow)
                {
                    _tooltipPanel.SetActive(shouldShow);
                }
            }

            // 툴팁 위치 업데이트
            if (_tooltipCanvas != null)
            {
                _tooltipCanvas.transform.position = transform.position + _tooltipOffset;
            }
        }

        /// <summary>
        /// 장비 데이터로 툴팁 업데이트
        /// </summary>
        public void UpdateTooltip(EquipmentData equipmentData)
        {
            if (equipmentData == null)
            {
                Debug.LogWarning("[EquipmentTooltip] EquipmentData is null!");
                return;
            }

            // 등급 텍스트
            if (_gradeText != null)
            {
                string gradeKorean = GetGradeKorean(equipmentData.Grade);
                _gradeText.text = $"등급: {gradeKorean}";
                _gradeText.color = GetGradeColor(equipmentData.Grade);
            }

            // 아이템 이름
            if (_itemNameText != null)
            {
                _itemNameText.text = $"장비이름: {equipmentData.EquipmentName}";
            }

            // 공격력
            if (_attackText != null)
            {
                _attackText.text = $"공격력: {equipmentData.AttackBonus:F0}";
            }

            // 방어력
            if (_defenseText != null)
            {
                _defenseText.text = $"방어력: {equipmentData.DefenseBonus:F0}";
            }

            // 배경 색상
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

        /// <summary>
        /// 등급을 한글로 변환
        /// </summary>
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

        /// <summary>
        /// 등급에 따른 색상 반환
        /// </summary>
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

        /// <summary>
        /// 등급에 따른 배경 색상 반환
        /// </summary>
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

        /// <summary>
        /// 툴팁 강제 표시
        /// </summary>
        public void ShowTooltip()
        {
            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 툴팁 강제 숨김
        /// </summary>
        public void HideTooltip()
        {
            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 툴팁 표시 거리 설정
        /// </summary>
        public void SetShowDistance(float distance)
        {
            _showDistance = distance;
        }

        /// <summary>
        /// 툴팁 오프셋 설정
        /// </summary>
        public void SetTooltipOffset(Vector3 offset)
        {
            _tooltipOffset = offset;
        }

        private void OnDrawGizmosSelected()
        {
            // 표시 거리 시각화
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _showDistance);

            // 툴팁 위치 시각화
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + _tooltipOffset, 0.1f);
        }
    }
}
