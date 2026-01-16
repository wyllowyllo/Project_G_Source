using System;
using System.Collections;
using System.Text;
using Monster.Feedback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _portraitImage;
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;

        [Header("Settings")]
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] private float _typeSpeed = 0.03f;

        private DialogueData _currentDialogue;
        private int _currentLineIndex;
        private Coroutine _typewriterCoroutine;
        private bool _isTyping;
        private bool _isActive;

        public event Action OnDialogueComplete;

        private void Awake()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_isActive) return;

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                OnClick();
            }
        }

        public void ShowDialogue(DialogueData dialogue)
        {
            if (dialogue == null || dialogue.LineCount == 0)
            {
                Debug.LogWarning("[DialogueUI] Empty dialogue data");
                OnDialogueComplete?.Invoke();
                return;
            }

            _currentDialogue = dialogue;
            _currentLineIndex = 0;
            _isActive = true;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }

            StartCoroutine(FadeIn());
            DisplayCurrentLine();
        }

        private void OnClick()
        {
            if (_isTyping)
            {
                CompleteTyping();
            }
            else
            {
                NextLine();
            }
        }

        private void DisplayCurrentLine()
        {
            if (_currentDialogue == null) return;

            var line = _currentDialogue.Lines[_currentLineIndex];

            _speakerNameText.text = line.SpeakerName;

            if (_portraitImage != null)
            {
                if (line.Portrait != null)
                {
                    _portraitImage.sprite = line.Portrait;
                    _portraitImage.gameObject.SetActive(true);
                }
                else
                {
                    _portraitImage.gameObject.SetActive(false);
                }
            }

            // 지속 카메라 쉐이크
            if (CameraShakeController.Instance != null)
            {
                CameraShakeController.Instance.StartContinuousShake(line.CameraShake);
            }

            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
            }
            _typewriterCoroutine = StartCoroutine(TypeText(line.Text));
        }

        private IEnumerator TypeText(string text)
        {
            _isTyping = true;
            var builder = new StringBuilder(text.Length);
            _dialogueText.text = "";

            foreach (char c in text)
            {
                builder.Append(c);
                _dialogueText.text = builder.ToString();
                yield return new WaitForSeconds(_typeSpeed);
            }

            _isTyping = false;
            _typewriterCoroutine = null;
        }

        private void CompleteTyping()
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
                _typewriterCoroutine = null;
            }

            var line = _currentDialogue.Lines[_currentLineIndex];
            _dialogueText.text = line.Text;
            _isTyping = false;
        }

        private void NextLine()
        {
            _currentLineIndex++;

            if (_currentLineIndex >= _currentDialogue.LineCount)
            {
                EndDialogue();
            }
            else
            {
                DisplayCurrentLine();
            }
        }

        private void EndDialogue()
        {
            _isActive = false;

            if (CameraShakeController.Instance != null)
            {
                CameraShakeController.Instance.StopContinuousShake();
            }

            StartCoroutine(FadeOutAndComplete());
        }

        private IEnumerator FadeIn()
        {
            _canvasGroup.blocksRaycasts = true;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOutAndComplete()
        {
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / _fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _currentDialogue = null;

            OnDialogueComplete?.Invoke();
        }
    }
}
