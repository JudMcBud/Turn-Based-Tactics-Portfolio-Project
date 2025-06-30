using UnityEngine;

/// <summary>
/// Basic camera controller for the tactics game
/// Handles initial camera positioning for optimal grid viewing
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Vector3 defaultPosition = new Vector3(5, 10, -10);
    [SerializeField] private Vector3 defaultRotation = new Vector3(48, 0, 0);

    [Header("Grid References")]
    [SerializeField] private GridManager gridManager;

    void Start()
    {
        SetupCameraPosition();
    }

    /// <summary>
    /// Sets up the camera position based on grid size or default values
    /// </summary>
    private void SetupCameraPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found!");
            return;
        }

        Vector3 targetPosition = defaultPosition;
        Vector3 targetRotation = defaultRotation;

        // If grid manager is available, calculate optimal position based on grid size
        if (gridManager != null)
        {
            float gridCenterX = gridManager.gridWidth * 0.5f;
            float gridCenterZ = gridManager.gridHeight * 0.5f;

            // Position camera to look at center of grid
            targetPosition = new Vector3(gridCenterX, defaultPosition.y, gridCenterZ + defaultPosition.z);
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = Quaternion.Euler(targetRotation);

        Debug.Log($"Camera positioned at {targetPosition} with rotation {targetRotation}");
    }

    /// <summary>
    /// Public method to recenter camera on grid (useful for runtime adjustments)
    /// </summary>
    public void RecenterOnGrid()
    {
        SetupCameraPosition();
    }
}
