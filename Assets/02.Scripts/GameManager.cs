using System.Collections;
using Dungeon;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private EGameState _state = EGameState.MainMenu;
    public EGameState State => _state;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void TriggerGameOver()
    {
        if(_state != EGameState.GameOver)
        {
            StartCoroutine(GameOver_Coroutine());
        }
    }

    private IEnumerator GameOver_Coroutine()
    {
        _state = EGameState.GameOver;

        if (DungeonManager.Instance != null && DungeonManager.Instance.IsInDungeon)
        {
            DungeonManager.Instance.FailDungeon();
        }

        yield return null;
    }
}
