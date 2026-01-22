using System;
using UnityEngine;

public class SkillViewerInput : MonoBehaviour
{
    private KeyCode _toggleKey = KeyCode.K;
    public event Action<bool> OnToggleRequested;

    private bool _isOpen;


    private void Update()
    {
        if(Input.GetKeyDown(_toggleKey))
        {
            _isOpen = !_isOpen;

/*            if (_isOpen)
            {
                SoundManager.Instance.PlaySfx(SoundManager.ESfx.SkillUIClick);
            }*/

            OnToggleRequested?.Invoke(_isOpen);
        }
    }
}
