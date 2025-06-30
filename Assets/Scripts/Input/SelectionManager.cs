using UnityEngine;

/// <summary>
/// Manages cell and unit selection state for the tactics game
/// Handles selecting, deselecting, and tracking the currently selected cell/unit
/// </summary>
public class SelectionManager : MonoBehaviour
{
    [Header("Selection Settings")]
    [SerializeField] private bool showSelectionDebug = true;
    [SerializeField] private bool showMovementRange = true;

    [Header("Current Selection")]
    [SerializeField] private Cell currentSelectedCell;
    [SerializeField] private Unit currentSelectedUnit;

    [Header("References")]
    public GridManager gridManager;

    // Events for other systems to subscribe to
    public System.Action<Cell> OnCellSelected;
    public System.Action<Cell> OnCellDeselected;
    public System.Action<Unit> OnUnitSelected;
    public System.Action<Unit> OnUnitDeselected;
    public System.Action OnSelectionCleared;

    // Properties
    public Cell CurrentSelectedCell => currentSelectedCell;
    public Unit CurrentSelectedUnit => currentSelectedUnit;
    public bool HasCellSelection => currentSelectedCell != null;
    public bool HasUnitSelection => currentSelectedUnit != null;
    public bool HasAnySelection => HasCellSelection || HasUnitSelection;

    private void Awake()
    {
        // Ensure only one SelectionManager exists
        if (FindObjectsByType<SelectionManager>(FindObjectsSortMode.None).Length > 1)
        {
            Debug.LogWarning("Multiple SelectionManagers found! Destroying duplicate.");
            Destroy(gameObject);
        }

        // Find GridManager if not assigned
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();
    }

    /// <summary>
    /// Selects a unit, deselecting previous selections if needed
    /// </summary>
    public void SelectUnit(Unit unitToSelect)
    {
        if (unitToSelect == null)
        {
            if (showSelectionDebug)
                Debug.LogWarning("Attempted to select null unit");
            return;
        }

        // If the same unit is clicked, toggle selection
        if (currentSelectedUnit == unitToSelect)
        {
            DeselectCurrentUnit();
            return;
        }

        // Clear previous selections
        ClearAllSelections();

        // Select the new unit
        currentSelectedUnit = unitToSelect;

        // Show visual feedback
        currentSelectedUnit.SetSelected(true);

        // Also select the cell the unit is on
        if (unitToSelect.OccupiedCell != null)
        {
            currentSelectedCell = unitToSelect.OccupiedCell;
            currentSelectedCell.Select(true);
        }

        if (showSelectionDebug)
            Debug.Log($"Selected unit: {unitToSelect.unitName} at ({unitToSelect.GridPosition.x}, {unitToSelect.GridPosition.y})");

        // Show movement range if enabled
        if (showMovementRange && gridManager != null)
        {
            Cell[] movementRange = unitToSelect.GetMovementRange();
            gridManager.HighlightCells(movementRange, true);
        }

        // Notify other systems
        OnUnitSelected?.Invoke(currentSelectedUnit);

        // Fire unit selected event
        unitToSelect.OnUnitSelected?.Invoke(unitToSelect);
    }

    /// <summary>
    /// Deselects the currently selected unit
    /// </summary>
    public void DeselectCurrentUnit()
    {
        if (currentSelectedUnit == null) return;

        Unit unitToDeselect = currentSelectedUnit;

        // Remove visual feedback
        unitToDeselect.SetSelected(false);

        // Clear movement range highlights
        if (showMovementRange && gridManager != null)
        {
            Cell[] movementRange = unitToDeselect.GetMovementRange();
            gridManager.HighlightCells(movementRange, false);
        }

        if (showSelectionDebug)
            Debug.Log($"Deselected unit: {unitToDeselect.unitName}");

        // Clear the reference
        currentSelectedUnit = null;

        // Notify other systems
        OnUnitDeselected?.Invoke(unitToDeselect);
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

        // If we have a unit selected and click on a valid movement cell, move the unit
        if (currentSelectedUnit != null && currentSelectedUnit.CanMoveThisTurn)
        {
            Cell[] movementRange = currentSelectedUnit.GetMovementRange();
            if (System.Array.Exists(movementRange, cell => cell == cellToSelect))
            {
                // Move the unit to the clicked cell
                bool moveSuccess = currentSelectedUnit.MoveTo(cellToSelect);
                if (moveSuccess)
                {
                    if (showSelectionDebug)
                        Debug.Log($"Moved {currentSelectedUnit.unitName} to ({cellToSelect.gridX}, {cellToSelect.gridY})");

                    // Update selection to new position
                    if (currentSelectedCell != null)
                        currentSelectedCell.Select(false);

                    currentSelectedCell = cellToSelect;
                    currentSelectedCell.Select(true);

                    // Clear and update movement range
                    if (showMovementRange && gridManager != null)
                    {
                        gridManager.ClearAllHighlights();
                        Cell[] newMovementRange = currentSelectedUnit.GetMovementRange();
                        gridManager.HighlightCells(newMovementRange, true);
                    }

                    return;
                }
            }
        }

        // If the same cell is clicked, toggle selection
        if (currentSelectedCell == cellToSelect)
        {
            DeselectCurrentCell();
            return;
        }

        // Check if cell has a unit on it
        if (cellToSelect.occupant != null)
        {
            Unit unitOnCell = cellToSelect.occupant.GetComponent<Unit>();
            if (unitOnCell != null)
            {
                // Select the unit instead of just the cell
                SelectUnit(unitOnCell);
                return;
            }
        }

        // Regular cell selection
        ClearAllSelections();

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
    /// Clears all selections (both unit and cell)
    /// </summary>
    public void ClearAllSelections()
    {
        // Clear unit selection
        if (currentSelectedUnit != null)
        {
            DeselectCurrentUnit();
        }

        // Clear cell selection
        if (currentSelectedCell != null)
        {
            DeselectCurrentCell();
        }

        // Clear highlights
        if (gridManager != null)
        {
            gridManager.ClearAllHighlights();
        }

        OnSelectionCleared?.Invoke();
    }

    /// <summary>
    /// Clears all selections (alias for ClearAllSelections)
    /// </summary>
    public void ClearSelection()
    {
        ClearAllSelections();
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
