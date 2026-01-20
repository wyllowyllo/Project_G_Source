using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;

    private void Awake()
    {
        if (_cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _cameraTransform = mainCamera.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (_cameraTransform == null) return;

        transform.forward = _cameraTransform.forward;
    }
}
