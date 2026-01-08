using UnityEngine;

public class PlayerVFXController : MonoBehaviour
{
    [Header("Attack VFX")]
    [SerializeField] private GameObject[] _attackVFX;
    [SerializeField] private Transform _attackVFXSpawnPoint;

    [Header("Hit VFX")]
    [SerializeField] private GameObject[] _hitVFX;

    [Header("VFX Settings")]
    [SerializeField] private float _vfxLifetime = 2f;

    private void Awake()
    {
        if(_attackVFXSpawnPoint == null)
        {
            GameObject SpawnPoint = new GameObject("AttackVFXSpawnPoint");
            SpawnPoint.transform.SetParent(transform);
            SpawnPoint.transform.localPosition = new Vector3(0f, 1f, 1f);
            _attackVFXSpawnPoint = SpawnPoint.transform;
        }
    }

    public void SpawnAttackVFX(int comboStep)
    {
        if(_attackVFX == null || _attackVFX.Length == 0)
        {
            return;
        }

        int index = Mathf.Clamp(comboStep - 1, 0, _attackVFX.Length - 1);
        GameObject vfxPrefab = _attackVFX[index];

        if (vfxPrefab == null || _attackVFXSpawnPoint == null)
        {
            return;
        }

        GameObject vfx = Instantiate(vfxPrefab, _attackVFXSpawnPoint.position, _attackVFXSpawnPoint.rotation);

        Destroy(vfx, _vfxLifetime);
    }

    public void SpawnHitVFX(Vector3 position, int comboStep)
    {
        if(_hitVFX == null || _hitVFX.Length == 0)
        {
            return;
        }

        int index = Mathf.Clamp(comboStep - 1, 0, _hitVFX.Length - 1);
        GameObject vfxPrefab = _hitVFX[index];

        if (vfxPrefab == null)
        {
            return;
        }

        GameObject vfx = Instantiate(vfxPrefab, position, Quaternion.identity);

        Destroy(vfx, _vfxLifetime);
    }

    // 커스텀
    public void Spawn(GameObject vfxPrefab, Vector3 position, Quaternion rotation)
    {
        if(vfxPrefab == null)
        {
            return;
        }
        
        GameObject vfx = Instantiate(vfxPrefab, position, rotation);
        Destroy(vfx, _vfxLifetime);
    }

    // VFX 설정
    public void SetAttackVFX(GameObject[] vfxArray)
    {
        _attackVFX = vfxArray;
    }

    public void SetHitVFX(GameObject[] vfxArray)
    {
        _hitVFX = vfxArray;
    }

    public void SetVFXLifetime(float lifetime)
    {
        _vfxLifetime = lifetime;
    }
}

