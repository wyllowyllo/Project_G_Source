using UnityEngine;
using Equipment;

/// <summary>
/// DroppedEquipment 디버그 헬퍼
/// DroppedEquipment 오브젝트에 추가하여 문제를 진단합니다
/// </summary>
public class EquipmentTooltipDebugger : MonoBehaviour
{
    [Header("테스트용 장비 데이터")]
    [SerializeField] private EquipmentData _testEquipmentData;
    
    private DroppedEquipment _droppedEquipment;
    private EquipmentTooltipController _tooltipController;
    private Transform _playerTransform;

    private void Start()
    {
        Debug.Log("========== 툴팁 디버거 시작 ==========");
        
        // 컴포넌트 확인
        _droppedEquipment = GetComponent<DroppedEquipment>();
        _tooltipController = GetComponent<EquipmentTooltipController>();
        
        if (_droppedEquipment == null)
        {
            Debug.LogError("[Debug] DroppedEquipment 컴포넌트가 없습니다!");
            return;
        }
        
        if (_tooltipController == null)
        {
            Debug.LogError("[Debug] EquipmentTooltipController 컴포넌트가 없습니다!");
            return;
        }

        // EquipmentData 확인
        if (_droppedEquipment.EquipmentData == null)
        {
            Debug.LogWarning("[Debug] ⚠️ EquipmentData가 null입니다! Initialize()가 호출되지 않았습니다.");
            
            // 테스트 데이터가 있으면 자동으로 초기화
            if (_testEquipmentData != null)
            {
                Debug.Log("[Debug] 테스트 데이터로 초기화합니다...");
                _droppedEquipment.Initialize(_testEquipmentData);
                _tooltipController.UpdateTooltip(_testEquipmentData);
                Debug.Log($"[Debug] ✅ 초기화 완료: {_testEquipmentData.EquipmentName}");
            }
            else
            {
                Debug.LogError("[Debug] ❌ 테스트 데이터도 없습니다! Inspector에서 Test Equipment Data를 할당하세요!");
            }
        }
        else
        {
            Debug.Log($"[Debug] ✅ EquipmentData 확인: {_droppedEquipment.EquipmentData.EquipmentName}");
        }

        // Player 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            Debug.Log("[Debug] ✅ Player 찾음");
        }
        else
        {
            Debug.LogError("[Debug] ❌ Player 태그를 가진 오브젝트를 찾을 수 없습니다!");
        }

        // Canvas 확인
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"[Debug] Canvas RenderMode: {canvas.renderMode}");
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                Debug.LogError("[Debug] ❌ Canvas가 World Space가 아닙니다!");
            }
        }
        else
        {
            Debug.LogError("[Debug] ❌ Canvas를 찾을 수 없습니다!");
        }

        Debug.Log("========== 툴팁 디버거 완료 ==========");
    }

    private void Update()
    {
        // 거리 체크 (매초마다)
        if (Time.frameCount % 60 == 0 && _playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            Debug.Log($"[Debug] 플레이어 거리: {distance:F2}m (표시 거리: 5m)");
            
            if (distance <= 5f)
            {
                Debug.Log("[Debug] ✅ 플레이어가 범위 안에 있습니다. 툴팁이 보여야 합니다!");
            }
            else
            {
                Debug.Log("[Debug] ⚠️ 플레이어가 범위 밖에 있습니다.");
            }
        }

        // F키로 강제 표시 테스트
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("[Debug] F키 눌림 - 툴팁 강제 표시");
            if (_tooltipController != null)
            {
                _tooltipController.ShowTooltip();
            }
        }

        // G키로 강제 숨김 테스트
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("[Debug] G키 눌림 - 툴팁 강제 숨김");
            if (_tooltipController != null)
            {
                _tooltipController.HideTooltip();
            }
        }
    }

    private void OnDrawGizmos()
    {
        // DroppedEquipment 위치 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // 표시 거리 범위
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 5f);

        // 플레이어와의 연결선
        if (_playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            Gizmos.color = distance <= 5f ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, _playerTransform.position);
        }
    }
}
