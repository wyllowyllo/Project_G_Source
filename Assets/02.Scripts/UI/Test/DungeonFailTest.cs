using UnityEngine;

public class DungeonFailTest : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private DungeonFailUI dungeonFailUI;
    [SerializeField] private KeyCode testKey = KeyCode.O;
    [SerializeField] private string testDungeonName = "몽글몽글 연덕";

    [Header("디버그 옵션")]
    [SerializeField] private bool showDebugMessage = true;

    private void Update()
    {
        // P키를 누르면 던전 클리어 애니메이션 실행
        if (Input.GetKeyDown(testKey))
        {
            TestDungeonClear();
        }
    }

    private void TestDungeonClear()
    {
        if (dungeonFailUI == null)
        {
            Debug.LogError("DungeonClearUI가 할당되지 않았습니다!");
            return;
        }

        if (showDebugMessage)
        {
            Debug.Log($"[테스트] 던전 클리어 애니메이션 실행: {testDungeonName}");
        }

        // 던전 클리어 애니메이션 실행
        dungeonFailUI.ShowDungeonClear(testDungeonName);
    }

    // Inspector에서 테스트 버튼으로 실행하기
    [ContextMenu("던전 클리어 테스트")]
    private void TestFromInspector()
    {
        TestDungeonClear();
    }
}
