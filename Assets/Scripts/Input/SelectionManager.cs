using UnityEngine;

/// <summary>
/// Manages cell selection state for the tactics game
/// Handles selecting, deselecting, and tracking the currently selected cell
/// </summary>
public class SelectionManager : MonoBehaviour
{
    [Header("Selection Settings")]
    [SerializeField] private bool allowMultipleSelection = false;
    [SerializeField] private bool showSelectionDebug = true;

    [Header("Current Selection")]
    [SerializeField] private Cell currentSelectedCell;

    // Events for other systems to subscribe to
    public System.Action<Cell> OnCellSelected;
    public System.Action<Cell> OnCellDeselected;
    public System.Action OnSelectionCleared;

    // Properties
    public Cell CurrentSelectedCell => currentSelectedCell;
    public bool HasSelection => currentSelectedCell != null;

    private void Awake()
    {
        // Ensure only one SelectionManager exists
        if (FindObjectsByType<SelectionManager>(FindObjectsSortMode.None).Length > 1)
        {
            Debug.LogWarning("Multiple SelectionManagers found! Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Selects a cell, deselecting the previous one if needed
    /// </summary>
    public void SelectCell(Cell cellToSelect)
    {
        if (cellToSelect == null)
        {
            if (showSelectionDebug)
                Debug.LogWarning("Attempted to select null cell");
            return;
        }

        // If the same cell is clicked, toggle selection
        if (currentSelectedCell == cellToSelect)
        {
            DeselectCurrentCell();
            return;
        }

        // Deselect previous cell if we don't allow multiple selection
        if (!allowMultipleSelection && currentSelectedCell != null)
        {
            DeselectCurrentCell();
        }

        // Select the new cell
        currentSelectedCell = cellToSelect;
        currentSelectedCell.Select(true);

        if (showSelectionDebug)
            Debug.Log($"Selected cell at ({cellToSelect.gridX}, {cellToSelect.gridY})");

        // Notify other systems
        OnCellSelected?.Invoke(currentSelectedCell);
    }

    /// <summary>
    /// Deselects the currently selected cell
    /// </summary>
    public void DeselectCurrentCell()
    {
        if (currentSelectedCell == null) return;

        Cell cellToDeselect = currentSelectedCell;
        currentSelectedCell.Select(false);

        if (showSelectionDebug)
            Debug.Log($"Deselected cell at ({cellToDeselect.gridX}, {cellToDeselect.gridY})");

        // Clear the reference
        currentSelectedCell = null;

        // Notify other systems
        OnCellDeselected?.Invoke(cellToDeselect);
    }

    /// <summary>
    /// Clears all selections
    /// </summary>
    public void ClearSelection()
    {
        if (currentSelectedCell != null)
        {
            DeselectCurrentCell();
        }

        OnSelectionCleared?.Invoke();
    }

    /// <summary>
    /// Checks if a specific cell is currently selected
    /// </summary>
    public bool IsCellSelected(Cell cell)
    {
        return currentSelectedCell != null && currentSelectedCell == cell;
    }

    /// <summary>
    /// Gets the grid position of the currently selected cell
    /// </summary>
    public Vector3Int GetSelectedCellPosition()
    {
        if (currentSelectedCell == null)
            return Vector3Int.zero;

        return currentSelectedCell.GetGridPosition();
    }

    /// <summary>
    /// Gets the world position of the currently selected cell
    /// </summary>
    public Vector3 GetSelectedCellWorldPosition()
    {
        if (currentSelectedCell == null)
            return Vector3.zero;

        return currentSelectedCell.GetWorldPosition();
    }

    /// <summary>
    /// Handle cell click from external systems
    /// </summary>
    public void OnCellClicked(Cell clickedCell)
    {
        if (clickedCell == null) return;

        // Check if the cell can be selected (not occupied by enemy, walkable, etc.)
        if (CanSelectCell(clickedCell))
        {
            SelectCell(clickedCell);
        }
        else
        {
            if (showSelectionDebug)
                Debug.Log($"Cannot select cell at ({clickedCell.gridX}, {clickedCell.gridY}) - cell is not selectable");
        }
    }

    /// <summary>
    /// Determines if a cell can be selected based on game rules
    /// </summary>
    private bool CanSelectCell(Cell cell)
    {
        // Basic selection rules - you can expand this based on your game logic
        if (cell == null) return false;

        // For now, allow selection of any cell that's walkable
        // You might want to add more complex rules like:
        // - Only allow selection of cells with friendly units
        // - Only allow selection within movement range
        // - Block selection of enemy-occupied cells

        return cell.isWalkable;
    }

    // Debug visualization in editor
    private void OnDrawGizmos()
    {
        if (currentSelectedCell != null && showSelectionDebug)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(currentSelectedCell.transform.position + Vector3.up * 0.1f, Vector3.one * 1.1f);
        }
    }
}
