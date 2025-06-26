using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Centralized input manager for the tactics game using Unity's new Input System
/// </summary>
public class TacticsInputManager : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActions;

    [Header("References")]
    public GridManager gridManager;
    public Camera mainCamera;

    // Input Action references
    private InputAction primaryAction;
    private InputAction mousePosition;
    private InputAction testGridAction;

    // Events for other systems to subscribe to
    public System.Action<Vector3> OnPrimaryActionPerformed;
    public System.Action<Vector3> OnMouseMove;
    public System.Action OnTestGridRequested;

    private void Awake()
    {
        // Get camera if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Get grid manager if not assigned
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();
    }

    private void OnEnable()
    {
        // Get the gameplay action map
        var gameplayMap = inputActions.FindActionMap("Gameplay");

        // Get individual actions
        primaryAction = gameplayMap.FindAction("PrimaryAction");
        mousePosition = gameplayMap.FindAction("MousePosition");
        testGridAction = gameplayMap.FindAction("TestGrid");

        // Subscribe to action events
        primaryAction.performed += OnPrimaryActionPressed;
        mousePosition.performed += OnMousePositionChanged;
        testGridAction.performed += OnTestGridPressed;

        // Enable the action map
        gameplayMap.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (primaryAction != null)
            primaryAction.performed -= OnPrimaryActionPressed;
        if (mousePosition != null)
            mousePosition.performed -= OnMousePositionChanged;
        if (testGridAction != null)
            testGridAction.performed -= OnTestGridPressed;

        // Disable the action map
        var gameplayMap = inputActions.FindActionMap("Gameplay");
        gameplayMap?.Disable();
    }

    private void OnPrimaryActionPressed(InputAction.CallbackContext context)
    {
        Vector2 screenPos = mousePosition.ReadValue<Vector2>();
        Vector3 worldPos = ScreenToWorldPosition(screenPos);

        Debug.Log($"Primary action at screen: {screenPos}, world: {worldPos}");

        // Try to get the cell at this position
        if (gridManager != null)
        {
            Cell cell = gridManager.GetCellAtWorldPosition(worldPos);
            if (cell != null)
            {
                Debug.Log($"Clicked on cell: {cell}");
                // The cell's own OnMouseDown will handle the click event
            }
        }

        // Notify other systems
        OnPrimaryActionPerformed?.Invoke(worldPos);
    }

    private void OnMousePositionChanged(InputAction.CallbackContext context)
    {
        Vector2 screenPos = context.ReadValue<Vector2>();
        Vector3 worldPos = ScreenToWorldPosition(screenPos);

        // Notify other systems about mouse movement
        OnMouseMove?.Invoke(worldPos);
    }

    private void OnTestGridPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Test grid action pressed!");
        OnTestGridRequested?.Invoke();
    }

    private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
    {
        if (mainCamera == null) return Vector3.zero;

        // Convert screen position to world position
        // For a tactics game, you'll want to raycast to the ground plane
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        // Create a plane at Y=0 (ground level)
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    // Public methods for other scripts to use
    public Vector2 GetMouseScreenPosition()
    {
        return mousePosition.ReadValue<Vector2>();
    }

    public Vector3 GetMouseWorldPosition()
    {
        return ScreenToWorldPosition(GetMouseScreenPosition());
    }

    public bool IsPrimaryActionPressed()
    {
        return primaryAction.IsPressed();
    }
}
