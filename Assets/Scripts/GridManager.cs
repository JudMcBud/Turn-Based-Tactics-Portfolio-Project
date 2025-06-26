using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public GameObject cellPrefab;
    private Cell[,] gridCells;

    void Start()
    {
        gridCells = new Cell[gridWidth, gridHeight];
        CreateGrid();
    }

    private void CreateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3Int position = new Vector3Int(x, 0, y);
                GameObject cellObject = Instantiate(cellPrefab, position, Quaternion.Euler(90f, 0f, 0f));
                cellObject.transform.SetParent(transform);
                cellObject.name = $"Cell_{x}_{y}";

                Cell cell = cellObject.GetComponent<Cell>();
                if (cell != null)
                {
                    cell.Initialize(x, y);
                    // Subscribe to cell events if needed
                    cell.OnCellClicked += OnCellClicked;
                    cell.OnCellHovered += OnCellHovered;
                }

                gridCells[x, y] = cell;
            }
        }
    }

    // Event handlers for cell interactions
    private void OnCellClicked(Cell clickedCell)
    {
        Debug.Log($"Cell clicked at position ({clickedCell.gridX}, {clickedCell.gridY})");
        // Add your cell click logic here
        // For example: select cell, move unit, show cell info, etc.
    }

    private void OnCellHovered(Cell hoveredCell)
    {
        Debug.Log($"Cell hovered at position ({hoveredCell.gridX}, {hoveredCell.gridY})");
        // Add your cell hover logic here
        // For example: show cell preview, highlight movement range, etc.
    }

    // Utility methods for grid management
    public Cell GetCell(int x, int y)
    {
        if (IsValidPosition(x, y))
        {
            return gridCells[x, y];
        }
        return null;
    }

    public Cell GetCellAtWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x);
        int y = Mathf.RoundToInt(worldPosition.y);
        return GetCell(x, y);
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    public void HighlightCells(Cell[] cellsToHighlight, bool highlight = true)
    {
        foreach (Cell cell in cellsToHighlight)
        {
            if (cell != null)
            {
                cell.Highlight(highlight);
            }
        }
    }

    public void ClearAllHighlights()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridCells[x, y] != null)
                {
                    gridCells[x, y].Highlight(false);
                    gridCells[x, y].Select(false);
                }
            }
        }
    }
}
