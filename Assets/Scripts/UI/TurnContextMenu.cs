using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

/// <summary>
/// Context menu that appears when clicking empty cells to end turns
/// Provides options for ending the current phase
/// </summary>
public class TurnContextMenu : MonoBehaviour
{
    [Header("Menu Settings")]
    [SerializeField] private bool showConfirmation = true;
    [SerializeField] private Vector2 menuSize = new Vector2(120, 80);

    [Header("UI References")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private RectTransform menuRectTransform;

    // Static instance for easy access
    private static TurnContextMenu instance;
    private static TurnManager connectedTurnManager;
    private static Vector3 menuWorldPosition;
    private static bool isMenuVisible = false;
    private static int showFrame = -1;

    // Input Handling
    private Camera gameCamera;
    private TacticsInputManager inputManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            gameCamera = ComponentFinder.GetMainCamera();
            inputManager = ComponentFinder.GetInputManager();

            // Create UI if it doesn't exist
            if (menuCanvas == null)
            {
                CreateCanvasUI();
            }

            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Hide by default
        if (menuPanel != null) menuPanel.SetActive(false);
        // Subscribe to input events if we have an input manager
        if (inputManager != null)
        {
            inputManager.OnPrimaryActionPerformed += OnPrimaryActionPerformed;
        }

        SetupButtonEvents();
    }

    private void OnDestroy()
    {
        // Unsubscribe from input events
        if (inputManager != null)
        {
            inputManager.OnPrimaryActionPerformed -= OnPrimaryActionPerformed;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    /// Creates the Canvas-based UI elements programmatically
    /// </summary>
    private void CreateCanvasUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("TurnContextMenu_Canvas");
        canvasObj.transform.SetParent(transform);

        menuCanvas = canvasObj.AddComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menuCanvas.sortingOrder = 1000; // Ensure it's on top

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Menu Panel
        GameObject panelObj = new GameObject("MenuPanel");
        panelObj.transform.SetParent(canvasObj.transform);

        menuPanel = panelObj;
        menuRectTransform = panelObj.AddComponent<RectTransform>();
        menuRectTransform.sizeDelta = menuSize;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f); // Background color

        //Create Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform);

        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "End Turn";
        titleText.fontSize = 8;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.6f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = new Vector2(5, 5);
        titleRect.offsetMax = new Vector2(-5, -5);

        //Create End Turn Button
        GameObject endTurnButtonObj = new GameObject("EndTurnButton");
        endTurnButtonObj.transform.SetParent(panelObj.transform, false);

        endTurnButton = endTurnButtonObj.AddComponent<Button>();
        Image endTurnButtonImage = endTurnButtonObj.AddComponent<Image>();
        endTurnButtonImage.color = new Color(0.8f, 0.3f, 0.3f, 1f);

        RectTransform endButtonRect = endTurnButtonObj.GetComponent<RectTransform>();
        endButtonRect.anchorMin = new Vector2(0.05f, 0.35f);
        endButtonRect.anchorMax = new Vector2(0.95f, 0.55f);
        endButtonRect.offsetMin = Vector2.zero;
        endButtonRect.offsetMax = Vector2.zero;

        //End Turn Button Text
        GameObject endButtonTextObj = new GameObject("EndTurnButtonText");
        endButtonTextObj.transform.SetParent(endTurnButtonObj.transform, false);

        TextMeshProUGUI endButtonText = endButtonTextObj.AddComponent<TextMeshProUGUI>();
        endButtonText.text = "End Turn";
        endButtonText.fontSize = 7;
        endButtonText.color = Color.white;
        endButtonText.alignment = TextAlignmentOptions.Center;

        RectTransform endButtonTextRect = endButtonTextObj.GetComponent<RectTransform>();
        endButtonTextRect.anchorMin = Vector2.zero;
        endButtonTextRect.anchorMax = Vector2.zero;
        endButtonTextRect.offsetMin = Vector2.zero;
        endButtonTextRect.offsetMax = Vector2.zero;

        // Create Cancel Button
        GameObject cancelButtonObj = new GameObject("CancelButton");
        cancelButtonObj.transform.SetParent(panelObj.transform, false);

        cancelButton = cancelButtonObj.AddComponent<Button>();
        Image cancelButtonImage = cancelButtonObj.AddComponent<Image>();
        cancelButtonImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);

        RectTransform cancelButtonRect = cancelButtonObj.GetComponent<RectTransform>();
        cancelButtonRect.anchorMin = new Vector2(0.05f, 0.05f);
        cancelButtonRect.anchorMax = new Vector2(0.95f, 0.25f);
        cancelButtonRect.offsetMin = Vector2.zero;
        cancelButtonRect.offsetMax = Vector2.zero;

