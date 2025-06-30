using UnityEngine;

/// <summary>
/// Centralized utility for finding common game components
/// Reduces duplicate FindFirstObjectByType calls across the codebase
/// </summary>
public static class ComponentFinder
{
    private static GridManager _gridManager;
    private static SelectionManager _selectionManager;
    private static TacticsInputManager _inputManager;
    private static Camera _mainCamera;

    /// <summary>
    /// Gets the GridManager instance, caching it for performance
    /// </summary>
    public static GridManager GetGridManager()
    {
        if (_gridManager == null)
            _gridManager = Object.FindFirstObjectByType<GridManager>();
        return _gridManager;
    }

    /// <summary>
    /// Gets the SelectionManager instance, caching it for performance
    /// </summary>
    public static SelectionManager GetSelectionManager()
    {
        if (_selectionManager == null)
            _selectionManager = Object.FindFirstObjectByType<SelectionManager>();
        return _selectionManager;
    }

    /// <summary>
    /// Gets the TacticsInputManager instance, caching it for performance
    /// </summary>
    public static TacticsInputManager GetInputManager()
    {
        if (_inputManager == null)
            _inputManager = Object.FindFirstObjectByType<TacticsInputManager>();
        return _inputManager;
    }

    /// <summary>
    /// Gets the main camera, with fallback to Camera.main
    /// </summary>
    public static Camera GetMainCamera()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;
        return _mainCamera;
    }

    /// <summary>
    /// Clears the cached references - call this when scene changes
    /// </summary>
    public static void ClearCache()
    {
        _gridManager = null;
        _selectionManager = null;
        _inputManager = null;
        _mainCamera = null;
    }

    /// <summary>
    /// Validates that all core managers are present in the scene
    /// </summary>
    public static bool ValidateManagers()
    {
        bool isValid = true;

        if (GetGridManager() == null)
        {
            Debug.LogError("GridManager not found in scene!");
            isValid = false;
        }

        if (GetSelectionManager() == null)
        {
            Debug.LogError("SelectionManager not found in scene!");
            isValid = false;
        }

        if (GetInputManager() == null)
        {
            Debug.LogError("TacticsInputManager not found in scene!");
            isValid = false;
        }

        if (GetMainCamera() == null)
        {
            Debug.LogError("Main Camera not found in scene!");
            isValid = false;
        }

        return isValid;
    }
}
