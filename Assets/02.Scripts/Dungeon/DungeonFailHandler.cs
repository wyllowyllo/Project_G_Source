using System;
using System.Collections;
using Core;
using UnityEngine;

namespace Dungeon
{
    public class DungeonFailHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _inputAcceptDelay = 1f;

        [Header("UI Reference")]
        [SerializeField] private DungeonFailUI _dungeonFailUI;

        public event Action OnFailSequenceStarted;
        public event Action OnInputAccepted;
        public event Action OnReturnToTown;

        private DungeonManager _dungeonManager;
        private bool _canAcceptInput;
        private bool _animationCompleted;

        private void OnEnable()
        {
            if (_dungeonFailUI == null)
            {
                _dungeonFailUI = FindObjectOfType<DungeonFailUI>(true);
            }

            _dungeonManager = DungeonManager.Instance;
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonFailed += HandleDungeonFailed;
            }

            if (_dungeonFailUI != null)
            {
                _dungeonFailUI.OnAnimationComplete += HandleAnimationComplete;
            }
            else
            {
                Debug.LogWarning($"[{nameof(DungeonFailHandler)}] DungeonFailUI not found in current scene!");
            }
        }

        private void OnDisable()
        {
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonFailed -= HandleDungeonFailed;
            }

            if (_dungeonFailUI != null)
            {
                _dungeonFailUI.OnAnimationComplete -= HandleAnimationComplete;
            }
        }

        private void Update()
        {
            if (!_canAcceptInput) return;

            if (Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ReturnToTown();
            }
        }

        private void HandleDungeonFailed()
        {
            _animationCompleted = false;
            StartCoroutine(FailSequence());
        }

        private IEnumerator FailSequence()
        {
            _canAcceptInput = false;
            OnFailSequenceStarted?.Invoke();

            yield return new WaitForSeconds(_inputAcceptDelay);
            _canAcceptInput = true;
            OnInputAccepted?.Invoke();

            yield return new WaitUntil(() => _animationCompleted);
            ReturnToTown();
        }

        private void HandleAnimationComplete()
        {
            _animationCompleted = true;
        }

        private void ReturnToTown()
        {
            _canAcceptInput = false;
            OnReturnToTown?.Invoke();
            _dungeonManager?.ReturnToTown();
        }
    }
}
