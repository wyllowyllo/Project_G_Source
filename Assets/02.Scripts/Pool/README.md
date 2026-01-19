# Object Pooling System

UnityEngine.Pool 기반 범용 오브젝트 풀링 시스템

## 아키텍처

```
┌─────────────────────────────────────┐
│         ObjectPoolManager           │  ← 싱글턴 풀 매니저
│   (Singleton, DontDestroyOnLoad)    │
└─────────────┬───────────────────────┘
              │
    ┌─────────┴─────────┐
    │ ObjectPool<T>     │  ← 프리팹별 풀
    │ UnityEngine.Pool  │
    └─────────┬─────────┘
              │
    ┌─────────┴─────────┐
    │   IPooledObject   │  ← 풀링 가능 객체 인터페이스
    └───────────────────┘
              ▲
    ┌─────────┼─────────────────┐
    │         │                 │
┌───┴───┐ ┌───┴───┐      ┌──────┴──────┐
│PooledVFX│ │VFXAutoDestroy│ │PooledProjectile│  ← 구체 구현
└─────────┘ └──────────────┘ └────────────────┘
```

## 파일 구조

```
Assets/02.Scripts/Pool/
├── Core/
│   ├── IPooledObject.cs      # 풀링 객체 인터페이스
│   ├── ObjectPoolManager.cs  # 싱글턴 매니저
│   └── PoolSpawner.cs        # 정적 유틸리티
├── Components/
│   └── PooledVFX.cs          # VFX용 구현체
└── README.md
```

## 설정 방법

### 1. ObjectPoolManager 추가

씬에 빈 GameObject를 생성하고 `ObjectPoolManager` 컴포넌트를 추가합니다.

```
Hierarchy:
└── [ObjectPoolManager]    ← ObjectPoolManager 컴포넌트
    └── Pool_Container     ← 자동 생성 (비활성 객체 보관)
```

**Inspector 설정:**
- `Default Capacity`: 풀 초기 용량 (기본값: 10)
- `Max Size`: 풀 최대 크기 (기본값: 100)

### 2. VFX 프리팹 설정

VFX 프리팹에 다음 중 하나의 컴포넌트를 추가합니다:

| 컴포넌트 | 용도 |
|----------|------|
| `VFXAutoDestroy` | 기존 VFX에 풀링 지원 추가 (하위 호환) |
| `PooledVFX` | 새로운 VFX 전용 컴포넌트 |

## 사용법

### 기본 스폰/반환

```csharp
using Pool.Core;

// 스폰
GameObject instance = PoolSpawner.Spawn(prefab, position, rotation);

// 반환
PoolSpawner.Release(instance);

// 지연 반환
PoolSpawner.Release(instance, 2f);  // 2초 후 반환
```

### VFX 스폰 (자동 반환)

```csharp
// duration 후 자동으로 풀에 반환
PoolSpawner.SpawnVFX(vfxPrefab, position, rotation, duration);
```

### 제네릭 스폰

```csharp
// 컴포넌트 타입으로 스폰
MyComponent comp = PoolSpawner.Spawn<MyComponent>(prefab, position, rotation);
```

### 풀 웜업 (미리 생성)

```csharp
// 게임 시작 시 미리 10개 생성
PoolSpawner.WarmupPool(bulletPrefab, 10);
```

### 풀 정리

```csharp
// 특정 프리팹의 풀 정리
PoolSpawner.ClearPool(prefab);

// 모든 풀 정리
PoolSpawner.ClearAllPools();
```

## 커스텀 풀링 객체 구현

`IPooledObject` 인터페이스를 구현하여 커스텀 풀링 객체를 만들 수 있습니다.

### 예시: 투사체

```csharp
using Pool.Core;
using UnityEngine;

public class PooledProjectile : MonoBehaviour, IPooledObject
{
    private Rigidbody _rigidbody;
    private TrailRenderer _trail;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _trail = GetComponent<TrailRenderer>();
    }

    public void OnSpawnFromPool()
    {
        // 속도 초기화
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        // Trail 리셋
        _trail?.Clear();
    }

    public void OnReturnToPool()
    {
        // 이동 정지
        _rigidbody.linearVelocity = Vector3.zero;
    }
}
```

### 예시: 데미지 넘버

```csharp
using Pool.Core;
using UnityEngine;
using TMPro;

public class PooledDamageNumber : MonoBehaviour, IPooledObject
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private float _lifetime = 1f;

    private float _spawnTime;

    public void OnSpawnFromPool()
    {
        _spawnTime = Time.time;
        // 애니메이션 리셋
        transform.localScale = Vector3.one;
    }

    public void OnReturnToPool()
    {
        _text.text = string.Empty;
    }

    private void Update()
    {
        if (Time.time >= _spawnTime + _lifetime)
        {
            PoolSpawner.Release(gameObject);
        }
    }

    public void SetDamage(int damage, bool isCritical)
    {
        _text.text = damage.ToString();
        _text.color = isCritical ? Color.yellow : Color.white;
    }
}
```

## 풀링 시스템 없이 동작

`ObjectPoolManager`가 씬에 없어도 코드는 정상 동작합니다.
- `PoolSpawner.Spawn()` → `Object.Instantiate()` 사용
- `PoolSpawner.Release()` → `Object.Destroy()` 사용

## 주의사항

1. **스케일 리셋**: 스폰 시 프리팹의 원본 스케일로 자동 복원됩니다.

2. **파티클 재시작**: `IPooledObject.OnSpawnFromPool()`에서 파티클을 Clear 후 Play 해야 합니다.

3. **참조 정리**: `OnReturnToPool()`에서 외부 참조를 정리하세요.

4. **DontDestroyOnLoad**: `ObjectPoolManager`는 씬 전환 시에도 유지됩니다.

## 적용된 파일

| 파일 | 변경 내용 |
|------|----------|
| `PlayerVFXController.cs` | VFX 스폰에 풀링 적용 |
| `SkillVFXHandler.cs` | 스킬 VFX에 풀링 적용 |
| `MonsterFeedback.cs` | 피격/사망 VFX에 풀링 적용 |
| `VFXAutoDestroy.cs` | 풀링 지원 추가 |
