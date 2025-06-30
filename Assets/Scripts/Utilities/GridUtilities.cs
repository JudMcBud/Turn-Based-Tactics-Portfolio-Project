using UnityEngine;

/// <summary>
/// Utility class for common grid-related calculations and operations
/// Consolidates duplicate logic found across Cell and Unit classes
/// </summary>
public static class GridUtilities
{
    /// <summary>
    /// Calculates Manhattan distance between two grid positions
    /// </summary>
    /// <param name="pos1">First grid position</param>
    /// <param name="pos2">Second grid position</param>
    /// <returns>Manhattan distance between positions</returns>
    public static int GetManhattanDistance(Vector2Int pos1, Vector2Int pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
    }

    /// <summary>
    /// Calculates Manhattan distance between two cells
    /// </summary>
    /// <param name="cell1">First cell</param>
    /// <param name="cell2">Second cell</param>
    /// <returns>Manhattan distance between cells</returns>
    public static int GetManhattanDistance(Cell cell1, Cell cell2)
    {
        if (cell1 == null || cell2 == null) return int.MaxValue;
        return GetManhattanDistance(new Vector2Int(cell1.gridX, cell1.gridY), new Vector2Int(cell2.gridX, cell2.gridY));
    }

    /// <summary>
    /// Calculates Euclidean distance between two grid positions
    /// </summary>
    /// <param name="pos1">First grid position</param>
    /// <param name="pos2">Second grid position</param>
    /// <returns>Euclidean distance between positions</returns>
    public static float GetEuclideanDistance(Vector2Int pos1, Vector2Int pos2)
    {
        return Vector2.Distance(new Vector2(pos1.x, pos1.y), new Vector2(pos2.x, pos2.y));
    }

    /// <summary>
    /// Calculates Euclidean distance between two cells
    /// </summary>
    /// <param name="cell1">First cell</param>
    /// <param name="cell2">Second cell</param>
    /// <returns>Euclidean distance between cells</returns>
    public static float GetEuclideanDistance(Cell cell1, Cell cell2)
    {
        if (cell1 == null || cell2 == null) return float.MaxValue;
        return GetEuclideanDistance(new Vector2Int(cell1.gridX, cell1.gridY), new Vector2Int(cell2.gridX, cell2.gridY));
    }

    /// <summary>
    /// Gets all cells within a given range using Manhattan distance
    /// </summary>
    /// <param name="centerPosition">Center position</param>
    /// <param name="range">Range in grid cells</param>
    /// <param name="gridManager">Grid manager reference</param>
    /// <param name="includeCenter">Whether to include the center position</param>
    /// <returns>Array of cells within range</returns>
    public static Cell[] GetCellsInRange(Vector2Int centerPosition, int range, GridManager gridManager, bool includeCenter = false)
    {
        if (gridManager == null) return new Cell[0];

        var cells = new System.Collections.Generic.List<Cell>();

        for (int x = centerPosition.x - range; x <= centerPosition.x + range; x++)
        {
            for (int y = centerPosition.y - range; y <= centerPosition.y + range; y++)
            {
                int distance = GetManhattanDistance(new Vector2Int(x, y), centerPosition);

                if (distance <= range && (includeCenter || distance > 0))
                {
                    Cell cell = gridManager.GetCell(x, y);
                    if (cell != null)
                    {
                        cells.Add(cell);
                    }
                }
            }
        }

        return cells.ToArray();
    }

    /// <summary>
    /// Gets adjacent cells (4-directional) from a given position
    /// </summary>
    /// <param name="centerPosition">Center position</param>
    /// <param name="gridManager">Grid manager reference</param>
    /// <returns>Array of adjacent cells</returns>
    public static Cell[] GetAdjacentCells(Vector2Int centerPosition, GridManager gridManager)
    {
        if (gridManager == null) return new Cell[0];

        var adjacentCells = new System.Collections.Generic.List<Cell>();
        var directions = new Vector2Int[]
        {
            Vector2Int.up,    // (0, 1)
            Vector2Int.down,  // (0, -1)
            Vector2Int.left,  // (-1, 0)
            Vector2Int.right  // (1, 0)
        };

        foreach (var direction in directions)
        {
            Vector2Int adjacentPos = centerPosition + direction;
            Cell cell = gridManager.GetCell(adjacentPos.x, adjacentPos.y);
            if (cell != null)
            {
                adjacentCells.Add(cell);
            }
        }

        return adjacentCells.ToArray();
    }

    /// <summary>
    /// Validates if a position is within grid bounds
    /// </summary>
    /// <param name="position">Position to validate</param>
    /// <param name="gridWidth">Grid width</param>
    /// <param name="gridHeight">Grid height</param>
    /// <returns>True if position is valid</returns>
    public static bool IsValidGridPosition(Vector2Int position, int gridWidth, int gridHeight)
    {
        return position.x >= 0 && position.x < gridWidth && position.y >= 0 && position.y < gridHeight;
    }
}
