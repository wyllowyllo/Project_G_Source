using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace SJ
{
    /// <summary>
    /// 원신 스타일 3단 콤보 시스템
    /// - 각 공격 애니메이션이 완전히 재생됨 (애니메이션 캔슬 방지)
    /// - 콤보 윈도우 시스템으로 부드러운 연결
    /// - 입력 버퍼링으로 타이밍 보정
    /// - 확장 가능한 구조 (4콤보, 5콤보로 확장 가능)
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("=== Combo Configuration ===")]
        [SerializeField] private ComboAttackData[] comboAttacks = new ComboAttackData[3];
        [SerializeField] private float comboTimeout = 2f;
        [SerializeField] private bool canMoveWhileAttacking = false;

        [Header("=== Input Buffer ===")]
        [SerializeField] private float inputBufferDuration = 0.3f;
        [SerializeField] private bool showDebugLogs = true;

        [Header("=== Attack Detection ===")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float detectionRadius = 2f;

        [Header("=== Visual Effects ===")]
        [SerializeField] private GameObject[] attackVFX;
        [SerializeField] private GameObject[] hitVFX;

        [Header("=== Events ===")]
        public UnityEvent<int> OnComboStarted;
        public UnityEvent<int> OnComboExecuted;
        public UnityEvent<int, GameObject> OnEnemyHit;
        public UnityEvent OnComboReset;

        // Components
        private Animator animator;
        private PlayerMovement playerMovement;

        // Combat State Machine
        private ComboState currentState = ComboState.Idle;
        private int currentComboIndex = 0;
        private int queuedComboIndex = -1; // 다음에 실행될 콤보 인덱스

        // Input Buffer
        private bool hasBufferedInput = false;
        private float bufferedInputTime = 0f;

        // Attack State
        private bool isInComboWindow = false;
        private bool damageDealtThisAttack = false;
        private HashSet<Collider> hitEnemiesThisAttack = new HashSet<Collider>();

        // Timers
        private Coroutine attackCoroutine;
        private Coroutine comboTimeoutCoroutine;

        // Animator Parameters (cached for performance)
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");
        private static readonly int ComboIndexHash = Animator.StringToHash("ComboIndex");
        private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
        private static readonly int AttackSpeedHash = Animator.StringToHash("AttackSpeed");

        private void Awake()
        {
            InitializeComponents();
            ValidateConfiguration();
            InitializeComboData();
        }

        private void Update()
        {
            ProcessInput();
            UpdateInputBuffer();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void InitializeComponents()
        {
            animator = GetComponent<Animator>();
            playerMovement = GetComponent<PlayerMovement>();

            if (animator == null)
            {
                Debug.LogError($"[PlayerCombat] Animator 컴포넌트를 찾을 수 없습니다! ({gameObject.name})");
            }

            if (playerMovement == null)
            {
                Debug.LogError($"[PlayerCombat] PlayerMovement 컴포넌트를 찾을 수 없습니다! ({gameObject.name})");
            }

            // Attack Point 자동 생성
            if (attackPoint == null)
            {
                GameObject point = new GameObject("AttackPoint");
                point.transform.SetParent(transform);
                point.transform.localPosition = new Vector3(0, 1f, 1f);
                attackPoint = point.transform;

                if (showDebugLogs)
                    Debug.Log("[PlayerCombat] AttackPoint가 자동 생성되었습니다.");
            }
        }
        private void ValidateConfiguration()
        {
            if (comboAttacks == null || comboAttacks.Length == 0)
            {
                Debug.LogError("[PlayerCombat] Combo Attacks가 설정되지 않았습니다!");
                return;
            }

            for (int i = 0; i < comboAttacks.Length; i++)
            {
                if (comboAttacks[i].animationDuration <= 0)
                {
                    Debug.LogWarning($"[PlayerCombat] Combo {i + 1}: animationDuration이 0 이하입니다!");
                }

                if (comboAttacks[i].damage <= 0)
                {
                    Debug.LogWarning($"[PlayerCombat] Combo {i + 1}: damage가 0 이하입니다!");
                }
            }
        }

        private void InitializeComboData()
        {
            if (comboAttacks.Length == 0)
            {
                comboAttacks = new ComboAttackData[3]
                {
                    new ComboAttackData
                    {
                        comboName = "Light Attack 1",
                        damage = 100f,
                        animationDuration = 0.6f,
                        comboWindowStart = 0.4f,
                        comboWindowEnd = 0.9f,
                        damageFrame = 0.3f,
                        attackRange = 2f,
                        knockbackForce = 3f
                    },
                    new ComboAttackData
                    {
                        comboName = "Light Attack 2",
                        damage = 120f,
                        animationDuration = 0.7f,
                        comboWindowStart = 0.45f,
                        comboWindowEnd = 0.9f,
                        damageFrame = 0.35f,
                        attackRange = 2.2f,
                        knockbackForce = 4f
                    },
                    new ComboAttackData
                    {
                        comboName = "Light Attack 3 (Finisher)",
                        damage = 180f,
                        animationDuration = 1.0f,
                        comboWindowStart = 0.5f,
                        comboWindowEnd = 0.95f,
                        damageFrame = 0.4f,
                        attackRange = 2.5f,
                        knockbackForce = 8f
                    }
                };
            }
        }

        private void ProcessInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleAttackInput();
            }
        }

        private void HandleAttackInput()
        {
            switch (currentState)
            {
                case ComboState.Idle:
                    // Idle 상태: 즉시 첫 번째 공격 실행
                    StartCombo();
                    break;

                case ComboState.Attacking:
                    // 공격 중: 입력을 버퍼에 저장
                    BufferInput();
                    break;

                case ComboState.ComboWindow:
                    // 콤보 윈도우: 즉시 다음 공격 실행
                    ExecuteNextCombo();
                    break;

                case ComboState.Recovery:
                    // 회복 중: 입력 무시
                    if (showDebugLogs)
                        Debug.Log("[PlayerCombat] 회복 중에는 공격할 수 없습니다.");
                    break;
            }
        }

        private void UpdateInputBuffer()
        {
            if (!hasBufferedInput) return;

            // 버퍼 시간 경과 체크
            bufferedInputTime -= Time.deltaTime;

            if (bufferedInputTime <= 0)
            {
                // 버퍼 시간 초과
                ClearInputBuffer();

                if (showDebugLogs)
                    Debug.Log("[PlayerCombat] 입력 버퍼 시간 초과");
                return;
            }

            // 콤보 윈도우에 진입하면 버퍼된 입력 실행
            if (currentState == ComboState.ComboWindow)
            {
                ExecuteBufferedInput();
            }
        }

        private void BufferInput()
        {
            hasBufferedInput = true;
            bufferedInputTime = inputBufferDuration;

            if (showDebugLogs)
                Debug.Log($"[PlayerCombat] 입력 버퍼 저장됨 ({inputBufferDuration}초)");
        }

        private void ExecuteBufferedInput()
        {
            if (!hasBufferedInput) return;

            ClearInputBuffer();
            ExecuteNextCombo();

            if (showDebugLogs)
                Debug.Log("[PlayerCombat] 버퍼된 입력 실행!");
        }

        private void ClearInputBuffer()
        {
            hasBufferedInput = false;
            bufferedInputTime = 0f;
        }

        private void StartCombo()
        {
            currentComboIndex = 0;
            ChangeState(ComboState.Attacking);
            ExecuteAttack(currentComboIndex);

            OnComboStarted?.Invoke(currentComboIndex);

            if (showDebugLogs)
                Debug.Log("[PlayerCombat] 콤보 시작!");
        }

        private void ExecuteNextCombo()
        {
            // 다음 콤보 인덱스 계산
            int nextComboIndex = currentComboIndex + 1;

            // 최대 콤보 수를 초과하면 첫 번째 콤보로 리셋
            if (nextComboIndex >= comboAttacks.Length)
            {
                if (showDebugLogs)
                    Debug.Log("[PlayerCombat] 최대 콤보 도달, 콤보 리셋");

                ResetCombo();
                return;
            }

            currentComboIndex = nextComboIndex;
            ChangeState(ComboState.Attacking);
            ExecuteAttack(currentComboIndex);

            if (showDebugLogs)
                Debug.Log($"[PlayerCombat] 다음 콤보 실행: {currentComboIndex + 1}단");
        }
        private void ExecuteAttack(int comboIndex)
        {
            if (comboIndex < 0 || comboIndex >= comboAttacks.Length)
            {
                Debug.LogError($"[PlayerCombat] 잘못된 콤보 인덱스: {comboIndex}");
                return;
            }

            ComboAttackData attackData = comboAttacks[comboIndex];

            // 공격 상태 초기화
            damageDealtThisAttack = false;
            hitEnemiesThisAttack.Clear();
            isInComboWindow = false;
            ClearInputBuffer();

            // 플레이어 회전 (적 방향 or 카메라 방향)
            RotateTowardsTarget();

            // 이동 제한
            if (!canMoveWhileAttacking && playerMovement != null)
            {
                playerMovement.SetMovementEnabled(false);
            }

            // 애니메이션 재생
            PlayAttackAnimation(comboIndex);

            // 공격 이펙트 생성
            SpawnAttackVFX(comboIndex);

            // 공격 타이밍 코루틴 시작
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }
            attackCoroutine = StartCoroutine(AttackRoutine(attackData, comboIndex));

            // 콤보 타임아웃 갱신
            RefreshComboTimeout();

            // 이벤트 발생
            OnComboExecuted?.Invoke(comboIndex);

            if (showDebugLogs)
                Debug.Log($"[PlayerCombat] {attackData.comboName} 실행!");
        }

        private IEnumerator AttackRoutine(ComboAttackData attackData, int comboIndex)
        {
            float elapsedTime = 0f;
            float duration = attackData.animationDuration;

            // 타이밍 계산
            float damageTime = duration * attackData.damageFrame;
            float windowStartTime = duration * attackData.comboWindowStart;
            float windowEndTime = duration * attackData.comboWindowEnd;

            bool damageApplied = false;
            bool windowOpened = false;

            // 애니메이션 진행
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;

                // 데미지 적용 타이밍
                if (!damageApplied && elapsedTime >= damageTime)
                {
                    ApplyDamage(attackData, comboIndex);
                    damageApplied = true;
                }

                // 콤보 윈도우 시작
                if (!windowOpened && elapsedTime >= windowStartTime)
                {
                    OpenComboWindow();
                    windowOpened = true;
                }

                // 콤보 윈도우 종료
                if (windowOpened && elapsedTime >= windowEndTime)
                {
                    CloseComboWindow();
                }

                yield return null;
            }

            // 공격 종료
            FinishAttack(comboIndex);
        }

        private void OpenComboWindow()
        {
            isInComboWindow = true;
            ChangeState(ComboState.ComboWindow);

            if (showDebugLogs)
                Debug.Log($"[PlayerCombat] 콤보 윈도우 열림 (입력 대기 중...)");
        }

        private void CloseComboWindow()
        {
            if (currentState == ComboState.ComboWindow)
            {
                isInComboWindow = false;
                ChangeState(ComboState.Recovery);

                if (showDebugLogs)
                    Debug.Log("[PlayerCombat] 콤보 윈도우 닫힘");
            }
        }
        private void FinishAttack(int comboIndex)
        {
            // 이동 허용
            if (playerMovement != null)
            {
                playerMovement.SetMovementEnabled(true);
            }

            // Animator 상태 업데이트
            if (animator != null)
            {
                animator.SetBool(IsAttackingHash, false);
            }

            // Idle 상태로 전환 (콤보 타임아웃에 의해 관리됨)
            if (currentState != ComboState.Attacking) // 다음 공격이 시작되지 않았다면
            {
                ChangeState(ComboState.Idle);
            }

            if (showDebugLogs)
                Debug.Log($"[PlayerCombat] 공격 종료: {comboAttacks[comboIndex].comboName}");
        }

        private void ResetCombo()
        {
            currentComboIndex = 0;
            queuedComboIndex = -1;

            ClearInputBuffer();
            CancelComboTimeout();

            ChangeState(ComboState.Idle);

            if (animator != null)
            {
                animator.SetInteger(ComboIndexHash, 0);
                animator.SetBool(IsAttackingHash, false);
            }

            OnComboReset?.Invoke();

            if (showDebugLogs)
                Debug.Log("[PlayerCombat] 콤보 리셋");
        }

        private void RefreshComboTimeout()
        {
            CancelComboTimeout();
            comboTimeoutCoroutine = StartCoroutine(ComboTimeoutRoutine());
        }

        private void CancelComboTimeout()
        {
            if (comboTimeoutCoroutine != null)
            {
                StopCoroutine(comboTimeoutCoroutine);
                comboTimeoutCoroutine = null;
            }
        }

        private IEnumerator ComboTimeoutRoutine()
        {
            yield return new WaitForSeconds(comboTimeout);

            if (showDebugLogs)
                Debug.Log("[PlayerCombat] 콤보 타임아웃");

            ResetCombo();
        }

        private void ApplyDamage(ComboAttackData attackData, int comboIndex)
        {
            if (damageDealtThisAttack) return;

            damageDealtThisAttack = true;

            // 공격 범위 내 적 탐지
            Collider[] hitColliders = Physics.OverlapSphere(
                attackPoint.position,
                attackData.attackRange,
                enemyLayer
            );

            int hitCount = 0;

            foreach (Collider enemyCollider in hitColliders)
            {
                // 이미 타격한 적은 스킵
                if (hitEnemiesThisAttack.Contains(enemyCollider))
                    continue;

                // 데미지 적용
                IDamageable damageable = enemyCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackData.damage);
                    hitCount++;
                }

                // 넉백 적용
                ApplyKnockback(enemyCollider, attackData.knockbackForce);

                // 타격 이펙트 생성
                SpawnHitVFX(enemyCollider.transform.position, comboIndex);

                // 타격 목록에 추가
                hitEnemiesThisAttack.Add(enemyCollider);

                // 이벤트 발생
                OnEnemyHit?.Invoke(comboIndex, enemyCollider.gameObject);

                if (showDebugLogs)
                    Debug.Log($"[PlayerCombat] {enemyCollider.name}에게 {attackData.damage} 데미지!");
            }

            if (hitCount > 0 && showDebugLogs)
            {
                Debug.Log($"[PlayerCombat] 총 {hitCount}명 타격!");
            }
        }

        private void ApplyKnockback(Collider enemyCollider, float force)
        {
            Vector3 knockbackDir = (enemyCollider.transform.position - transform.position).normalized;
            knockbackDir.y = 0.3f; // 약간 위로

            // Rigidbody 넉백
            Rigidbody rb = enemyCollider.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(knockbackDir * force, ForceMode.Impulse);
            }

            // IKnockbackable 인터페이스
            IKnockbackable knockbackable = enemyCollider.GetComponent<IKnockbackable>();
            if (knockbackable != null)
            {
                knockbackable.ApplyKnockback(knockbackDir, force);
            }
        }

        private void RotateTowardsTarget()
        {
            // 가장 가까운 적 찾기
            Collider nearestEnemy = FindNearestEnemy();

            Vector3 targetDirection;

            if (nearestEnemy != null)
            {
                // 적이 있으면 적 방향
                targetDirection = (nearestEnemy.transform.position - transform.position).normalized;
                targetDirection.y = 0;
            }
            else if (Camera.main != null)
            {
                // 적이 없으면 카메라 방향
                targetDirection = Camera.main.transform.forward;
                targetDirection.y = 0;
                targetDirection.Normalize();
            }
            else
            {
                return;
            }

            if (targetDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(targetDirection);

                if (playerMovement != null)
                {
                    playerMovement.SetLookDirection(targetDirection);
                }
            }
        }

        private Collider FindNearestEnemy()
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);

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

        private void PlayAttackAnimation(int comboIndex)
        {
            if (animator == null) return;

            animator.SetTrigger(AttackTrigger);
            animator.SetInteger(ComboIndexHash, comboIndex);
            animator.SetBool(IsAttackingHash, true);
        }

        private void SpawnAttackVFX(int comboIndex)
        {
            if (attackVFX == null || comboIndex >= attackVFX.Length) return;

            GameObject vfxPrefab = attackVFX[comboIndex];
            if (vfxPrefab == null) return;

            GameObject vfx = Instantiate(vfxPrefab, attackPoint.position, attackPoint.rotation);
            Destroy(vfx, 2f);
        }

        private void SpawnHitVFX(Vector3 position, int comboIndex)
        {
            if (hitVFX == null || comboIndex >= hitVFX.Length) return;

            GameObject vfxPrefab = hitVFX[comboIndex];
            if (vfxPrefab == null) return;

            GameObject vfx = Instantiate(vfxPrefab, position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        private void ChangeState(ComboState newState)
        {
            if (currentState == newState) return;

            ComboState previousState = currentState;
            currentState = newState;

            if (showDebugLogs)
                Debug.Log($"[PlayerCombat] State: {previousState} → {newState}");
        }

        public void SetCombatEnabled(bool enabled)
        {
            this.enabled = enabled;

            if (!enabled)
            {
                ResetCombo();
            }
        }

        public bool IsAttacking()
        {
            return currentState == ComboState.Attacking ||
                   currentState == ComboState.ComboWindow ||
                   currentState == ComboState.Recovery;
        }

        public int GetCurrentComboIndex()
        {
            return currentComboIndex;
        }

        public ComboState GetCurrentState()
        {
            return currentState;
        }

        public void ForceResetCombo()
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }

            ResetCombo();
        }
        public void CancelAttack()
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }

            if (playerMovement != null)
            {
                playerMovement.SetMovementEnabled(true);
            }

            ResetCombo();
        }

        public enum ComboState
        {
            Idle,           // 대기 (공격 가능)
            Attacking,      // 공격 중 (애니메이션 재생 중)
            ComboWindow,    // 콤보 윈도우 (다음 공격 입력 가능)
            Recovery        // 회복 중 (공격 불가)
        }

        [System.Serializable]
        public struct ComboAttackData
        {
            [Header("Basic Info")]
            [Tooltip("콤보 이름 (예: Light Attack 1)")]
            public string comboName;

            [Header("Damage")]
            [Tooltip("공격 데미지")]
            public float damage;

            [Tooltip("넉백 강도")]
            public float knockbackForce;

            [Tooltip("공격 범위")]
            public float attackRange;

            [Header("Animation Timing")]
            [Tooltip("애니메이션 전체 길이 (초)")]
            public float animationDuration;

            [Tooltip("데미지 적용 타이밍 (0~1, 애니메이션의 몇 % 지점)")]
            [Range(0f, 1f)]
            public float damageFrame;

            [Header("Combo Window")]
            [Tooltip("콤보 윈도우 시작 시점 (0~1, 애니메이션의 몇 % 지점)")]
            [Range(0f, 1f)]
            public float comboWindowStart;

            [Tooltip("콤보 윈도우 종료 시점 (0~1, 애니메이션의 몇 % 지점)")]
            [Range(0f, 1f)]
            public float comboWindowEnd;
        }

        public interface IDamageable
        {
            void TakeDamage(float damage);
        }

        public interface IKnockbackable
        {
            void ApplyKnockback(Vector3 direction, float force);
        }
    }
}