using System;
using System.Collections;
using Core;
using Dialogue;
using Monster.Manager;
using UnityEngine;

namespace Dungeon
{
    public class DungeonClearHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _autoReturnDelay = 10f;
        [SerializeField] private float _inputAcceptDelay = 1f;

        public event Action OnClearSequenceStarted;
        public event Action OnInputAccepted;
        public event Action OnReturnToTown;

        private DungeonManager _dungeonManager;
        private MonsterTracker _monsterTracker;
        private Coroutine _returnCoroutine;
        private bool _canAcceptInput;

        private void OnEnable()
        {
            _dungeonManager = DungeonManager.Instance;
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonCleared += HandleDungeonCleared;
                Debug.Log("[DungeonClearHandler] DungeonManager 이벤트 구독 완료");
            }
            else
            {
                Debug.LogWarning("[DungeonClearHandler] OnEnable: DungeonManager.Instance가 null입니다");
            }

            _monsterTracker = MonsterTracker.Instance;
            if (_monsterTracker != null)
            {
                _monsterTracker.OnAllMonstersDefeated.AddListener(HandleAllMonstersDefeated);
                Debug.Log("[DungeonClearHandler] MonsterTracker 이벤트 구독 완료");
            }
        }

        private void OnDisable()
        {
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonCleared -= HandleDungeonCleared;
            }

            if (_monsterTracker != null)
            {
                _monsterTracker.OnAllMonstersDefeated.RemoveListener(HandleAllMonstersDefeated);
            }

            StopReturnCoroutine();
        }

        private void Start()
        {
            // OnEnable 시점에 Instance가 없었을 경우 재시도
            if (_monsterTracker == null)
            {
                _monsterTracker = MonsterTracker.Instance;
                if (_monsterTracker != null)
                {
                    _monsterTracker.OnAllMonstersDefeated.AddListener(HandleAllMonstersDefeated);
                }
            }

            if (_dungeonManager == null)
            {
                _dungeonManager = DungeonManager.Instance;
                if (_dungeonManager != null)
                {
                    _dungeonManager.DungeonCleared += HandleDungeonCleared;
                }
            }
        }

        private void HandleAllMonstersDefeated()
        {
            _dungeonManager?.CompleteDungeon();
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

        private void HandleDungeonCleared(int xpReward)
        {
            Debug.Log($"[DungeonClearHandler] HandleDungeonCleared 호출됨! XP: {xpReward}");
            StopReturnCoroutine();
            _returnCoroutine = StartCoroutine(ClearSequence());
        }

        private IEnumerator ClearSequence()
        {
            _canAcceptInput = false;
            OnClearSequenceStarted?.Invoke();

            // 클리어 대화를 마을 도착 시 표시하도록 예약
            var clearDialogue = _dungeonManager?.CurrentDungeon?.ClearDialogue;
            if (clearDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.QueueDialogueOnTownLoad(clearDialogue);
            }

            yield return new WaitForSeconds(_inputAcceptDelay);
            _canAcceptInput = true;
            OnInputAccepted?.Invoke();

            yield return new WaitForSeconds(_autoReturnDelay - _inputAcceptDelay);
            ReturnToTown();
        }

        private void ReturnToTown()
        {
            Debug.Log("[DungeonClearHandler] ReturnToTown 호출됨");
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
