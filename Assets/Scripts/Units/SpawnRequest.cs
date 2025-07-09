using UnityEngine;

/// <summary>
/// Batch spawn request for spawning multiple units
/// Contains all information needed to spawn a unit at a specific location
/// </summary>
[System.Serializable]
public class SpawnRequest
{
    [Header("Position")]
    public int gridX;
    public int gridY;

    [Header("Unit Configuration")]
    public string unitName = "Batch Unit";
    public Team faction = Team.Player;
    public UnitType unitType = UnitType.Infantry;

    [Header("Stats")]
    public UnitStats stats = new UnitStats();

    [Header("Optional Overrides")]
    public GameObject customPrefab;
    public Material customMaterial;

    public SpawnRequest()
    {
        // Default constructor
    }

    public SpawnRequest(int x, int y, string name, Team team, UnitType type, UnitStats unitStats)
    {
        gridX = x;
        gridY = y;
        unitName = name;
        faction = team;
        unitType = type;
        stats = unitStats != null ? unitStats.Clone() : new UnitStats();
    }

    /// <summary>
    /// Creates a spawn request with preset stats
    /// </summary>
    public static SpawnRequest CreateWithPreset(int x, int y, string name, Team team, UnitType type, string presetName)
    {
        UnitStats presetStats = presetName.ToLower() switch
        {
            "weak" => UnitStats.Presets.Weak,
            "normal" => UnitStats.Presets.Normal,
            "strong" => UnitStats.Presets.Strong,
            "boss" => UnitStats.Presets.Boss,
            "scout" => UnitStats.Presets.Scout,
            "tank" => UnitStats.Presets.Tank,
            "mage" => UnitStats.Presets.Mage,
            "healer" => UnitStats.Presets.Healer,
            _ => UnitStats.Presets.Normal
        };

        return new SpawnRequest(x, y, name, team, type, presetStats);
    }

    public override string ToString()
    {
        return $"{unitName} ({faction}) at ({gridX}, {gridY}) - {stats}";
    }
}
