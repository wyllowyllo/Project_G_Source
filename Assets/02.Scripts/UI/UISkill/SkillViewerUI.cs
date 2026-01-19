using UnityEngine;
using UnityEngine.UI;

public class SkillViewerUI : MonoBehaviour
{
    [SerializeField] private GameObject _skillUIPanel;
    [SerializeField] private SkillViewerInput _skillViewerInput;

    private Common.CursorManager _cursorManager;

    private void Awake()
    {
        _skillUIPanel.SetActive(false);

        _cursorManager = Common.CursorManager.Instance;

        if (_cursorManager == null)
        {
            Debug.LogWarning("SkillViewerUI: CursorManager를 찾을 수 없습니다.");
        }
    }

    private void OnEnable()
    {
        _skillViewerInput.OnToggleRequested += HandleToggle;
    }

    private void OnDisable()
    {
        _skillViewerInput.OnToggleRequested -= HandleToggle;
    }

private void HandleToggle(bool isOpen)
    {
        _skillUIPanel.SetActive(isOpen);

        // Skill UI가 열리면 커서 활성화, 닫히면 커서 비활성화
        if (_cursorManager != null)
        {
            if (isOpen)
            {
                _cursorManager.UnlockCursor();
            }
            else
            {
                _cursorManager.LockCursor();
            }
        }
    }


}
