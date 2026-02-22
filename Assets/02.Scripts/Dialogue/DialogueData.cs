using Monster.Feedback.Data;
using UnityEngine;

namespace Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "ProjectG/Dialogue")]
    public class DialogueData : ScriptableObject
    {
        [SerializeField] private DialogueLine[] _lines;

        public DialogueLine[] Lines => _lines;
        public int LineCount => _lines?.Length ?? 0;
    }

    [System.Serializable]
    public class DialogueLine
    {
        [SerializeField] private string _speakerName;
        [SerializeField] private Sprite _portrait;
        [SerializeField, TextArea(2, 5)] private string _text;

        [Header("Effects")]
        [SerializeField] private CameraShakeConfig _cameraShake;

        public string SpeakerName => _speakerName;
        public Sprite Portrait => _portrait;
        public string Text => _text;
        public CameraShakeConfig CameraShake => _cameraShake;
    }
}
