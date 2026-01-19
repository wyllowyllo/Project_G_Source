using System;
using UnityEngine;

namespace Combat.Core
{
    public enum StatModifierType
    {
        Additive,
        Multiplicative
    }

    [Serializable]
    public class StatModifier
    {
        [SerializeField] private float _value;
        [SerializeField] private StatModifierType _type;
        
        public float Value => _value;
        public StatModifierType Type => _type;

        public IModifierSource Source { get; }

        public StatModifier(float value, StatModifierType type, IModifierSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "[StatModifier] Source cannot be null");

            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                Debug.LogWarning("[StatModifier] Invalid value (NaN or Infinity), defaulting to 0");
                value = 0f;
            }

            _value = value;
            _type = type;
            Source = source;
        }
    }
}
