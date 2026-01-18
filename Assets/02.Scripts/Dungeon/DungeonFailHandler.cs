using System;
using System.Collections;
using Core;
using UnityEngine;

namespace Dungeon
{
    public class DungeonFailHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _autoReturnDelay = 10f;
        [SerializeField] private float _inputAcceptDelay = 1f;

        public event Action OnFailSequenceStarted;
        public event Action OnInputAccepted;
        public event Action OnReturnToTown;

        private DungeonManager _dungeonManager;
        private Coroutine _returnCoroutine;
        private bool _canAcceptInput;

        private void OnEnable()
        {
            _dungeonManager = DungeonManager.Instance;
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonFailed += HandleDungeonFailed;
            }
        }

        private void OnDisable()
        {
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonFailed -= HandleDungeonFailed;
            }
            StopReturnCoroutine();
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
            StopReturnCoroutine();
            _returnCoroutine = StartCoroutine(FailSequence());
        }

        private IEnumerator FailSequence()
        {
            _canAcceptInput = false;
            OnFailSequenceStarted?.Invoke();

            yield return new WaitForSeconds(_inputAcceptDelay);
            _canAcceptInput = true;
            OnInputAccepted?.Invoke();

            yield return new WaitForSeconds(_autoReturnDelay - _inputAcceptDelay);
            ReturnToTown();
        }

        private void ReturnToTown()
        {
            StopReturnCoroutine();
            _canAcceptInput = false;
            OnReturnToTown?.Invoke();
            _dungeonManager?.ReturnToTown();
        }

        private void StopReturnCoroutine()
        {
            if (_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
                _returnCoroutine = null;
            }
        }
    }
}
