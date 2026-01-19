using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
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
        [SerializeField] private bool _alwaysShow = false;
        [SerializeField] private bool _clampToScreen = true;
        [SerializeField] private float _screenPadding = 50f;

        [Header("Rendering")]
        [SerializeField] private bool _alwaysOnTop = true;
        [SerializeField] private string _sortingLayerName = "UI";
        [SerializeField] private int _sortingOrder = 100;

        private static readonly Dictionary<Material, Material> _sharedMaterials = new();
        private static readonly Dictionary<Material, int> _materialRefCount = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticFields()
        {
            CleanupAllMaterials();
        }

        static EquipmentTooltipController()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            CleanupAllMaterials();
        }

        private static void CleanupAllMaterials()
        {
            foreach (var kvp in _sharedMaterials)
            {
                if (kvp.Value != null)
                {
                    if (Application.isPlaying)
                        Object.Destroy(kvp.Value);
                    else
                        Object.DestroyImmediate(kvp.Value);
                }
            }
            _sharedMaterials.Clear();
            _materialRefCount.Clear();
        }

        private DroppedEquipment _droppedEquipment;
        private Camera _mainCamera;
        private RectTransform _tooltipRect;
        private EquipmentGradeSettings _gradeSettings;
        private readonly List<Material> _usedOriginalMaterials = new();

        private void Awake()
        {
            _droppedEquipment = GetComponent<DroppedEquipment>();
            _mainCamera = Camera.main;
            _gradeSettings = EquipmentGradeSettings.Instance;

            if (_tooltipCanvas != null)
            {
                _tooltipCanvas.renderMode = RenderMode.WorldSpace;
                _tooltipCanvas.worldCamera = _mainCamera;
                _tooltipCanvas.sortingLayerName = _sortingLayerName;
                _tooltipCanvas.sortingOrder = _sortingOrder;

                if (_alwaysOnTop)
                {
                    ApplyAlwaysOnTop();
                }
            }

            if (_tooltipPanel != null)
            {
                _tooltipRect = _tooltipPanel.GetComponent<RectTransform>();
                if (!_alwaysShow)
                {
                    _tooltipPanel.SetActive(false);
                }
            }
        }

        private void ApplyAlwaysOnTop()
        {
            var graphics = _tooltipCanvas.GetComponentsInChildren<Graphic>(true);
            foreach (var graphic in graphics)
            {
                if (graphic is TextMeshProUGUI tmp)
                {
                    var originalMat = tmp.fontMaterial;
                    var sharedMat = GetOrCreateSharedMaterial(originalMat, true);
                    tmp.fontMaterial = sharedMat;
                    _usedOriginalMaterials.Add(originalMat);
                }
                else
                {
                    var originalMat = graphic.materialForRendering;
                    var sharedMat = GetOrCreateSharedMaterial(originalMat, false);
                    graphic.material = sharedMat;
                    _usedOriginalMaterials.Add(originalMat);
                }
            }
        }

        private static Material GetOrCreateSharedMaterial(Material original, bool isTMP)
        {
            if (_sharedMaterials.TryGetValue(original, out var existing))
            {
                _materialRefCount[original]++;
                return existing;
            }

            var newMat = new Material(original);
            if (isTMP)
            {
                newMat.SetInt(ShaderUtilities.ShaderTag_ZTestMode, (int)CompareFunction.Always);
            }
            else
            {
                newMat.SetInt("unity_GUIZTestMode", (int)CompareFunction.Always);
            }

            _sharedMaterials[original] = newMat;
            _materialRefCount[original] = 1;
            return newMat;
        }

        private void OnDestroy()
        {
            foreach (var originalMat in _usedOriginalMaterials)
            {
                if (originalMat == null) continue;
                if (!_materialRefCount.ContainsKey(originalMat)) continue;

                _materialRefCount[originalMat]--;
                if (_materialRefCount[originalMat] <= 0)
                {
                    if (_sharedMaterials.TryGetValue(originalMat, out var sharedMat))
                    {
                        if (sharedMat != null) Destroy(sharedMat);
                        _sharedMaterials.Remove(originalMat);
                    }
                    _materialRefCount.Remove(originalMat);
                }
            }
            _usedOriginalMaterials.Clear();
        }

        private void Start()
        {
            if (_droppedEquipment != null && _droppedEquipment.EquipmentData != null)
            {
                UpdateTooltip(_droppedEquipment.EquipmentData);
            }
        }

        private void LateUpdate()
        {
            if (_tooltipCanvas == null || _mainCamera == null) return;
            if (_tooltipPanel != null && !_tooltipPanel.activeSelf) return;

            Vector3 worldPosition = transform.position + _tooltipOffset;

            if (_clampToScreen)
            {
                worldPosition = ClampToScreen(worldPosition);
            }

            _tooltipCanvas.transform.position = worldPosition;

            if (_enableBillboard)
            {
                _tooltipCanvas.transform.LookAt(
                    _tooltipCanvas.transform.position + _mainCamera.transform.rotation * Vector3.forward,
                    _mainCamera.transform.rotation * Vector3.up
                );
            }
        }

        private Vector3 ClampToScreen(Vector3 worldPosition)
        {
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPosition);

            if (screenPos.z < 0) return worldPosition;

            Vector3[] corners = new Vector3[4];
            if (_tooltipRect != null)
            {
                _tooltipRect.GetWorldCorners(corners);
                Vector3 minCorner = _mainCamera.WorldToScreenPoint(corners[0]);
                Vector3 maxCorner = _mainCamera.WorldToScreenPoint(corners[2]);
                float tooltipWidth = Mathf.Abs(maxCorner.x - minCorner.x);
                float tooltipHeight = Mathf.Abs(maxCorner.y - minCorner.y);

                float minX = _screenPadding + tooltipWidth * 0.5f;
                float maxX = Screen.width - _screenPadding - tooltipWidth * 0.5f;
                float minY = _screenPadding + tooltipHeight * 0.5f;
                float maxY = Screen.height - _screenPadding - tooltipHeight * 0.5f;

                if (minX < maxX) screenPos.x = Mathf.Clamp(screenPos.x, minX, maxX);
                if (minY < maxY) screenPos.y = Mathf.Clamp(screenPos.y, minY, maxY);
            }
            else
            {
                screenPos.x = Mathf.Clamp(screenPos.x, _screenPadding, Screen.width - _screenPadding);
                screenPos.y = Mathf.Clamp(screenPos.y, _screenPadding, Screen.height - _screenPadding);
            }

            return _mainCamera.ScreenToWorldPoint(screenPos);
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
            if (_gradeSettings == null) return Color.white;
            return _gradeSettings.GetTextColor(grade);
        }

        private Color GetBackgroundColor(EquipmentGrade grade)
        {
            if (_gradeSettings == null) return Color.gray;
            return _gradeSettings.GetBackgroundColor(grade);
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
    }
}
