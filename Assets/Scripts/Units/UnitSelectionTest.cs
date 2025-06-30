using UnityEngine;

/// <summary>
/// Test script to verify unit selection and movement functionality
/// </summary>
public class UnitSelectionTest : MonoBehaviour
{
    [Header("Test Settings")]
    public GameObject unitPrefab;
    public GridManager gridManager;
    public SelectionManager selectionManager;
    public TacticsInputManager inputManager;

    [Header("Test Configuration")]
    public int testUnitsToSpawn = 3;
    public bool spawnUnitsOnStart = true;
    public bool showTestInstructions = true;

    private Unit[] testUnits;

    private void Start()
    {
        // Find managers if not assigned
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();
        if (selectionManager == null)
            selectionManager = FindFirstObjectByType<SelectionManager>();
        if (inputManager == null)
            inputManager = FindFirstObjectByType<TacticsInputManager>();

        // Subscribe to selection events for testing
        if (selectionManager != null)
        {
            selectionManager.OnUnitSelected += OnUnitSelected;
            selectionManager.OnUnitDeselected += OnUnitDeselected;
            selectionManager.OnCellSelected += OnCellSelected;
            selectionManager.OnSelectionCleared += OnSelectionCleared;
        }

        // Spawn test units
        if (spawnUnitsOnStart)
        {
            SpawnTestUnits();
        }

        // Show instructions
        if (showTestInstructions)
        {
            Debug.Log("=== UNIT SELECTION TEST ===");
            Debug.Log("1. Click on a unit to select it");
            Debug.Log("2. Click on a highlighted cell to move the unit");
            Debug.Log("3. Click on empty space to deselect");
            Debug.Log("4. Only player units (blue) can be selected");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (selectionManager != null)
        {
            selectionManager.OnUnitSelected -= OnUnitSelected;
            selectionManager.OnUnitDeselected -= OnUnitDeselected;
            selectionManager.OnCellSelected -= OnCellSelected;
            selectionManager.OnSelectionCleared -= OnSelectionCleared;
        }
    }

    [ContextMenu("Spawn Test Units")]
    public void SpawnTestUnits()
    {
        if (unitPrefab == null || gridManager == null)
        {
            Debug.LogError("Missing unitPrefab or gridManager for spawning test units!");
            return;
        }

        testUnits = new Unit[testUnitsToSpawn];

        for (int i = 0; i < testUnitsToSpawn; i++)
        {
            // Spawn unit at different positions
            Vector3 spawnPosition = new Vector3(i * 2, 0, 0);
            GameObject unitObject = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
            unitObject.name = $"TestUnit_{i}";

            Unit unit = unitObject.GetComponent<Unit>();
            if (unit != null)
            {
                unit.unitName = $"Test Unit {i + 1}";
                unit.team = Team.Player; // Make them player units so they can be selected
                unit.gridManager = gridManager;
                unit.selectionManager = selectionManager;

                testUnits[i] = unit;

                Debug.Log($"Spawned {unit.unitName} at position {spawnPosition}");
            }
        }
    }

    [ContextMenu("Clear Test Units")]
    public void ClearTestUnits()
    {
        if (testUnits != null)
        {
            foreach (Unit unit in testUnits)
            {
                if (unit != null)
                {
                    DestroyImmediate(unit.gameObject);
                }
            }
            testUnits = null;
        }
    }

    // Event handlers for testing
    private void OnUnitSelected(Unit selectedUnit)
    {
        Debug.Log($"✓ Unit Selected: {selectedUnit.unitName} at ({selectedUnit.GridPosition.x}, {selectedUnit.GridPosition.y})");
    }

    private void OnUnitDeselected(Unit deselectedUnit)
    {
        Debug.Log($"✗ Unit Deselected: {deselectedUnit.unitName}");
    }

    private void OnCellSelected(Cell selectedCell)
    {
        Debug.Log($"✓ Cell Selected: ({selectedCell.gridX}, {selectedCell.gridY})");
    }

    private void OnSelectionCleared()
    {
        Debug.Log("✗ Selection Cleared");
    }

    // Test methods accessible from inspector
    [ContextMenu("Test Unit Selection")]
    public void TestUnitSelection()
    {
        if (testUnits != null && testUnits.Length > 0 && testUnits[0] != null)
        {
            selectionManager.SelectUnit(testUnits[0]);
        }
    }

    [ContextMenu("Test Unit Movement")]
    public void TestUnitMovement()
    {
        if (selectionManager.CurrentSelectedUnit != null && gridManager != null)
        {
            // Try to move the selected unit to a nearby cell
            Unit selectedUnit = selectionManager.CurrentSelectedUnit;
            Vector2Int currentPos = selectedUnit.GridPosition;
            Cell targetCell = gridManager.GetCell(currentPos.x + 1, currentPos.y);

            if (targetCell != null)
            {
                selectionManager.SelectCell(targetCell);
            }
        }
    }

    [ContextMenu("Clear Selection")]
    public void TestClearSelection()
    {
        if (selectionManager != null)
        {
            selectionManager.ClearAllSelections();
        }
    }

    private void OnGUI()
    {
        if (!showTestInstructions) return;

        GUI.Box(new Rect(10, 10, 300, 120), "Unit Selection Test");

        GUI.Label(new Rect(20, 35, 280, 20), "1. Click on units to select them");
        GUI.Label(new Rect(20, 55, 280, 20), "2. Click on highlighted cells to move");
        GUI.Label(new Rect(20, 75, 280, 20), "3. Click empty space to deselect");

        if (GUI.Button(new Rect(20, 95, 100, 25), "Spawn Units"))
        {
            SpawnTestUnits();
        }

        if (GUI.Button(new Rect(130, 95, 100, 25), "Clear Units"))
        {
            ClearTestUnits();
        }
    }
}
