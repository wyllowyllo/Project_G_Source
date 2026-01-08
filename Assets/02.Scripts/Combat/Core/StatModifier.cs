using System;
using UnityEngine;

namespace Combat.Core
{
    public enum StatModifierType
    {
        Additive,
        Multiplicative
    }

    public class StatModifier
    {
        public float Value { get; }
        public StatModifierType Type { get; }
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

            Value = value;
            Type = type;
            Source = source;
        }
    }
}
