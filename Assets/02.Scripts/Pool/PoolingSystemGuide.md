# Object Pooling System 사용법

## 씬 설정

빈 GameObject 생성 → `ObjectPoolManager` 컴포넌트 추가

---

## 기본 사용법

```csharp
using Pool.Core;

// 스폰
var obj = PoolSpawner.Spawn(prefab, position, rotation);

// 반환
PoolSpawner.Release(obj);

// 지연 반환
PoolSpawner.Release(obj, 2f);

// VFX 스폰 (duration 후 자동 반환)
PoolSpawner.SpawnVFX(prefab, position, rotation, duration);
```

---

## 기존 코드 전환

```csharp
// Before
var obj = Instantiate(prefab, pos, rot);
Destroy(obj, 2f);

// After
var obj = PoolSpawner.Spawn(prefab, pos, rot);
PoolSpawner.Release(obj, 2f);
```

---

## VFX 프리팹 설정

VFX 프리팹에 아래 컴포넌트 중 하나를 추가하면 자동으로 풀에 반환됩니다.

### VFXAutoDestroy (권장)

1. VFX 프리팹 선택
2. `Add Component` → `VFXAutoDestroy` 추가
3. Inspector 설정:
   - `Use Particle Duration`: 체크 시 ParticleSystem duration 사용
   - `Manual Duration`: 수동 지정 시간
   - `Destroy Delay`: 추가 대기 시간

### PooledVFX

1. VFX 프리팹 선택 (ParticleSystem 필수)
2. `Add Component` → `PooledVFX` 추가
3. Inspector 설정은 VFXAutoDestroy와 동일

---

## 커스텀 풀링 객체 만들기

### Step 1: 스크립트 생성

```csharp
using Pool.Core;
using UnityEngine;

public class PooledBullet : MonoBehaviour, IPooledObject
{
    public void OnSpawnFromPool()
    {
        // 풀에서 꺼낼 때 호출됨
    }

    public void OnReturnToPool()
    {
        // 풀에 반환할 때 호출됨
    }
}
```

### Step 2: 초기화 로직 작성

`OnSpawnFromPool()`에 매번 스폰될 때 리셋해야 할 것들을 작성:

```csharp
public void OnSpawnFromPool()
{
    // 물리 초기화
    _rigidbody.linearVelocity = Vector3.zero;

    // Trail 초기화
    _trail.Clear();

    // 파티클 재시작
    _particleSystem.Clear();
    _particleSystem.Play();

    // 상태 초기화
    _isAlive = true;
    _hp = _maxHp;
}
```

### Step 3: 정리 로직 작성

`OnReturnToPool()`에 풀에 돌아갈 때 정리할 것들을 작성:

```csharp
public void OnReturnToPool()
{
    // 코루틴 정지
    StopAllCoroutines();

    // 사운드 정지
    _audioSource.Stop();

    // 이벤트 해제
    SomeManager.OnEvent -= HandleEvent;
}
```

### Step 4: 프리팹에 컴포넌트 추가

1. 프리팹 선택
2. `Add Component` → 만든 스크립트 추가

### Step 5: 코드에서 사용

```csharp
// 스폰
var bullet = PoolSpawner.Spawn(bulletPrefab, pos, rot);

// 반환 (충돌 시, 수명 종료 시 등)
PoolSpawner.Release(bullet.gameObject);
```

---

## 자동 반환 예제

일정 시간 후 자동으로 풀에 반환:

```csharp
using Pool.Core;
using UnityEngine;

public class PooledBullet : MonoBehaviour, IPooledObject
{
    [SerializeField] private float _lifetime = 3f;

    private float _spawnTime;

    public void OnSpawnFromPool()
    {
        _spawnTime = Time.time;
    }

    public void OnReturnToPool() { }

    private void Update()
    {
        if (Time.time >= _spawnTime + _lifetime)
        {
            PoolSpawner.Release(gameObject);
        }
    }
}
```

---

## 기타

```csharp
// 미리 생성
PoolSpawner.WarmupPool(prefab, 10);

// 풀 정리
PoolSpawner.ClearPool(prefab);
PoolSpawner.ClearAllPools();
```
