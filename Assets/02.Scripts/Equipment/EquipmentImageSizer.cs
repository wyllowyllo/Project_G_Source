using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 장비 슬롯의 Icon 이미지 크기를 동적으로 조정하는 컴포넌트
/// - 빈 슬롯일 때: 100x100 (기본 아이콘)
/// - 장비 착용 시: 350x350 (장비 이미지)
/// EquipmentSlotUI의 Icon 오브젝트에 붙이면 자동으로 작동합니다.
/// </summary>
[RequireComponent(typeof(Image))]
public class EquipmentImageSizer : MonoBehaviour
{
    [Header("크기 설정")]
    [SerializeField] private Vector2 _emptySizeSlot = new Vector2(100f, 100f); // 빈 슬롯 크기
    [SerializeField] private Vector2 _equippedSize = new Vector2(350f, 350f);   // 장비 착용 시 크기

    [Header("빈 슬롯 판별 (선택사항)")]
    [Tooltip("빈 슬롯 아이콘을 지정하면 해당 스프라이트일 때 작은 크기로 유지합니다")]
    [SerializeField] private Sprite _emptySlotIcon;

    [Header("옵션")]
    [SerializeField] private bool _preserveAspect = true;

    private Image _image;
    private RectTransform _rectTransform;
    private Sprite _lastSprite;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
        CheckAndApplySize();
    }

    private void LateUpdate()
    {
        // 스프라이트가 변경되었는지 체크
        if (_image != null && _image.sprite != _lastSprite)
        {
            _lastSprite = _image.sprite;
            CheckAndApplySize();
        }
    }

    private void Initialize()
    {
        if (_image == null)
        {
            _image = GetComponent<Image>();
        }

        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        if (_image != null)
        {
            _lastSprite = _image.sprite;
        }
    }

    /// <summary>
    /// 현재 스프라이트 상태를 확인하고 적절한 크기를 적용합니다.
    /// </summary>
    private void CheckAndApplySize()
    {
        if (_rectTransform == null || _image == null)
        {
            Initialize();
            return;
        }

        Vector2 targetSize;

        // 빈 슬롯인지 판별
        if (IsEmptySlot())
        {
            targetSize = _emptySizeSlot;
        }
        else
        {
            targetSize = _equippedSize;
        }

        // 크기 적용
        _rectTransform.sizeDelta = targetSize;
        _image.preserveAspect = _preserveAspect;
    }

    /// <summary>
    /// 빈 슬롯인지 확인합니다.
    /// </summary>
    private bool IsEmptySlot()
    {
        // 스프라이트가 없으면 빈 슬롯
        if (_image.sprite == null)
        {
            return true;
        }

        // 빈 슬롯 아이콘이 지정되어 있고, 현재 스프라이트가 그것과 같으면 빈 슬롯
        if (_emptySlotIcon != null && _image.sprite == _emptySlotIcon)
        {
            return true;
        }

        // 이미지가 비활성화되어 있으면 빈 슬롯
        if (!_image.enabled)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 외부에서 크기를 수동으로 변경할 때 사용
    /// </summary>
    public void SetSize(Vector2 emptySize, Vector2 equippedSize)
    {
        _emptySizeSlot = emptySize;
        _equippedSize = equippedSize;
        CheckAndApplySize();
    }

    /// <summary>
    /// 즉시 크기를 재적용합니다.
    /// </summary>
    public void RefreshSize()
    {
        CheckAndApplySize();
    }
}