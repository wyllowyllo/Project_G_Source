using UnityEngine;

namespace Combat.Data
{
    [CreateAssetMenu(fileName = "ComboSettings", menuName = "Combat/Combo Settings")]
    public class ComboSettings : ScriptableObject
    {
        [Header("Combo")]
        [SerializeField, Range(1, 5)] private int _maxComboSteps = 3;
        [SerializeField, Range(0.1f, 2f)] private float _comboWindowDuration = 0.8f;

        [Header("Combo Damage Multipliers")]
        [SerializeField] private float[] _comboDamageMultipliers = { 1.0f, 1.1f, 1.3f };

        public int MaxComboSteps => _maxComboSteps;
        public float ComboWindowDuration => _comboWindowDuration;

        public float GetComboMultiplier(int step)
        {
            if (step < 1 || _comboDamageMultipliers == null || _comboDamageMultipliers.Length == 0)
                return 1f;

            int index = Mathf.Clamp(step - 1, 0, _comboDamageMultipliers.Length - 1);
            return _comboDamageMultipliers[index];
        }
    }
}
