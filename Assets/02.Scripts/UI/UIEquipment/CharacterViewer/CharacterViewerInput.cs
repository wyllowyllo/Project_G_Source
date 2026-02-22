using UnityEngine;
using System;

public class CharacterViewerInput : MonoBehaviour
{
    [Header("입력 설정")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.Tab;

    // 입력 이벤트
    public event Action OnToggleRequested;
    public event Action<float> OnRotationInput;

    private bool _isActive = false;

    public void SetActive(bool active)
    {
        _isActive = active;
    }

    private void Update()
    {
        // Toggle 입력 감지
        if (Input.GetKeyDown(_toggleKey))
        {
/*            if (!_isActive)
            {
                SoundManager.Instance.PlayUISfx(SoundManager.EUISfx.EquipmentUIOpen);
            }*/
            OnToggleRequested?.Invoke();
        }

        // Viewer가 활성화된 경우에만 카메라 입력 처리
        if (_isActive)
        {
            HandleRotationInput();
        }
    }

    private void HandleRotationInput()
    {
        if (Input.GetMouseButton(1))
        {
            float rotationInput = Input.GetAxis("Mouse X");
            if (rotationInput != 0f)
            {
                OnRotationInput?.Invoke(rotationInput);
            }
        }
    }

    public KeyCode ToggleKey
    {
        get => _toggleKey;
        set => _toggleKey = value;
    }

    public void OnCloneDisable()
    {
        OnToggleRequested = null;
        OnRotationInput = null;
    }
}
