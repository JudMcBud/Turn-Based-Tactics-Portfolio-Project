using UnityEngine;

/// <summary>
/// Serializable stats container for unit configuration
/// Used for spawning units and configuring their stats
/// </summary>
[System.Serializable]
public class UnitStats
{
    [Header("Health")]
    public int maxHealthPoints = 100;

    [Header("Combat Stats")]
    public int attackPower = 20;
    public int defense = 10;
    public int resistance = 10;

    [Header("Movement")]
    public int speed = 15;
    public int movementDistance = 2;

    public UnitStats()
    {
        // Default constructor with default values
    }

    public UnitStats(int health, int attack, int def, int res, int spd, int movement)
    {
        maxHealthPoints = health;
        attackPower = attack;
        defense = def;
        resistance = res;
        speed = spd;
        movementDistance = movement;
    }

    /// <summary>
    /// Creates a copy of these stats
    /// </summary>
    public UnitStats Clone()
    {
        return new UnitStats(maxHealthPoints, attackPower, defense, resistance, speed, movementDistance);
    }

    /// <summary>
    /// Returns a string representation of the stats
    /// </summary>
    public override string ToString()
    {
        return $"HP:{maxHealthPoints} ATK:{attackPower} DEF:{defense} RES:{resistance} SPD:{speed} MOV:{movementDistance}";
    }

    /// <summary>
    /// Preset stat configurations
    /// </summary>
    public static class Presets
    {
        public static UnitStats Weak => new UnitStats(50, 10, 5, 5, 10, 1);
        public static UnitStats Normal => new UnitStats(100, 20, 10, 10, 15, 2);
        public static UnitStats Strong => new UnitStats(150, 30, 15, 15, 20, 3);
        public static UnitStats Boss => new UnitStats(300, 50, 25, 25, 10, 2);
        public static UnitStats Scout => new UnitStats(80, 15, 8, 8, 25, 4);
        public static UnitStats Tank => new UnitStats(200, 25, 20, 15, 8, 1);
        public static UnitStats Mage => new UnitStats(70, 35, 5, 20, 12, 2);
        public static UnitStats Healer => new UnitStats(90, 10, 10, 25, 14, 2);
    }
}
