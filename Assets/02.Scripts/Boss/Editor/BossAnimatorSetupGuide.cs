#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Boss.Editor
{
    /// <summary>
    /// 보스 Animator Controller 설정 가이드
    /// 메뉴에서 참조 문서 확인 가능
    /// </summary>
    public static class BossAnimatorSetupGuide
    {
        [MenuItem("ProjectG/Boss/Show Animator Setup Guide")]
        public static void ShowGuide()
        {
            EditorUtility.DisplayDialog(
                "Boss Animator Setup Guide",
                GUIDE_TEXT,
                "OK"
            );
        }

        private const string GUIDE_TEXT = @"=== Boss Animator Controller Setup ===

[Parameters - Float]
• Speed (float): 이동 속도
• MoveX (float): X축 이동 방향
• MoveY (float): Y축 이동 방향

[Parameters - Bool]
• InCombat (bool): 전투 모드 여부

[Parameters - Trigger]
• MeleeAttack: 근접 공격 (Attack01)
• Charge: 돌진 공격
• Breath: 브레스 공격 (Attack03)
• Projectile: 투사체 발사 (Attack02)
• Summon: 잡졸 소환 (Taunting)
• Stagger: 그로기 (Dizzy)
• Hit: 피격 (GetHit)
• Death: 사망 (Die)
• PhaseTransition: 페이즈 전환 (Victory/Taunting)

[Animation Events - 필수]

근접 공격 (Attack01):
  • EnableMeleeHitbox() - 공격 시작
  • DisableMeleeHitbox() - 공격 종료
  • OnMeleeAttackComplete() - 애니메이션 완료

돌진 공격:
  • EnableChargeHitbox() - 돌진 시작
  • DisableChargeHitbox() - 돌진 종료
  • OnChargeComplete() - 애니메이션 완료

브레스 공격 (Attack03):
  • StartBreath() - 브레스 시작
  • StopBreath() - 브레스 종료
  • OnBreathComplete() - 애니메이션 완료

투사체 발사 (Attack02):
  • FireProjectile() - 단일 발사
  • FireAllProjectiles() - 전체 발사
  • OnProjectileComplete() - 애니메이션 완료

소환 (Taunting):
  • SpawnMinions() - 소환 실행
  • OnSummonComplete() - 애니메이션 완료

상태 변화:
  • OnStaggerComplete() - 그로기 완료
  • OnHitComplete() - 피격 완료
  • OnDeathComplete() - 사망 완료
  • OnPhaseTransitionComplete() - 페이즈 전환 완료

[State Machine 구조 권장]

Base Layer:
  ├── IdleBattle (기본)
  ├── Attack01 (MeleeAttack)
  ├── Attack02 (Projectile)
  ├── Attack03 (Breath)
  ├── RunFWD + Attack01 (Charge)
  ├── Taunting (Summon/PhaseTransition)
  ├── Dizzy (Stagger)
  ├── GetHit (Hit)
  └── Die (Dead)

[주의사항]
• BossAnimationEventReceiver를 Animator가 있는 오브젝트에 부착
• 모든 공격 애니메이션은 Root Motion 사용 권장
• Charge는 코드에서 이동 처리 (Root Motion X)";

        [MenuItem("ProjectG/Boss/Create Animator Controller Template")]
        public static void CreateAnimatorTemplate()
        {
            // AnimatorController 생성
            var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(
                "Assets/Animations/Boss/BossAnimatorController.controller"
            );

            // 파라미터 추가
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            controller.AddParameter("InCombat", AnimatorControllerParameterType.Bool);

            controller.AddParameter("MeleeAttack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Charge", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Breath", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Projectile", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Summon", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Stagger", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("PhaseTransition", AnimatorControllerParameterType.Trigger);

            AssetDatabase.SaveAssets();
            Selection.activeObject = controller;

            EditorUtility.DisplayDialog(
                "Success",
                "Animator Controller 템플릿이 생성되었습니다.\n경로: Assets/Animations/Boss/BossAnimatorController.controller\n\n파라미터가 자동으로 추가되었습니다. 상태와 전이를 수동으로 설정해주세요.",
                "OK"
            );
        }
    }
}
#endif
