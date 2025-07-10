using UnityEngine;

/// <summary>
/// Integration test script for the Turn Manager system
/// Provides testing commands and validates turn/phase behavior
/// </summary>
public class TurnManagerTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool showTestUI = true;
    [SerializeField] private bool runAutomatedTests = false;
    [SerializeField] private float automatedTestInterval = 3.0f;

    [Header("Debug Info")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private SelectionManager selectionManager;
    [SerializeField] private GridManager gridManager;

    private float nextTestTime = 0f;
    private int testPhase = 0;

    private void Start()
    {
        // Find components
        if (turnManager == null)
            turnManager = ComponentFinder.GetTurnManager();

        if (selectionManager == null)
            selectionManager = ComponentFinder.GetSelectionManager();

        if (gridManager == null)
            gridManager = ComponentFinder.GetGridManager();

        // Validate setup
        ValidateSetup();

        if (runAutomatedTests)
        {
            Debug.Log("[TurnManagerTester] Starting automated tests in 3 seconds...");
            nextTestTime = Time.time + 3f;
        }
    }

    private void Update()
    {
        // Handle automated testing
        if (runAutomatedTests && Time.time >= nextTestTime)
        {
            RunAutomatedTest();
            nextTestTime = Time.time + automatedTestInterval;
        }

        // Manual test shortcuts
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestPhaseTransition();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            TestUnitRefresh();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            TestUnitSelection();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            TestContextMenu();
        }
    }

    private void ValidateSetup()
    {
        bool allGood = true;

        if (turnManager == null)
        {
            Debug.LogError("[TurnManagerTester] TurnManager not found!");
            allGood = false;
        }

        if (selectionManager == null)
        {
            Debug.LogError("[TurnManagerTester] SelectionManager not found!");
            allGood = false;
        }

        if (gridManager == null)
        {
            Debug.LogError("[TurnManagerTester] GridManager not found!");
            allGood = false;
        }

        if (allGood)
        {
            Debug.Log("[TurnManagerTester] All components found - system ready for testing!");
        }
    }

    [ContextMenu("Test Phase Transition")]
    public void TestPhaseTransition()
    {
        if (turnManager == null) return;

        Debug.Log($"[TurnManagerTester] Testing phase transition from {turnManager.CurrentPhase}");
        turnManager.EndCurrentPhase();
    }

    [ContextMenu("Test Unit Refresh")]
    public void TestUnitRefresh()
    {
        if (turnManager == null) return;

        Debug.Log("[TurnManagerTester] Testing unit action refresh");
        turnManager.RefreshActivePhaseUnits();
    }

    [ContextMenu("Test Unit Selection")]
    public void TestUnitSelection()
    {
        if (turnManager == null) return;

        var activeUnits = turnManager.GetActivePhaseUnits();
        if (activeUnits.Count > 0)
        {
            var testUnit = activeUnits[0];
            Debug.Log($"[TurnManagerTester] Testing selection of {testUnit.unitName} during {turnManager.CurrentPhase}");

            if (selectionManager != null)
            {
                selectionManager.SelectUnit(testUnit);
            }
        }
        else
        {
            Debug.Log($"[TurnManagerTester] No active units found for {turnManager.CurrentPhase}");
        }
    }

    [ContextMenu("Test Context Menu")]
    public void TestContextMenu()
    {
        if (turnManager == null) return;

        Vector3 testPosition = Camera.main.transform.position + Camera.main.transform.forward * 5f;
        Debug.Log($"[TurnManagerTester] Testing context menu at {testPosition}");
        TurnContextMenu.Show(testPosition, turnManager);
    }

    private void RunAutomatedTest()
    {
        switch (testPhase % 4)
        {
            case 0:
                TestUnitSelection();
                break;
            case 1:
                TestUnitRefresh();
                break;
            case 2:
                TestContextMenu();
                break;
            case 3:
                TestPhaseTransition();
                break;
        }

        testPhase++;
    }

    private void OnGUI()
    {
        if (!showTestUI || turnManager == null) return;

        // Test UI panel
        GUI.Box(new Rect(Screen.width - 220, 10, 200, 150), "Turn Manager Tester");

        float y = 35;
        GUI.Label(new Rect(Screen.width - 210, y, 180, 20), $"Turn: {turnManager.CurrentTurn}");
        y += 20;
        GUI.Label(new Rect(Screen.width - 210, y, 180, 20), $"Phase: {turnManager.CurrentPhase}");
        y += 25;

        if (GUI.Button(new Rect(Screen.width - 200, y, 80, 20), "End Phase"))
        {
            TestPhaseTransition();
        }
        if (GUI.Button(new Rect(Screen.width - 115, y, 80, 20), "Refresh"))
        {
            TestUnitRefresh();
        }
        y += 25;

        if (GUI.Button(new Rect(Screen.width - 200, y, 80, 20), "Select Unit"))
        {
            TestUnitSelection();
        }
        if (GUI.Button(new Rect(Screen.width - 115, y, 80, 20), "Context Menu"))
        {
            TestContextMenu();
        }
        y += 25;

        GUI.Label(new Rect(Screen.width - 210, y, 180, 20), "Shortcuts: T,R,U,C");
    }
}
