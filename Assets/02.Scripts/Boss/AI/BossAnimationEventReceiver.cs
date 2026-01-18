using Boss.AI.States;
using Boss.Combat;
using UnityEngine;

namespace Boss.AI
{
    /// <summary>
    /// 애니메이션 이벤트를 수신하여 BossController에 전달하는 브릿지 컴포넌트
    /// Animator가 있는 오브젝트(Body 하위)에 부착
    /// </summary>
    public class BossAnimationEventReceiver : MonoBehaviour
    {
        [Header("자동 검색 (비워두면 부모에서 검색)")]
        [SerializeField] private BossController _controller;
        [SerializeField] private BossBreathAttacker _breathAttacker;
        [SerializeField] private BossProjectileLauncher _projectileLauncher;

        private void Awake()
        {
            // 자동 검색
            if (_controller == null)
            {
                _controller = GetComponentInParent<BossController>();
            }

            if (_breathAttacker == null)
            {
                _breathAttacker = GetComponentInParent<BossBreathAttacker>();
            }

            if (_projectileLauncher == null)
            {
                _projectileLauncher = GetComponentInParent<BossProjectileLauncher>();
            }

            if (_controller == null)
            {
                Debug.LogError($"{gameObject.name}: BossController를 찾을 수 없습니다.");
            }
        }

        #region 근접 공격 이벤트

        /// <summary>
        /// Animation Event: 근접 공격 히트박스 활성화
        /// </summary>
        public void EnableMeleeHitbox()
        {
            _controller?.EnableMeleeHitbox();
        }

        /// <summary>
        /// Animation Event: 근접 공격 히트박스 비활성화
        /// </summary>
        public void DisableMeleeHitbox()
        {
            _controller?.DisableMeleeHitbox();
        }

        /// <summary>
        /// Animation Event: 근접 공격 애니메이션 완료
        /// </summary>
        public void OnMeleeAttackComplete()
        {
            _controller?.OnMeleeAttackAnimationComplete();
        }

        #endregion

        #region 돌진 공격 이벤트

        /// <summary>
        /// Animation Event: 돌진 히트박스 활성화
        /// </summary>
        public void EnableChargeHitbox()
        {
            _controller?.EnableChargeHitbox();
        }

        /// <summary>
        /// Animation Event: 돌진 히트박스 비활성화
        /// </summary>
        public void DisableChargeHitbox()
        {
            _controller?.DisableChargeHitbox();
        }

        /// <summary>
        /// Animation Event: 돌진 애니메이션 완료
        /// </summary>
        public void OnChargeComplete()
        {
            _controller?.OnChargeAnimationComplete();
        }

        #endregion

        #region 브레스 공격 이벤트

        /// <summary>
        /// Animation Event: 브레스 공격 시작
        /// </summary>
        public void StartBreath()
        {
            _controller?.StartBreathAttack();
        }

        /// <summary>
        /// Animation Event: 브레스 공격 종료
        /// </summary>
        public void StopBreath()
        {
            _controller?.StopBreathAttack();
        }

        /// <summary>
        /// Animation Event: 브레스 애니메이션 완료
        /// </summary>
        public void OnBreathComplete()
        {
            _controller?.OnBreathAnimationComplete();
        }

        #endregion

        #region 투사체 공격 이벤트

        /// <summary>
        /// Animation Event: 단일 투사체 발사
        /// </summary>
        public void FireProjectile()
        {
            _controller?.FireProjectile();
        }

        /// <summary>
        /// Animation Event: 모든 투사체 동시 발사
        /// </summary>
        public void FireAllProjectiles()
        {
            if (_controller == null) return;
            int count = _controller.Data.ProjectileCount;
            for (int i = 0; i < count; i++)
            {
                _controller.FireProjectile(i, count);
            }
        }

        /// <summary>
        /// Animation Event: 투사체 애니메이션 완료
        /// </summary>
        public void OnProjectileComplete()
        {
            _controller?.OnProjectileAnimationComplete();
        }

        #endregion

        #region 소환 이벤트

        /// <summary>
        /// Animation Event: 잡졸 소환 실행
        /// </summary>
        public void SpawnMinions()
        {
            _controller?.SpawnMinions();
        }

        /// <summary>
        /// Animation Event: 소환 애니메이션 완료
        /// </summary>
        public void OnSummonComplete()
        {
            Debug.Log("[BossAnimationEventReceiver] SummonComplete");
            _controller?.OnSummonAnimationComplete();
        }

        #endregion

        #region 상태 변화 이벤트

        /// <summary>
        /// Animation Event: 피격 애니메이션 완료
        /// </summary>
        public void OnHitComplete()
        {
            _controller?.OnHitAnimationComplete();
        }

        /// <summary>
        /// Animation Event: 사망 애니메이션 완료
        /// </summary>
        public void OnDeathComplete()
        {
            _controller?.OnDeathAnimationComplete();
        }

        /// <summary>
        /// Animation Event: 페이즈 전환 애니메이션 완료
        /// </summary>
        public void OnPhaseTransitionComplete()
        {
            Debug.Log("[BossAnimationEventReceiver] PhaseTransitionComplete");
            _controller?.OnPhaseTransitionAnimationComplete();
        }

        #endregion

        #region 범용 이벤트

        /// <summary>
        /// Animation Event: 사운드 재생 (선택적)
        /// </summary>
        public void PlaySound(string soundName)
        {
            // AudioManager 연동 시 구현
            Debug.Log($"[BossAnimationEvent] PlaySound: {soundName}");
        }

        /// <summary>
        /// Animation Event: 이펙트 재생 (선택적)
        /// </summary>
        public void PlayEffect(string effectName)
        {
            // EffectManager 연동 시 구현
            Debug.Log($"[BossAnimationEvent] PlayEffect: {effectName}");
        }

        /// <summary>
        /// Animation Event: 카메라 흔들림 (선택적)
        /// </summary>
        public void CameraShake(float intensity)
        {
            // CameraManager 연동 시 구현
            Debug.Log($"[BossAnimationEvent] CameraShake: {intensity}");
        }

        /// <summary>
        /// Animation Event: 미사용 이벤트 (에러 방지용)
        /// </summary>
        public void NewEvent()
        {
            // 애니메이션에 남아있는 미사용 이벤트 처리
        }

        #endregion
    }
}
