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
            TrySubscribe();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Unsubscribe();
            StopReturnCoroutine();
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
                Debug.Log("[DungeonClearHandler] DungeonManager 이벤트 구독 완료");
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
                Debug.Log("[DungeonClearHandler] MonsterTracker 이벤트 구독 완료");
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
            Debug.Log("[DungeonClearHandler] HandleAllMonstersDefeated 호출됨!");
            
            if (_dungeonManager == null)
            {
                Debug.LogError("[DungeonClearHandler] _dungeonManager가 null입니다!");
                return;
            }
            
            Debug.Log("[DungeonClearHandler] DungeonManager.CompleteDungeon() 호출");
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
            Debug.Log($"[DungeonClearHandler] HandleDungeonCleared 호출됨! XP: {xpReward}");
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
            Debug.Log("[DungeonClearHandler] ReturnToTown 호출됨");
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
            Debug.Log($"[DungeonClearHandler] 생존 몬스터 수: {aliveMonsters.Count}");
            
            foreach (var monster in aliveMonsters)
            {
                if (monster != null && monster.gameObject != null)
                {
                    Debug.Log($"[DungeonClearHandler] 몬스터 제거: {monster.name}");
                    Destroy(monster.gameObject);
                }
            }
            
            _monsterTracker.CleanupDestroyedMonsters();
            
            Debug.Log("[DungeonClearHandler] 모든 몬스터 제거 완료");
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
