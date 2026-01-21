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
            if (_monsterTracker == null)
            {
                _monsterTracker = MonsterTracker.Instance;
            }
            
            if (_dungeonManager == null)
            {
                _dungeonManager = DungeonManager.Instance;
            }
            
            // MonsterTracker UnityEvent를 코드로 자동 연결
            if (_monsterTracker != null)
            {
                // 기존 리스너가 없는 경우에만 추가
                _monsterTracker.OnAllMonstersDefeated.RemoveListener(HandleAllMonstersDefeated);
                _monsterTracker.OnAllMonstersDefeated.AddListener(HandleAllMonstersDefeated);
                Debug.Log("[DungeonClearHandler] MonsterTracker UnityEvent 자동 연결 완료");
            }
            
            // DungeonManager 이벤트도 추가 확인
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonCleared -= HandleDungeonCleared;
                _dungeonManager.DungeonCleared += HandleDungeonCleared;
                Debug.Log("[DungeonClearHandler] DungeonManager 이벤트 구독 완료");
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
            // 키보드 8: 모든 몬스터 죽이기 (디버그)
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                Debug.Log("[DungeonClearHandler] 키보드 8 누름 - 모든 몬스터 제거");
                KillAllMonsters();
                return;
            }
            
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
            
            // MonsterTracker 정리
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
