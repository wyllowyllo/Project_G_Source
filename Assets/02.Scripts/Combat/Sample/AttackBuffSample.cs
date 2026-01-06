using System.Collections;
using Combat.Core;
using UnityEngine;

namespace Combat.Sample
{
    /// <summary>
    /// 스탯 수정자(버프/디버프) 샘플 코드입니다.
    /// 
    /// 사용 예시:
    /// var buff = GetComponent<AttackBuffSample>();
    /// buff.ApplyTo(targetCombatant);
    /// </summary>
    public class AttackBuffSample : MonoBehaviour, IModifierSource
    {
        [Header("Settings")]
        [SerializeField] private float _attackBonus = 0.5f;
        [SerializeField] private float _duration = 5f;
        
        public string Id => $"AttackBuff_{GetInstanceID()}";

        /// <summary>
        /// 대상에게 공격력 버프 적용
        /// </summary>
        public void ApplyTo(Combatant target)
        {
            var modifier = new StatModifier(
                _attackBonus, 
                StatModifierType.Multiplicative, 
                this
            );
            
            target.Stats.AttackDamage.AddModifier(modifier);
            
            Debug.Log($"[Buff] {target.name}에게 공격력 {_attackBonus * 100:F0}% 증가 버프 적용 ({_duration}초)");
            
            StartCoroutine(RemoveAfterDuration(target, _duration));
        }

        private IEnumerator RemoveAfterDuration(Combatant target, float duration)
        {
            yield return new WaitForSeconds(duration);
            
            if (target != null)
            {
                target.Stats.RemoveAllModifiersFromSource(this);
                Debug.Log($"[Buff] {target.name}의 공격력 버프 종료");
            }
        }

        /// <summary>
        /// 대상에게서 이 버프 제거
        /// </summary>
        public void RemoveFrom(Combatant target)
        {
            target.Stats.RemoveAllModifiersFromSource(this);
        }
    }
}
