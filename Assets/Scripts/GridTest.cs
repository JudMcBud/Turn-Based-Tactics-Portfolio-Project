using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Test script to verify Cell and GridManager work together properly
/// Uses Unity's new Input System via TacticsInputManager
/// </summary>
public class GridTest : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;
    public TacticsInputManager inputManager;
    public Cell testCell;

    private void Start()
    {
        // Find references if not assigned
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();

        if (inputManager == null)
            inputManager = FindFirstObjectByType<TacticsInputManager>();
    }

    private void OnEnable()
    {
        // Subscribe to input events
        if (inputManager != null)
        {
            inputManager.OnTestGridRequested += PerformGridTest;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        if (inputManager != null)
        {
            inputManager.OnTestGridRequested -= PerformGridTest;
        }
    }

    private void PerformGridTest()
    {
        // Find GridManager if not assigned
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<GridManager>();
            if (gridManager == null)
            {
                Debug.LogError("GridManager not found in scene!");
                return;
            }
        }

        // Test getting a cell
        testCell = gridManager.GetCell(0, 0);

        if (testCell != null)
        {
            Debug.Log($"Successfully got cell: {testCell}");

            // Test cell properties
            testCell.SetCellType(CellType.Water);
            testCell.Highlight(true);

            Debug.Log("Grid test completed successfully! Cell (0,0) set to Water type and highlighted.");
        }
        else
        {
            Debug.LogWarning("Could not get test cell from grid manager - grid may not be initialized yet.");
        }
    }

    // Optional: Method to manually test different cells
    public void TestCellAtPosition(int x, int y)
    {
        if (gridManager == null)
        {
            Debug.LogError("GridManager reference is null!");
            return;
        }

        Cell cell = gridManager.GetCell(x, y);
        if (cell != null)
        {
            Debug.Log($"Testing cell at ({x}, {y}): {cell}");
            cell.Highlight(true);
        }
        else
        {
            Debug.LogWarning($"No cell found at position ({x}, {y})");
        }
    }
}
