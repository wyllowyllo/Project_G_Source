using System;
using System.Collections;
using Dialogue;
using Monster.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            StopReturnCoroutine();
            Unsubscribe();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TrySubscribe();
        }

        private void TrySubscribe()
        {
            var newDungeonManager = DungeonManager.Instance;
            if (newDungeonManager != null && newDungeonManager != _dungeonManager)
            {
                if (_dungeonManager != null)
                {
                    _dungeonManager.DungeonCleared -= HandleDungeonCleared;
                }
                _dungeonManager = newDungeonManager;
                _dungeonManager.DungeonCleared += HandleDungeonCleared;
            }

            var newMonsterTracker = MonsterTracker.Instance;
            if (newMonsterTracker != null && newMonsterTracker != _monsterTracker)
            {
                if (_monsterTracker != null)
                {
                    _monsterTracker.OnAllMonstersDefeated.RemoveListener(HandleAllMonstersDefeated);
                }
                _monsterTracker = newMonsterTracker;
                _monsterTracker.OnAllMonstersDefeated.AddListener(HandleAllMonstersDefeated);
            }
        }

        private void Unsubscribe()
        {
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonCleared -= HandleDungeonCleared;
            }

            if (_monsterTracker != null)
            {
                _monsterTracker.OnAllMonstersDefeated.RemoveListener(HandleAllMonstersDefeated);
            }
        }

        public void HandleAllMonstersDefeated()
        {
            if (_dungeonManager == null)
            {
                Debug.LogError("[DungeonClearHandler] _dungeonManager가 null입니다!");
                return;
            }

            _dungeonManager.CompleteDungeon();
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
            StopReturnCoroutine();
            _returnCoroutine = StartCoroutine(ClearSequence());
        }

        private IEnumerator ClearSequence()
        {
            _canAcceptInput = false;
            OnClearSequenceStarted?.Invoke();
            
            var clearDialogue = _dungeonManager?.GetCurrentDungeonClearDialogue();
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
            StopReturnCoroutine();
            _canAcceptInput = false;
            OnReturnToTown?.Invoke();
            _dungeonManager?.ReturnToTown();
        }

        

        private void KillAllMonsters()
        {
            if (_monsterTracker == null)
            {
                Debug.LogError("[DungeonClearHandler] MonsterTracker가 null입니다!");
                return;
            }

            var aliveMonsters = _monsterTracker.GetAliveMonsters();
            foreach (var monster in aliveMonsters)
            {
                if (monster != null && monster.gameObject != null)
                {
                    Destroy(monster.gameObject);
                }
            }

            _monsterTracker.CleanupDestroyedMonsters();
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
