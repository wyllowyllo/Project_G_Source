using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using SJ;

public class SjEnemy : MonoBehaviour, IDamageable, IKnockbackable
{
    [Header("Enemy Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isInvincible = false;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackResistance = 1f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private bool canBeKnockedBack = true;

    [Header("Visual Feedback")]
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float damageFlashDuration = 0.1f;
    [SerializeField] private GameObject damageNumberPrefab;

    [Header("Death Settings")]
    [SerializeField] private float deathDelay = 1f;
    [SerializeField] private GameObject deathEffectPrefab;

    [Header("UI")]
    [SerializeField] private Transform healthBarPosition;
    [SerializeField] private GameObject healthBarPrefab;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent<float> OnDamageTaken;
    public UnityEvent<Vector3, float> OnKnockback;
    public UnityEvent OnDeath;

    // Components
    private Renderer meshRenderer;
    private Material originalMaterial;
    private Rigidbody rb;
    private Animator animator;

    // State
    private bool isDead = false;
    private bool isKnockedBack = false;

    // UI
    private GameObject healthBarInstance;

    // Animation Parameters
    private readonly int hitTriggerHash = Animator.StringToHash("Hit");
    private readonly int deathTriggerHash = Animator.StringToHash("Death");
    private readonly int isDeadHash = Animator.StringToHash("IsDead");

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        CreateHealthBar();
    }

    private void OnDestroy()
    {
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        meshRenderer = GetComponentInChildren<Renderer>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }

        // Rigidbody 설정 확인
        if (rb != null && canBeKnockedBack)
        {
            rb.isKinematic = false;
        }
    }

    private void CreateHealthBar()
    {
        if (healthBarPrefab == null) return;

        // 체력바 위치 설정
        if (healthBarPosition == null)
        {
            GameObject hpPos = new GameObject("HealthBarPosition");
            hpPos.transform.SetParent(transform);
            hpPos.transform.localPosition = Vector3.up * 2f;
            healthBarPosition = hpPos.transform;
        }

        // 체력바 생성
        healthBarInstance = Instantiate(healthBarPrefab, healthBarPosition.position, Quaternion.identity);
        healthBarInstance.transform.SetParent(healthBarPosition);

        // 체력바 초기화
        UpdateHealthBar();
    }

    #endregion

