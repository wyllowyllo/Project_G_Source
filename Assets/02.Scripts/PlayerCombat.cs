using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combo Settings")]
    [SerializeField] private int _maxComboCount = 3;
    [SerializeField] private float _comboResetTime = 1.0f;
    [SerializeField] private float _attackCooldown = 0.1f;
    [SerializeField] private bool _canMoveWhileAttacking = false;

    [Header("Attack Damage")]
    [SerializeField]
    private AttackData[] _comboAttacks = new AttackData[3]
    {
        new AttackData { damage = 10f, animationDuration = 0.5f, knockbackForce = 2f },
        new AttackData { damage = 15f, animationDuration = 0.6f, knockbackForce = 3f },
        new AttackData { damage = 25f, animationDuration = 0.8f, knockbackForce = 5f }
    };

    [Header("Attack Range")]
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private float _attackRadius = 1f;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private Transform _attackPoint;

    [Header("Root Motion")]
    [SerializeField] private bool _useRootMotion = false;
    [SerializeField] private float[] _rootMotionMultiplier = new float[] { 1f, 1.2f, 1.5f };

    [Header("VFX Settings")]
    [SerializeField] private GameObject[] _hitEffectPrefabs;
    [SerializeField] private GameObject[] _attackEffectPrefabs;

    [Header("Events")]
    public UnityEvent<int> OnComboChanged;
    public UnityEvent<int> OnAttackExecuted;
    public UnityEvent<int, GameObject> OnEnemyHit;

    // Components
    private Animator _animator;
    private PlayerMovement _playerMovement;

    // Combat State
    private int _currentComboIndex = 0;
    private bool _isAttacking = false;
    private bool _canAttack = true;
    private bool _hasDealtDamage = false;
    private bool _canNextCombo = false;

    // Coroutines
    private Coroutine _comboResetCoroutine;
    private Coroutine _attackCoroutine;

    // Animation Parameters
    private readonly int _attackTriggerHash = Animator.StringToHash("Attack");
    private readonly int _comboIndexHash = Animator.StringToHash("ComboIndex");
    private readonly int _isAttackingHash = Animator.StringToHash("IsAttacking");
    private readonly int _attackSpeedHash = Animator.StringToHash("AttackSpeed");

    // Hit Detection
    private System.Collections.Generic.HashSet<Collider> hitEnemiesInCurrentAttack = new System.Collections.Generic.HashSet<Collider>();

    private void Awake()
    {
        InitializeComponents();
        ValidateSettings();
    }

    private void Update()
    {
        HandleCombatInput();
    }

    private void InitializeComponents()
    {
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();

        if (_animator == null)
        {
            Debug.LogError($"Animator가 {gameObject.name}에 없습니다!");
        }

        if (_playerMovement == null)
        {
            Debug.LogError($"PlayerMovementKCC가 {gameObject.name}에 없습니다!");
        }

        // AttackPoint 자동 설정
        if (_attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(0, 1f, 1f);
            _attackPoint = attackPointObj.transform;
        }

        // Root Motion 설정
        if (_animator != null && _useRootMotion)
        {
            _animator.applyRootMotion = true;
        }
    }

    private void ValidateSettings()
    {
        // 배열 크기 검증
        if (_comboAttacks.Length != _maxComboCount)
        {
            Debug.LogWarning($"comboAttacks 배열 크기({_comboAttacks.Length})가 maxComboCount({_maxComboCount})와 다릅니다!");
        }

        if (_rootMotionMultiplier.Length != _maxComboCount)
        {
            Debug.LogWarning($"rootMotionMultiplier 배열 크기를 조정합니다.");
            System.Array.Resize(ref _rootMotionMultiplier, _maxComboCount);
            for (int i = 0; i < _maxComboCount; i++)
            {
                if (_rootMotionMultiplier[i] == 0) _rootMotionMultiplier[i] = 1f;
            }
        }
    }

    private void HandleCombatInput()
    {
        // 왼쪽 마우스 클릭 감지
        if (Input.GetMouseButtonDown(0) && _canAttack && !_isAttacking)
        {
            ExecuteAttack();
        }
    }

    private void ExecuteAttack()
    {
        // 공격 시작
        _isAttacking = true;
        _canAttack = false;
        _hasDealtDamage = false;
        hitEnemiesInCurrentAttack.Clear();

        // 이동 제어
        if (!_canMoveWhileAttacking && _playerMovement != null)
        {
            _playerMovement.SetMovementEnabled(false);
        }

        // 공격 방향으로 캐릭터 회전 (적이 없으면 카메라 방향)
        RotateTowardsTarget();

        // 애니메이션 재생
        PlayAttackAnimation();

        // 공격 이펙트 생성
        SpawnAttackEffect();

        // 공격 코루틴 시작
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
        }
        _attackCoroutine = StartCoroutine(AttackRoutine());

        // 콤보 리셋 타이머 재시작
        ResetComboTimer();

        // 이벤트 발생
        OnAttackExecuted?.Invoke(_currentComboIndex);

        Debug.Log($"공격 실행: 콤보 {_currentComboIndex + 1}타");
    }

    private IEnumerator AttackRoutine()
    {
        AttackData currentAttack = GetCurrentAttackData();

        // 공격 애니메이션 진행 (데미지 적용 타이밍까지)
        float damageDelay = currentAttack.animationDuration * 0.4f;
        yield return new WaitForSeconds(damageDelay);

        // 데미지 적용
        if (!_hasDealtDamage)
        {
            ApplyDamage();
            _hasDealtDamage = true;
        }

        // 나머지 애니메이션 대기
        float remainingTime = currentAttack.animationDuration - damageDelay;
        yield return new WaitForSeconds(remainingTime);

        // 공격 종료
        CompleteAttack();
    }

    private void RotateTowardsTarget()
    {
        // 가장 가까운 적 찾기
        Collider nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null)
        {
            // 적 방향으로 회전
            Vector3 directionToEnemy = (nearestEnemy.transform.position - transform.position).normalized;
            directionToEnemy.y = 0;

            if (directionToEnemy.magnitude > 0.1f && _playerMovement != null)
            {
                _playerMovement.SetLookDirection(directionToEnemy);
                transform.rotation = Quaternion.LookRotation(directionToEnemy);
            }
        }
        else if (Camera.main != null)
        {
            // 적이 없으면 카메라 방향으로
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            if (cameraForward.magnitude > 0.1f && _playerMovement != null)
            {
                _playerMovement.SetLookDirection(cameraForward);
                transform.rotation = Quaternion.LookRotation(cameraForward);
            }
        }
    }

    private Collider FindNearestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, _attackRange * 2f, _enemyLayer);

        if (enemies.Length == 0) return null;

        Collider nearest = null;
        float minDistance = float.MaxValue;

        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void ApplyDamage()
    {
        // 공격 범위 내의 적 탐지
        Collider[] hitEnemies = Physics.OverlapSphere(
            _attackPoint.position,
            _attackRadius,
            _enemyLayer
        );

        AttackData currentAttack = GetCurrentAttackData();
        int hitCount = 0;

        // 감지된 적들에게 데미지 적용
        foreach (Collider enemy in hitEnemies)
        {
            // 이미 이 공격에서 맞은 적은 스킵
            if (hitEnemiesInCurrentAttack.Contains(enemy))
                continue;

            // 데미지 적용
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(currentAttack.damage);
                hitEnemiesInCurrentAttack.Add(enemy);
                hitCount++;

                // 넉백 적용
                ApplyKnockback(enemy, currentAttack.knockbackForce);

                // 히트 이펙트 생성
                SpawnHitEffect(enemy.transform.position);

                // 이벤트 발생
                OnEnemyHit?.Invoke(_currentComboIndex, enemy.gameObject);

                Debug.Log($"{enemy.name}에게 {currentAttack.damage} 데미지를 입혔습니다!");
            }
        }

        if (hitCount > 0)
        {
            Debug.Log($"총 {hitCount}명의 적을 타격했습니다!");
        }
    }

    private void ApplyKnockback(Collider enemy, float force)
    {
        // 넉백 방향 계산
        Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
        knockbackDirection.y = 0.3f; // 약간 위로

        // Rigidbody가 있으면 물리적 넉백
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(knockbackDirection * force, ForceMode.Impulse);
        }

        // IKnockbackable 넉백 전달
        IKnockbackable knockbackable = enemy.GetComponent<IKnockbackable>();
        if (knockbackable != null)
        {
            knockbackable.ApplyKnockback(knockbackDirection, force);
        }
    }

    private void CompleteAttack()
    {
        _isAttacking = false;

        // 애니메이터 상태 업데이트
        if (_animator != null)
        {
            _animator.SetBool(_isAttackingHash, false);
        }

        // 이동 다시 허용
        if (_playerMovement != null)
        {
            _playerMovement.SetMovementEnabled(true);
        }

        // 다음 콤보 준비
        AdvanceCombo();

        // 공격 쿨다운 후 다시 공격 가능
        StartCoroutine(AttackCooldownRoutine());
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(_attackCooldown);
        _canAttack = true;
    }

    private void AdvanceCombo()
    {
        _currentComboIndex++;

        // 콤보 인덱스가 최대치를 넘으면 초기화
        if (_currentComboIndex >= _maxComboCount)
        {
            _currentComboIndex = 0;
        }

        // 애니메이터에 콤보 인덱스 전달
        if (_animator != null)
        {
            _animator.SetInteger(_comboIndexHash, _currentComboIndex);
        }

        // 이벤트 발생
        OnComboChanged?.Invoke(_currentComboIndex);
    }

    private void ResetCombo()
    {
        _currentComboIndex = 0;

        if (_animator != null)
        {
            _animator.SetInteger(_comboIndexHash, _currentComboIndex);
        }

        OnComboChanged?.Invoke(_currentComboIndex);

        Debug.Log("콤보가 초기화되었습니다.");
    }

    private void ResetComboTimer()
    {
        // 기존 타이머 중지
        if (_comboResetCoroutine != null)
        {
            StopCoroutine(_comboResetCoroutine);
        }

        // 새 타이머 시작
        _comboResetCoroutine = StartCoroutine(ComboResetRoutine());
    }

    private IEnumerator ComboResetRoutine()
    {
        yield return new WaitForSeconds(_comboResetTime);
        ResetCombo();
    }

    private void PlayAttackAnimation()
    {
        if (_animator == null) return;

        // 공격 트리거 발동
        _animator.SetTrigger(_attackTriggerHash);
        _animator.SetInteger(_comboIndexHash, _currentComboIndex);
        _animator.SetBool(_isAttackingHash, true);

/*        // 공격 속도 조절 (옵션)
        if (_animator.HasParameter(_attackSpeedHash))
        {
            _animator.SetFloat(_attackSpeedHash, 1f);
        }*/
    }

    private void SpawnAttackEffect()
    {
        if (_attackEffectPrefabs == null || _attackEffectPrefabs.Length == 0) return;

        int index = Mathf.Min(_currentComboIndex, _attackEffectPrefabs.Length - 1);
        if (_attackEffectPrefabs[index] != null)
        {
            GameObject effect = Instantiate(
                _attackEffectPrefabs[index],
                _attackPoint.position,
                _attackPoint.rotation
            );

            Destroy(effect, 2f);
        }
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (_hitEffectPrefabs == null || _hitEffectPrefabs.Length == 0) return;

        int index = Mathf.Min(_currentComboIndex, _hitEffectPrefabs.Length - 1);
        if (_hitEffectPrefabs[index] != null)
        {
            GameObject effect = Instantiate(
                _hitEffectPrefabs[index],
                position,
                Quaternion.identity
            );

            Destroy(effect, 2f);
        }
    }

    private AttackData GetCurrentAttackData()
    {
        if (_currentComboIndex >= 0 && _currentComboIndex < _comboAttacks.Length)
        {
            return _comboAttacks[_currentComboIndex];
        }

        Debug.LogWarning($"잘못된 콤보 인덱스: {_currentComboIndex}");
        return new AttackData { damage = 10f, animationDuration = 0.5f, knockbackForce = 2f };
    }

    public void SetCombatEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            ResetCombo();
            _isAttacking = false;
            _canAttack = true;
        }
    }

    public bool IsAttacking()
    {
        return _isAttacking;
    }

    public int GetCurrentComboIndex()
    {
        return _currentComboIndex;
    }

    public void ForceResetCombo()
    {
        if (_comboResetCoroutine != null)
        {
            StopCoroutine(_comboResetCoroutine);
        }
        ResetCombo();
    }

    public void CancelAttack()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
        }

        CompleteAttack();
    }

    private void OnDrawGizmosSelected()
    {
        if (_attackPoint == null) return;

        // 공격 범위 시각화
        Gizmos.color = _isAttacking ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(_attackPoint.position, _attackRadius);

        // 적 탐지 범위
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _attackRange * 2f);

        // 공격 방향 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, _attackPoint.position);
    }

}


    [System.Serializable]
public struct AttackData
{
    [Tooltip("공격 데미지")]
    public float damage;

    [Tooltip("애니메이션 지속 시간")]
    public float animationDuration;

    [Tooltip("넉백 강도")]
    public float knockbackForce;

    [Tooltip("공격 범위 배율 (옵션)")]
    public float rangeMultiplier;
}

public interface IDamageable
{
    void TakeDamage(float damage);
}

public interface IKnockbackable
{
    void ApplyKnockback(Vector3 direction, float force);
}

