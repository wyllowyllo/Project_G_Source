using UnityEngine;
using UnityEngine.UI;

public class SkillViewerUI : MonoBehaviour
{
    [SerializeField] private GameObject _skillUIPanel;
    [SerializeField] private SkillViewerInput _skillViewerInput;

    private void Awake()
    {
        _skillUIPanel.SetActive(false);
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
        if (Common.CursorManager.Instance != null)
        {
            if (isOpen)
            {
                Common.CursorManager.Instance.UnlockCursor();
            }
            else
            {
                Common.CursorManager.Instance.LockCursor();
            }
        }
    }


}
