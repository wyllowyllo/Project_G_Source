using UnityEngine;
using System.Collections.Generic;

public class ViewerLayerManager
{
    private readonly string _targetLayerName;
    private readonly Dictionary<Transform, int> _originalLayers = new Dictionary<Transform, int>();

    public ViewerLayerManager(string targetLayerName)
    {
        _targetLayerName = targetLayerName;
    }

    public void SetLayerRecursively(Transform rootObject)
    {
        if (rootObject == null)
        {
            return;
        }

        int targetLayer = LayerMask.NameToLayer(_targetLayerName);
        if (targetLayer == -1)
        {
            Debug.Log($"[ViewerLayerManager] Layer '{_targetLayerName}'가 존재하지 않습니다!");
            return;
        }

        SetLayerRecursivelyInternal(rootObject, targetLayer);
    }

    private void SetLayerRecursivelyInternal(Transform obj, int targetLayer)
    {
        // 원래 레이어 저장 (중복 저장 방지)
        if (!_originalLayers.ContainsKey(obj))
        {
            _originalLayers[obj] = obj.gameObject.layer;
        }

        // 레이어 변경
        obj.gameObject.layer = targetLayer;

        // 모든 자식에 적용
        foreach (Transform child in obj)
        {
            SetLayerRecursivelyInternal(child, targetLayer);
        }
    }

    // 저장된 원래 레이어로 복원
    public void RestoreLayers(Transform rootObject)
    {
        if (rootObject == null)
        {
            return;
        }

        RestoreLayersRecursively(rootObject);
    }

    private void RestoreLayersRecursively(Transform obj)
    {
        // 저장된 원래 레이어가 있으면 복원
        if (_originalLayers.ContainsKey(obj))
        {
            obj.gameObject.layer = _originalLayers[obj];
        }

        // 모든 자식에 적용
        foreach (Transform child in obj)
        {
            RestoreLayersRecursively(child);
        }
    }

    // 저장된 레이어 정보 초기화
    public void Clear()
    {
        _originalLayers.Clear();
    }

    public int GetLayerMask()
    {
        int layerMask = LayerMask.GetMask(_targetLayerName);
        if (layerMask == 0)
        {
            return -1; // Everything
        }
        return layerMask;
    }

    public string TargetLayerName => _targetLayerName;
}
