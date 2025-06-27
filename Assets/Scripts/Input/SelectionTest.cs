using UnityEngine;

/// <summary>
/// Test script to demonstrate the selection system
/// </summary>
public class SelectionTest : MonoBehaviour
{
    [Header("References")]
    public SelectionManager selectionManager;
    public GridManager gridManager;

    private void Start()
    {
        // Find references if not assigned
        if (selectionManager == null)
            selectionManager = FindFirstObjectByType<SelectionManager>();

        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();
    }

    private void OnEnable()
    {
        // Subscribe to selection events
        if (selectionManager != null)
        {
            selectionManager.OnCellSelected += OnCellSelected;
            selectionManager.OnCellDeselected += OnCellDeselected;
            selectionManager.OnSelectionCleared += OnSelectionCleared;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from selection events
        if (selectionManager != null)
        {
            selectionManager.OnCellSelected -= OnCellSelected;
            selectionManager.OnCellDeselected -= OnCellDeselected;
            selectionManager.OnSelectionCleared -= OnSelectionCleared;
        }
    }

    private void OnCellSelected(Cell selectedCell)
    {
        Debug.Log($"[SelectionTest] Cell selected: {selectedCell} at position ({selectedCell.gridX}, {selectedCell.gridY})");

        // Example: Highlight nearby cells when a cell is selected
        HighlightAdjacentCells(selectedCell, true);
    }

    private void OnCellDeselected(Cell deselectedCell)
    {
        Debug.Log($"[SelectionTest] Cell deselected: {deselectedCell} at position ({deselectedCell.gridX}, {deselectedCell.gridY})");

        // Clear highlights when cell is deselected
        HighlightAdjacentCells(deselectedCell, false);
    }

    private void OnSelectionCleared()
    {
        Debug.Log("[SelectionTest] All selections cleared");

        // Clear all highlights
        if (gridManager != null)
        {
            gridManager.ClearAllHighlights();
        }
    }

    /// <summary>
    /// Highlights the 4 adjacent cells (up, down, left, right) of the given cell
    /// </summary>
    private void HighlightAdjacentCells(Cell centerCell, bool highlight)
    {
        if (gridManager == null || centerCell == null) return;

        // Define the 4 directions (up, down, left, right)
        Vector2[] directions = {
            Vector2.up,     // (0, 1)
            Vector2.down,   // (0, -1)
            Vector2.left,   // (-1, 0)
            Vector2.right   // (1, 0)
        };

        foreach (Vector2 direction in directions)
        {
            int adjacentX = centerCell.gridX + (int)direction.x;
            int adjacentY = centerCell.gridY + (int)direction.y;

            Cell adjacentCell = gridManager.GetCell(adjacentX, adjacentY);
            if (adjacentCell != null && adjacentCell.CanMoveTo())
            {
                adjacentCell.Highlight(highlight);
            }
        }
    }

    // Public methods for testing from inspector or other scripts
    [ContextMenu("Test Select Cell (0,0)")]
    public void TestSelectCell()
    {
        if (gridManager != null)
        {
            Cell testCell = gridManager.GetCell(0, 0);
            if (testCell != null && selectionManager != null)
            {
                selectionManager.SelectCell(testCell);
            }
        }
    }

    [ContextMenu("Test Clear Selection")]
    public void TestClearSelection()
    {
        if (selectionManager != null)
        {
            selectionManager.ClearSelection();
        }
    }
}
