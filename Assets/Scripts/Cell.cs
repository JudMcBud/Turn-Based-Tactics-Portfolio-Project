using UnityEngine;

/// <summary>
/// Cell class for individual grid tiles with customizable properties
/// </summary>
public class Cell : MonoBehaviour
{
    [Header("Cell Position")]
    public int gridX;
    public int gridY;

    [Header("Cell Properties")]
    public CellType cellType = CellType.Normal;
    public int movementCost = 1;
    public bool isWalkable = true;
    public bool isOccupied = false;

    [Header("Visual Components")]
    public Renderer cellRenderer;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color selectedColor = Color.green;
    public Color blockedColor = Color.red;

    [Header("Occupant")]
    public GameObject occupant;

    // Events
    public System.Action<Cell> OnCellClicked;
    public System.Action<Cell> OnCellHovered;

    private Color currentColor;
    private bool isHighlighted = false;
    private bool isSelected = false;

    void Start()
    {
        // Get renderer if not assigned
        if (cellRenderer == null)
            cellRenderer = GetComponent<Renderer>();

        // Set initial color
        currentColor = defaultColor;
        UpdateVisual();
    }

    void OnMouseDown()
    {
        OnCellClicked?.Invoke(this);
    }

    void OnMouseEnter()
    {
        OnCellHovered?.Invoke(this);
    }

    public void Initialize(int x, int y, CellType type = CellType.Normal)
    {
        gridX = x;
        gridY = y;
        cellType = type;

        // Set properties based on cell type
        SetCellTypeProperties();
    }

    private void SetCellTypeProperties()
    {
        switch (cellType)
        {
            case CellType.Normal:
                movementCost = 1;
                isWalkable = true;
                break;
            case CellType.Difficult:
                movementCost = 2;
                isWalkable = true;
                break;
            case CellType.Obstacle:
                movementCost = 999;
                isWalkable = false;
                break;
            case CellType.Water:
                movementCost = 3;
                isWalkable = true;
                break;
            case CellType.Impassable:
                movementCost = 999;
                isWalkable = false;
                break;
        }
    }

    public void SetOccupant(GameObject newOccupant)
    {
        if (newOccupant != null)
        {
            occupant = newOccupant;
            isOccupied = true;
        }
        else
        {
            occupant = null;
            isOccupied = false;
        }
    }

    public void Highlight(bool highlight)
    {
        isHighlighted = highlight;
        UpdateVisual();
    }

    public void Select(bool select)
    {
        isSelected = select;
        UpdateVisual();
    }

    public void SetCellType(CellType newType)
    {
        cellType = newType;
        SetCellTypeProperties();
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (cellRenderer == null) return;

        Color targetColor = defaultColor;

        // Priority order: Selected > Highlighted > Blocked > Default
        if (isSelected)
        {
            targetColor = selectedColor;
        }
        else if (isHighlighted)
        {
            targetColor = highlightColor;
        }
        else if (!isWalkable || isOccupied)
        {
            targetColor = blockedColor;
        }
        else
        {
            // Color based on cell type
            switch (cellType)
            {
                case CellType.Normal:
                    targetColor = defaultColor;
                    break;
                case CellType.Difficult:
                    targetColor = Color.Lerp(defaultColor, Color.yellow, 0.3f);
                    break;
                case CellType.Obstacle:
                    targetColor = Color.gray;
                    break;
                case CellType.Water:
                    targetColor = Color.blue;
                    break;
                case CellType.Impassable:
                    targetColor = Color.black;
                    break;
            }
        }

        currentColor = targetColor;
        cellRenderer.material.color = currentColor;
    }

    public bool CanMoveTo()
    {
        return isWalkable && !isOccupied;
    }

    public Vector3Int GetGridPosition()
    {
        return new Vector3Int(gridX, gridY, 0);
    }

    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    // Distance calculation for pathfinding
    public float GetDistanceTo(Cell otherCell)
    {
        return Vector2.Distance(new Vector2(gridX, gridY), new Vector2(otherCell.gridX, otherCell.gridY));
    }

    public int GetManhattanDistanceTo(Cell otherCell)
    {
        return Mathf.Abs(gridX - otherCell.gridX) + Mathf.Abs(gridY - otherCell.gridY);
    }

    // Debug information
    public override string ToString()
    {
        return $"Cell({gridX}, {gridY}) - Type: {cellType}, Cost: {movementCost}, Walkable: {isWalkable}, Occupied: {isOccupied}";
    }
}

public enum CellType
{
    Normal,     // Standard walkable cell
    Difficult,  // Higher movement cost
    Obstacle,   // Blocks movement but not line of sight
    Water,      // Walkable but with penalty
    Impassable  // Completely blocks movement
}