    #region IDamageable Implementation

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) return;

        // 데미지 적용
        float actualDamage = damage;
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"{gameObject.name}이(가) {actualDamage} 데미지를 받았습니다. 남은 체력: {currentHealth}/{maxHealth}");

        // 시각적 피드백
        ShowDamageEffect();
        ShowDamageNumber(actualDamage);

        // 히트 애니메이션
        PlayHitAnimation();

        // 이벤트 발생
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        OnDamageTaken?.Invoke(actualDamage);

        // 체력바 업데이트
        UpdateHealthBar();

        // 체력이 0 이하면 사망
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    #endregion

    #region IKnockbackable Implementation

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (isDead || !canBeKnockedBack || isKnockedBack) return;

        // 저항력 적용
        float actualForce = force / knockbackResistance;

        // Rigidbody가 있으면 물리적 넉백
        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(direction * actualForce, ForceMode.Impulse);
        }

        // 넉백 상태 시작
        StartCoroutine(KnockbackRoutine());

        // 이벤트 발생
        OnKnockback?.Invoke(direction, actualForce);

        Debug.Log($"{gameObject.name}이(가) 넉백되었습니다. (Force: {actualForce})");
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;

        // 넉백 지속 시간 대기
        yield return new WaitForSeconds(knockbackDuration);

        isKnockedBack = false;

        // 속도 감소
        if (rb != null)
        {
            rb.linearVelocity *= 0.5f;
        }
    }

    #endregion

    #region Health System

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log($"{gameObject.name}이(가) 사망했습니다!");

        // 사망 애니메이션
        PlayDeathAnimation();

        // 사망 이펙트
        SpawnDeathEffect();

        // 충돌 비활성화
        DisableColliders();

        // 이벤트 발생
        OnDeath?.Invoke();

        // 체력바 제거
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }

        // 일정 시간 후 오브젝트 제거
        Destroy(gameObject, deathDelay);
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        UpdateHealthBar();

        Debug.Log($"{gameObject.name}이(가) {amount} 체력을 회복했습니다. 현재 체력: {currentHealth}/{maxHealth}");
    }

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    #endregion

    #region Visual Effects

    private void ShowDamageEffect()
    {
        if (meshRenderer != null && damageMaterial != null)
        {
            StartCoroutine(DamageFlashRoutine());
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        // 데미지 머티리얼로 변경
        if (meshRenderer != null)
        {
            meshRenderer.material = damageMaterial;
        }

        // 잠시 대기
        yield return new WaitForSeconds(damageFlashDuration);

        // 원래 머티리얼로 복구
        if (meshRenderer != null && originalMaterial != null && !isDead)
        {
            meshRenderer.material = originalMaterial;
        }
    }

    private void ShowDamageNumber(float damage)
    {
        if (damageNumberPrefab == null) return;

        // 데미지 숫자 생성
        Vector3 spawnPosition = transform.position + Vector3.up * 2f;
        GameObject damageNumber = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);

        // 데미지 값 설정 (DamageNumber 스크립트가 있다고 가정)
        DamageNumber damageNumberScript = damageNumber.GetComponent<DamageNumber>();
        if (damageNumberScript != null)
        {
            damageNumberScript.SetDamage(damage);
        }

        Destroy(damageNumber, 1.5f);
    }

    private void SpawnDeathEffect()
    {
        if (deathEffectPrefab == null) return;

        GameObject effect = Instantiate(
            deathEffectPrefab,
            transform.position,
            Quaternion.identity
        );

        Destroy(effect, 3f);
    }

    private void UpdateHealthBar()
    {
        if (healthBarInstance == null) return;

        // HealthBar 스크립트가 있다고 가정
        HealthBar healthBar = healthBarInstance.GetComponent<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth / maxHealth);
        }
    }

    #endregion

    #region Animation

    private void PlayHitAnimation()
    {
        if (animator != null && !isDead)
        {
            animator.SetTrigger(hitTriggerHash);
        }
    }

    private void PlayDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(deathTriggerHash);
            animator.SetBool(isDeadHash, true);
        }
    }

    #endregion

    #region Helper Methods

    private void DisableColliders()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    #endregion

    #region Public Methods

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsAlive()
    {
        return !isDead && currentHealth > 0;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        // 체력바 위치 표시
        if (healthBarPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(healthBarPosition.position, 0.1f);
        }

        // 체력 정보 표시
        if (Application.isPlaying)
        {
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2.5f,
                $"HP: {currentHealth:F0}/{maxHealth:F0}"
            );
        }
    }

    #endregion
}

/// <summary>
/// 데미지 숫자 표시를 위한 간단한 스크립트 (선택사항)
/// </summary>
public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshPro textMesh;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float fadeSpeed = 1f;

    private float alpha = 1f;

    public void SetDamage(float damage)
    {
        if (textMesh != null)
        {
            textMesh.text = damage.ToString("F0");
        }
    }

    private void Update()
    {
        // 위로 떠오르기
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // 페이드 아웃
        alpha -= fadeSpeed * Time.deltaTime;
        if (textMesh != null)
        {
            Color color = textMesh.color;
            color.a = alpha;
            textMesh.color = color;
        }
    }
}

/// <summary>
/// 체력바 표시를 위한 간단한 스크립트 (선택사항)
/// </summary>
public class HealthBar : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image fillImage;
    [SerializeField] private bool faceCamera = true;

    private Transform cameraTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        // 카메라를 바라보도록
        if (faceCamera && cameraTransform != null)
        {
            transform.LookAt(transform.position + cameraTransform.forward);
        }
    }

    public void SetHealth(float percentage)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(percentage);
        }
    }
}
