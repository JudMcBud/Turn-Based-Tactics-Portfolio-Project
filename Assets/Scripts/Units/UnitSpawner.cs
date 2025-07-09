using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Advanced unit spawning tool for testing and level design
/// Allows spawning units at specific cells with custom stats and factions
/// </summary>
public class UnitSpawner : MonoBehaviour
{
    [Header("Spawn Configuration")]
    [SerializeField] private bool autoAssignManagers = true;
    [SerializeField] private bool spawnOnStart = false;
    [SerializeField] private bool showDebugInfo = true;

    [Header("Target Cell Selection")]
    [SerializeField] private int targetGridX = 0;
    [SerializeField] private int targetGridY = 0;
    [SerializeField] private bool useWorldPosition = false;
    [SerializeField] private Vector3 targetWorldPosition = Vector3.zero;

    [Header("Unit Configuration")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Team selectedFaction = Team.Player;
    [SerializeField] private UnitType selectedUnitType = UnitType.Infantry;
    [SerializeField] private string customUnitName = "Spawned Unit";

    [Header("Unit Stats")]
    [SerializeField] private UnitStats unitStats = new UnitStats();

    [Header("Visual Configuration")]
    [SerializeField] private Material playerMaterial;
    [SerializeField] private Material enemyMaterial;
    [SerializeField] private Material neutralMaterial;
    [SerializeField] private bool autoColorByFaction = true;

    [Header("Batch Spawning")]
    [SerializeField] private bool enableBatchSpawn = false;
    [SerializeField] private List<SpawnRequest> batchSpawnList = new List<SpawnRequest>();

    [Header("Preview")]
    [SerializeField] private bool showPreview = true;
    [SerializeField] private Color previewColor = Color.cyan;

    // References (auto-assigned)
    private GridManager gridManager;
    private SelectionManager selectionManager;
    private Camera mainCamera;

    // Spawned units tracking
    private List<Unit> spawnedUnits = new List<Unit>();

    private void Start()
    {
        InitializeReferences();

        if (spawnOnStart)
        {
            if (enableBatchSpawn && batchSpawnList.Count > 0)
            {
                SpawnBatchUnits();
            }
            else
            {
                SpawnUnit();
            }
        }
    }

    private void InitializeReferences()
    {
        if (autoAssignManagers)
        {
            gridManager = ComponentFinder.GetGridManager();
            selectionManager = ComponentFinder.GetSelectionManager();
            mainCamera = ComponentFinder.GetMainCamera();

            if (showDebugInfo)
            {
                bool allFound = gridManager != null && selectionManager != null && mainCamera != null;
                Debug.Log($"[UnitSpawner] Auto-assigned managers: {(allFound ? "Success" : "Some missing")}");
            }
        }
    }

    /// <summary>
    /// Spawns a unit with the current inspector settings
    /// </summary>
    [ContextMenu("Spawn Unit")]
    public void SpawnUnit()
    {
        if (!ValidateSpawnConditions())
            return;

        Cell targetCell = GetTargetCell();
        if (targetCell == null)
        {
            Debug.LogError($"[UnitSpawner] Cannot spawn unit - target cell not found at ({targetGridX}, {targetGridY})");
            return;
        }

        if (targetCell.isOccupied)
        {
            Debug.LogWarning($"[UnitSpawner] Target cell ({targetGridX}, {targetGridY}) is already occupied!");
            return;
        }

        Unit spawnedUnit = CreateUnit(targetCell, unitStats, selectedFaction, selectedUnitType, customUnitName);

        if (spawnedUnit != null)
        {
            spawnedUnits.Add(spawnedUnit);

            if (showDebugInfo)
            {
                Debug.Log($"[UnitSpawner] Successfully spawned {spawnedUnit.unitName} ({selectedFaction}) at ({targetGridX}, {targetGridY})");
            }
        }
    }

    /// <summary>
    /// Spawns multiple units from the batch list
    /// </summary>
    [ContextMenu("Spawn Batch Units")]
    public void SpawnBatchUnits()
    {
        if (!enableBatchSpawn || batchSpawnList.Count == 0)
        {
            Debug.LogWarning("[UnitSpawner] Batch spawn is disabled or list is empty");
            return;
        }

        int successCount = 0;
        foreach (SpawnRequest request in batchSpawnList)
        {
            if (SpawnUnitFromRequest(request))
            {
                successCount++;
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"[UnitSpawner] Batch spawn complete: {successCount}/{batchSpawnList.Count} units spawned");
        }
    }

    /// <summary>
    /// Creates a unit at the specified cell with given parameters
    /// </summary>
    private Unit CreateUnit(Cell targetCell, UnitStats stats, Team faction, UnitType unitType, string unitName)
    {
        if (unitPrefab == null)
        {
            Debug.LogError("[UnitSpawner] No unit prefab assigned!");
            return null;
        }

        // Calculate spawn position (slightly above the cell)
        Vector3 spawnPosition = targetCell.GetWorldPosition() + Vector3.up * 0.5f;

        // Instantiate the unit
        GameObject unitObject = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
        unitObject.name = $"{unitName}_{faction}_{targetCell.gridX}_{targetCell.gridY}";

        // Get the Unit component
        Unit unit = unitObject.GetComponent<Unit>();
        if (unit == null)
        {
            Debug.LogError("[UnitSpawner] Unit prefab must have a Unit component!");
            Destroy(unitObject);
            return null;
        }

        // Configure the unit
        ConfigureUnit(unit, stats, faction, unitType, unitName);

        // Set grid position
        unit.SetGridPosition(targetCell.gridX, targetCell.gridY);

        // Apply visual configuration
        ApplyVisualConfiguration(unit, faction);

        return unit;
    }

    /// <summary>
    /// Configures a unit with the specified parameters
    /// </summary>
    private void ConfigureUnit(Unit unit, UnitStats stats, Team faction, UnitType unitType, string unitName)
    {
        unit.unitName = unitName;
        unit.team = faction;
        unit.unitType = unitType;

        // Assign managers
        unit.gridManager = gridManager;
        unit.selectionManager = selectionManager;

        // Apply stats using the public method instead of reflection
        unit.SetStats(
            stats.maxHealthPoints,
            stats.attackPower,
            stats.defense,
            stats.resistance,
            stats.speed,
            stats.movementDistance
        );
    }

    /// <summary>
    /// Applies visual configuration based on faction
    /// </summary>
    private void ApplyVisualConfiguration(Unit unit, Team faction)
    {
        if (!autoColorByFaction) return;

        Renderer unitRenderer = unit.GetComponent<Renderer>();
        if (unitRenderer == null)
            unitRenderer = unit.GetComponentInChildren<Renderer>();

        if (unitRenderer != null)
        {
            Material materialToApply = null;
            switch (faction)
            {
                case Team.Player:
                    materialToApply = playerMaterial;
                    break;
                case Team.Enemy:
                    materialToApply = enemyMaterial;
                    break;
                case Team.Neutral:
                    materialToApply = neutralMaterial;
                    break;
            }

            if (materialToApply != null)
            {
                unitRenderer.material = materialToApply;
            }
            else
            {
                // Fallback to color coding
                Color factionColor = GetFactionColor(faction);
                unitRenderer.material.color = factionColor;
            }
        }
    }

    /// <summary>
    /// Gets the default color for a faction
    /// </summary>
    private Color GetFactionColor(Team faction)
    {
        switch (faction)
        {
            case Team.Player: return Color.blue;
            case Team.Enemy: return Color.red;
            case Team.Neutral: return Color.gray;
            default: return Color.white;
        }
    }

    /// <summary>
    /// Spawns a unit from a batch spawn request
    /// </summary>
    private bool SpawnUnitFromRequest(SpawnRequest request)
    {
        Cell targetCell = gridManager.GetCell(request.gridX, request.gridY);
        if (targetCell == null || targetCell.isOccupied)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"[UnitSpawner] Cannot spawn {request.unitName} at ({request.gridX}, {request.gridY}) - cell invalid or occupied");
            }
            return false;
        }

        Unit spawnedUnit = CreateUnit(targetCell, request.stats, request.faction, request.unitType, request.unitName);
        if (spawnedUnit != null)
        {
            spawnedUnits.Add(spawnedUnit);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the target cell based on current settings
    /// </summary>
    private Cell GetTargetCell()
    {
        if (gridManager == null) return null;

        if (useWorldPosition)
        {
            return gridManager.GetCellAtWorldPosition(targetWorldPosition);
        }
        else
        {
            return gridManager.GetCell(targetGridX, targetGridY);
        }
    }

    /// <summary>
    /// Validates that spawning conditions are met
    /// </summary>
    private bool ValidateSpawnConditions()
    {
        if (unitPrefab == null)
        {
            Debug.LogError("[UnitSpawner] No unit prefab assigned!");
            return false;
        }

        if (gridManager == null)
        {
            Debug.LogError("[UnitSpawner] GridManager not found!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Clears all spawned units
    /// </summary>
    [ContextMenu("Clear Spawned Units")]
    public void ClearSpawnedUnits()
    {
        foreach (Unit unit in spawnedUnits)
        {
            if (unit != null)
            {
                DestroyImmediate(unit.gameObject);
            }
        }
        spawnedUnits.Clear();

        if (showDebugInfo)
        {
            Debug.Log("[UnitSpawner] All spawned units cleared");
        }
    }

    /// <summary>
    /// Selects the cell at current target coordinates (for visualization)
    /// </summary>
    [ContextMenu("Select Target Cell")]
    public void SelectTargetCell()
    {
        Cell targetCell = GetTargetCell();
        if (targetCell != null && selectionManager != null)
        {
            selectionManager.SelectCell(targetCell);
            Debug.Log($"[UnitSpawner] Selected target cell at ({targetCell.gridX}, {targetCell.gridY})");
        }
    }

    /// <summary>
    /// Sets target position based on currently selected cell
    /// </summary>
    [ContextMenu("Use Selected Cell as Target")]
    public void UseSelectedCellAsTarget()
    {
        if (selectionManager != null && selectionManager.CurrentSelectedCell != null)
        {
            Cell selectedCell = selectionManager.CurrentSelectedCell;
            targetGridX = selectedCell.gridX;
            targetGridY = selectedCell.gridY;
            useWorldPosition = false;

            if (showDebugInfo)
            {
                Debug.Log($"[UnitSpawner] Target set to selected cell: ({targetGridX}, {targetGridY})");
            }
        }
        else
        {
            Debug.LogWarning("[UnitSpawner] No cell currently selected");
        }
    }

    // Gizmo drawing for preview
    private void OnDrawGizmos()
    {
        if (!showPreview || gridManager == null) return;

        Cell targetCell = GetTargetCell();
        if (targetCell != null)
        {
            Gizmos.color = previewColor;
            Vector3 cellPos = targetCell.GetWorldPosition();
            Gizmos.DrawWireCube(cellPos + Vector3.up * 0.5f, new Vector3(0.8f, 1f, 0.8f));

            // Draw faction color indicator
            Gizmos.color = GetFactionColor(selectedFaction);
            Gizmos.DrawSphere(cellPos + Vector3.up * 1.5f, 0.2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showPreview) return;

        // Draw batch spawn previews
        if (enableBatchSpawn && batchSpawnList.Count > 0 && gridManager != null)
        {
            foreach (SpawnRequest request in batchSpawnList)
            {
                Cell cell = gridManager.GetCell(request.gridX, request.gridY);
                if (cell != null)
                {
                    Gizmos.color = GetFactionColor(request.faction);
                    Gizmos.DrawWireSphere(cell.GetWorldPosition() + Vector3.up * 0.5f, 0.3f);
                }
            }
        }
    }
}