        //Cancel Button Text
        GameObject cancelButtonTextObj = new GameObject("CancelButtonText");
        cancelButtonTextObj.transform.SetParent(cancelButtonObj.transform, false);

        TextMeshProUGUI cancelButtonText = cancelButtonTextObj.AddComponent<TextMeshProUGUI>();
        cancelButtonText.text = "Cancel";
        cancelButtonText.fontSize = 7;
        cancelButtonText.color = Color.white;
        cancelButtonText.alignment = TextAlignmentOptions.Center;

        RectTransform cancelButtonTextRect = cancelButtonTextObj.GetComponent<RectTransform>();
        cancelButtonTextRect.anchorMin = Vector2.zero;
        cancelButtonTextRect.anchorMax = Vector2.zero;
        cancelButtonTextRect.offsetMin = Vector2.zero;
        cancelButtonTextRect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Sets up button click events
    /// </summary>
    private void SetupButtonEvents()
    {
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(EndTurn);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(Hide);
        }
    }

    /// <summary>
    ///  Updates the menu position based on the world position
    /// </summary>
    private void UpdateMenuPosition()
    {
        if (gameCamera == null || menuRectTransform == null) return;

        //Convert world position to screen position
        Vector3 screenPosition = gameCamera.WorldToScreenPoint(menuWorldPosition);

        // Clamp to screen bounds
        screenPosition.x = Mathf.Clamp(screenPosition.x, 0, Screen.width - menuSize.x);
        screenPosition.y = Mathf.Clamp(screenPosition.y, 0, Screen.height - menuSize.y);
        menuRectTransform.position = screenPosition;
    }

    private void OnPrimaryActionPerformed(Vector3 worldPosition)
    {
        if (Time.frameCount <= showFrame + 1) return;

        // Hide menu if clicking elsewhere (only when menu is visible)
        if (isMenuVisible && menuPanel != null && menuPanel.activeInHierarchy)
        {
            Vector2 mousePos = inputManager.GetMouseScreenPosition();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                menuRectTransform, mousePos, gameCamera, out Vector2 localPoint);
            if (!menuRectTransform.rect.Contains(localPoint))
            {
                Hide();
            }
        }
    }

    /// <summary>
    /// Shows the context menu at the specified world position
    /// </summary>
    public static void Show(Vector3 worldPosition, TurnManager turnManager)
    {
        if (instance == null)
        {
            CreateInstance();
        }

        connectedTurnManager = turnManager;
        menuWorldPosition = worldPosition;
        isMenuVisible = true;
        showFrame = Time.frameCount;

        if (instance != null && instance.menuPanel != null)
        {
            //Update title text
            if (instance.titleText != null)
            {
                instance.titleText.text = $"End {connectedTurnManager.CurrentPhase}?";
            }

            Debug.Log("Setting menuPanel active");
            instance.menuPanel.SetActive(true);
            instance.UpdateMenuPosition();
        }
    }

    /// <summary>
    /// Hides the context menu
    /// </summary>
    public static void Hide()
    {
        isMenuVisible = false;
        connectedTurnManager = null;

        if (instance != null)
        {
            instance.menuPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Creates a context menu instance if none exists
    /// </summary>
    private static void CreateInstance()
    {
        GameObject menuObject = new GameObject("TurnContextMenu");
        instance = menuObject.AddComponent<TurnContextMenu>();

        // Initialize will happen in Awake
    }

    /// <summary>
    /// Handles the end turn action
    /// </summary>
    private void EndTurn()
    {
        Debug.Log($"[TurnContextMenu] Ending turn for {connectedTurnManager?.CurrentPhase}");
        if (connectedTurnManager != null)
        {
            if (showConfirmation)
            {
                // Show confirmation dialog
                string message = $"End {connectedTurnManager.CurrentPhase}?\n\nAny units with remaining actions will lose them.";
                if (EditorConfirmation(message))
                {
                    connectedTurnManager.EndCurrentPhase();
                }
            }
            else
            {
                connectedTurnManager.EndCurrentPhase();
            }
        }

        Hide();
    }

    /// <summary>
    /// Shows a confirmation dialog (fallback for runtime)
    /// </summary>
    private bool EditorConfirmation(string message)
    {
#if UNITY_EDITOR
        return UnityEditor.EditorUtility.DisplayDialog("End Turn", message, "Yes", "Cancel");
#else
        // In build, just proceed without confirmation or implement custom dialog
        Debug.Log($"[TurnContextMenu] {message}");
        return true;
#endif
    }

    /// <summary>
    /// Gets the current menu visibility state
    /// </summary>
    public static bool IsVisible => isMenuVisible;

    /// <summary>
    /// Forces the menu to hide (useful for scene transitions)
    /// </summary>
    public static void ForceHide()
    {
        Hide();
    }
}
