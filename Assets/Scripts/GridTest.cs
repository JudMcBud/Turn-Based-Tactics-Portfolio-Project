using UnityEngine;

/// <summary>
/// Test script to verify Cell and GridManager work together properly
/// </summary>
public class GridTest : MonoBehaviour
{
    public GridManager gridManager;
    public Cell testCell;

    void Start()
    {
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();

        // Test getting a cell
        testCell = gridManager?.GetCell(0, 0);

        if (testCell != null)
        {
            Debug.Log($"Successfully got cell: {testCell}");

            // Test cell properties
            testCell.SetCellType(CellType.Water);
            testCell.Highlight(true);
        }
        else
        {
            Debug.LogWarning("Could not get test cell from grid manager");
        }
    }
}
