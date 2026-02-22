using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 배그 방식: 복사본의 레이어만 변경하므로 원본 복원 기능 불필요
/// </summary>
public class ViewerLayerManager
{
    private readonly string _targetLayerName;
    private readonly int _targetLayer;

    public ViewerLayerManager(string targetLayerName)
    {
        _targetLayerName = targetLayerName;
        _targetLayer = LayerMask.NameToLayer(_targetLayerName);

        if (_targetLayer == -1)
        {
            Debug.LogWarning($"[ViewerLayerManager] Layer '{_targetLayerName}'가 존재하지 않습니다! " +
                           $"Project Settings > Tags and Layers에서 레이어를 추가하세요.");
        }
    }

    /// <summary>
    /// 오브젝트와 모든 자식의 레이어를 변경
    /// </summary>
    public void SetLayerRecursively(Transform rootObject)
    {
        if (rootObject == null)
        {
            Debug.LogWarning("[ViewerLayerManager] rootObject가 null입니다!");
            return;
        }

        if (_targetLayer == -1)
        {
            Debug.LogError($"[ViewerLayerManager] Layer '{_targetLayerName}'가 유효하지 않습니다!");
            return;
        }

        SetLayerRecursivelyInternal(rootObject, _targetLayer);
        Debug.Log($"[ViewerLayerManager] '{rootObject.name}'의 레이어를 '{_targetLayerName}'으로 변경했습니다.");
    }

    private void SetLayerRecursivelyInternal(Transform obj, int layer)
    {
        // 레이어 변경
        obj.gameObject.layer = layer;

        // 모든 자식에 재귀적으로 적용
        foreach (Transform child in obj)
        {
            SetLayerRecursivelyInternal(child, layer);
        }
    }

    /// <summary>
    /// 배그 방식: 복사본을 삭제하므로 레이어 복원 불필요
    /// 하지만 호환성을 위해 메서드는 남겨둠
    /// </summary>
    public void RestoreLayers(Transform rootObject)
    {
        // 배그 방식에서는 복사본을 삭제하므로 레이어 복원이 필요 없음
        // 아무 작업도 하지 않음
    }

    /// <summary>
    /// 저장된 레이어 정보 초기화 (배그 방식에서는 불필요)
    /// </summary>
    public void Clear()
    {
        // 배그 방식에서는 아무 작업도 하지 않음
    }

    /// <summary>
    /// 타겟 레이어의 마스크 반환
    /// </summary>
    public int GetLayerMask()
    {
        if (_targetLayer == -1)
        {
            Debug.LogWarning($"[ViewerLayerManager] Layer '{_targetLayerName}'가 유효하지 않아 모든 레이어를 렌더링합니다.");
            return -1; // Everything
        }

        return LayerMask.GetMask(_targetLayerName);
    }

    public string TargetLayerName => _targetLayerName;
    public int TargetLayer => _targetLayer;
    public bool IsValid => _targetLayer != -1;
}
