using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [SerializeField] private DialogueUI _dialogueUI;

        [Header("Game Start")]
        [Tooltip("게임 시작 시 보여줄 대화 (첫 플레이 시 1회)")]
        [SerializeField] private DialogueData _introDialogue;
        [SerializeField] private string _townSceneName = "TownScene";

        private Action _onComplete;
        private bool _introChecked;
        private DialogueData _pendingDialogue;

        public bool IsDialogueActive { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (Instance == this)
                Instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != _townSceneName) return;

            // 인트로 대화 (게임 시작 시 1회)
            if (!_introChecked)
            {
                _introChecked = true;

                if (_introDialogue != null)
                {
                    ShowDialogue(_introDialogue);
                    return;
                }
            }

            // 예약된 대화 (던전 클리어 후 등)
            if (_pendingDialogue != null)
            {
                var dialogue = _pendingDialogue;
                _pendingDialogue = null;
                ShowDialogue(dialogue);
            }
        }

        /// <summary>
        /// 마을 도착 시 표시할 대화를 예약합니다.
        /// </summary>
        public void QueueDialogueOnTownLoad(DialogueData dialogue)
        {
            _pendingDialogue = dialogue;
        }

        private void OnEnable()
        {
            if (_dialogueUI != null)
            {
                _dialogueUI.OnDialogueComplete += HandleDialogueComplete;
            }
        }

        private void OnDisable()
        {
            if (_dialogueUI != null)
            {
                _dialogueUI.OnDialogueComplete -= HandleDialogueComplete;
            }
        }

        public void ShowDialogue(DialogueData dialogue, Action onComplete = null)
        {
            if (dialogue == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (IsDialogueActive)
            {
                Debug.LogWarning("[DialogueManager] Dialogue already active");
                return;
            }

            _onComplete = onComplete;
            IsDialogueActive = true;
            _dialogueUI.Show();
            _dialogueUI.ShowDialogue(dialogue);
        }

        private void HandleDialogueComplete()
        {
            IsDialogueActive = false;
            _dialogueUI.Hide();

            var callback = _onComplete;
            _onComplete = null;
            callback?.Invoke();
        }
    }
}
