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
            selectionManager = ComponentFinder.GetSelectionManager();

        if (gridManager == null)
            gridManager = ComponentFinder.GetGridManager();
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

        Cell[] adjacentCells = GridUtilities.GetAdjacentCells(new Vector2Int(centerCell.gridX, centerCell.gridY), gridManager);

        foreach (Cell adjacentCell in adjacentCells)
        {
            if (adjacentCell.CanMoveTo())
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
