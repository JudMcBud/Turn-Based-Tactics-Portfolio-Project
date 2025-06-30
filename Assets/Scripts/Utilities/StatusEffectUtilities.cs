using UnityEngine;

/// <summary>
/// Utility class for status effect calculations and modifiers
/// Consolidates the scattered modifier logic from Unit class
/// </summary>
public static class StatusEffectUtilities
{
    /// <summary>
    /// Default buff multiplier for positive effects
    /// </summary>
    public const float BUFF_MULTIPLIER = 1.2f;

    /// <summary>
    /// Default poison damage per turn
    /// </summary>
    public const int POISON_DAMAGE_PER_TURN = 5;

    /// <summary>
    /// Applies status effect modifiers to a base stat value
    /// </summary>
    /// <param name="baseValue">The base stat value</param>
    /// <param name="isBuffed">Whether the unit is buffed</param>
    /// <param name="isStunned">Whether the unit is stunned (for movement stats)</param>
    /// <param name="statType">The type of stat being modified</param>
    /// <returns>Modified stat value</returns>
    public static int ApplyStatusModifiers(int baseValue, bool isBuffed, bool isStunned = false, StatType statType = StatType.Combat)
    {
        int modifiedValue = baseValue;

        // Apply buff effects
        if (isBuffed && statType != StatType.Movement)
        {
            modifiedValue = Mathf.RoundToInt(modifiedValue * BUFF_MULTIPLIER);
        }

        // Apply stun effects (only affects movement)
        if (isStunned && statType == StatType.Movement)
        {
            modifiedValue = 0;
        }

        return modifiedValue;
    }

    /// <summary>
    /// Processes status effects that trigger each turn
    /// </summary>
    /// <param name="unit">The unit to process effects for</param>
    /// <param name="isPoisoned">Whether the unit is poisoned</param>
    /// <returns>True if any effects were processed</returns>
    public static bool ProcessTurnEffects(Unit unit, bool isPoisoned)
    {
        bool effectsProcessed = false;

        if (isPoisoned)
        {
            unit.TakeDamage(POISON_DAMAGE_PER_TURN, DamageType.Magical);
            Debug.Log($"{unit.unitName} takes poison damage!");
            effectsProcessed = true;
        }

        return effectsProcessed;
    }

    /// <summary>
    /// Gets a description of all active status effects
    /// </summary>
    /// <param name="isStunned">Whether stunned</param>
    /// <param name="isPoisoned">Whether poisoned</param>
    /// <param name="isBuffed">Whether buffed</param>
    /// <returns>String description of active effects</returns>
    public static string GetStatusDescription(bool isStunned, bool isPoisoned, bool isBuffed)
    {
        var effects = new System.Collections.Generic.List<string>();

        if (isStunned) effects.Add("Stunned");
        if (isPoisoned) effects.Add("Poisoned");
        if (isBuffed) effects.Add("Buffed");

        return effects.Count > 0 ? string.Join(", ", effects) : "None";
    }
}

/// <summary>
/// Enum to categorize different types of stats for modifier application
/// </summary>
public enum StatType
{
    Combat,    // Attack, Defense, Resistance, Speed
    Movement   // Movement Distance
}
