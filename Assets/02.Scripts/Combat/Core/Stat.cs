using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Combat.Core
{
    [Serializable]
    public class Stat
    {
        [SerializeField] private float _baseValue;
        private readonly float _minValue;
        private readonly float _maxValue;
        [SerializeField] private List<StatModifier> _modifiers = new();

        public float BaseValue
        {
            get => _baseValue;
            set => _baseValue = value;
        }

        public float Value => Clamp(CalculateFinalValue());

        public Stat(float baseValue, float minValue = float.MinValue, float maxValue = float.MaxValue)
        {
            _baseValue = baseValue;
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public void AddModifier(StatModifier modifier)
        {
            if (modifier == null)
            {
                Debug.LogWarning("[Stat] Cannot add null modifier");
                return;
            }
            _modifiers.Add(modifier);
        }

        public bool RemoveModifier(StatModifier modifier)
        {
            if (modifier == null)
                return false;

            return _modifiers.Remove(modifier);
        }

        public bool RemoveAllModifiersFromSource(IModifierSource source)
        {
            if (source == null)
                return false;

            return _modifiers.RemoveAll(m => m.SourceId == source.Id) > 0;
        }

        public void ClearModifiers()
        {
            _modifiers.Clear();
        }

        private float CalculateFinalValue()
        {
            float additive = _modifiers
                .Where(m => m.Type == StatModifierType.Additive)
                .Sum(m => m.Value);

            float multiplicative = _modifiers
                .Where(m => m.Type == StatModifierType.Multiplicative)
                .Sum(m => m.Value);

            return (_baseValue + additive) * (1f + multiplicative);
        }

        private float Clamp(float value)
        {
            return Mathf.Clamp(value, _minValue, _maxValue);
        }
    }
}
