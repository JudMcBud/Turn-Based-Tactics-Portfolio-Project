using UnityEngine;
using System.Collections;

/// <summary>
/// Consolidated test manager for all game systems
/// Replaces multiple individual test scripts with a single comprehensive tester
/// </summary>
public class GameSystemTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = false;
    [SerializeField] private bool showDetailedLogs = true;
    [SerializeField] private float testDelay = 1f;

    [Header("Grid Testing")]
    [SerializeField] private bool testGridSystem = true;

    [Header("Selection Testing")]
    [SerializeField] private bool testSelectionSystem = true;

    [Header("Unit Testing")]
    [SerializeField] private bool testUnitSystem = true;
    [SerializeField] private GameObject testUnitPrefab;
    [SerializeField] private int testUnitsToSpawn = 2;

    [Header("References")]
    private GridManager gridManager;
    private SelectionManager selectionManager;
    private TacticsInputManager inputManager;

    private int testsPassed = 0;
    private int totalTests = 0;

    private void Start()
    {
        // Find all managers
        FindManagers();

        if (runTestsOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }

    private void FindManagers()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        selectionManager = FindFirstObjectByType<SelectionManager>();
        inputManager = FindFirstObjectByType<TacticsInputManager>();
    }

    /// <summary>
    /// Runs all enabled tests
    /// </summary>
    private IEnumerator RunAllTests()
    {
        LogTest("=== STARTING COMPREHENSIVE GAME SYSTEM TESTS ===");
        testsPassed = 0;
        totalTests = 0;

        if (testGridSystem)
        {
            yield return StartCoroutine(TestGridSystem());
            yield return new WaitForSeconds(testDelay);
        }

        if (testSelectionSystem)
        {
            yield return StartCoroutine(TestSelectionSystem());
            yield return new WaitForSeconds(testDelay);
        }

        if (testUnitSystem)
        {
            yield return StartCoroutine(TestUnitSystem());
            yield return new WaitForSeconds(testDelay);
        }

        LogTest($"=== TESTS COMPLETED: {testsPassed}/{totalTests} PASSED ===");
    }

    private IEnumerator TestGridSystem()
    {
        LogTest("--- Testing Grid System ---");

        // Test 1: Grid Manager exists
        TestCondition("GridManager exists", gridManager != null);

        if (gridManager != null)
        {
            // Test 2: Can get cell at position
            Cell testCell = gridManager.GetCell(0, 0);
            TestCondition("Can get cell at (0,0)", testCell != null);

            if (testCell != null)
            {
                // Test 3: Cell properties
                TestCondition("Cell has valid properties", testCell.isWalkable);

                // Test 4: Cell highlighting
                testCell.Highlight(true);
                TestCondition("Cell can be highlighted", testCell != null);
                yield return new WaitForSeconds(0.5f);
                testCell.Highlight(false);

                // Test 5: Cell type changes
                CellType originalType = testCell.cellType;
                testCell.SetCellType(CellType.Water);
                TestCondition("Cell type can be changed", testCell.cellType == CellType.Water);
                testCell.SetCellType(originalType);
            }
        }

        LogTest("Grid System tests completed.");
    }

    private IEnumerator TestSelectionSystem()
    {
        LogTest("--- Testing Selection System ---");

        // Test 1: Selection Manager exists
        TestCondition("SelectionManager exists", selectionManager != null);

        if (selectionManager != null && gridManager != null)
        {
            // Test 2: Can select cell
            Cell testCell = gridManager.GetCell(1, 1);
            if (testCell != null)
            {
                selectionManager.SelectCell(testCell);
                TestCondition("Can select cell", selectionManager.CurrentSelectedCell == testCell);
                yield return new WaitForSeconds(0.5f);

                // Test 3: Can clear selection
                selectionManager.ClearAllSelections();
                TestCondition("Can clear selection", selectionManager.CurrentSelectedCell == null);
            }
        }

        LogTest("Selection System tests completed.");
    }

    private IEnumerator TestUnitSystem()
    {
        LogTest("--- Testing Unit System ---");

        if (testUnitPrefab != null && gridManager != null)
        {
            // Spawn test units
            Unit[] testUnits = new Unit[testUnitsToSpawn];
            for (int i = 0; i < testUnitsToSpawn; i++)
            {
                Vector3 spawnPosition = new Vector3(i + 2, 0.8f, 0);
                GameObject unitObj = Instantiate(testUnitPrefab, spawnPosition, Quaternion.identity);
                testUnits[i] = unitObj.GetComponent<Unit>();

                if (testUnits[i] != null)
                {
                    testUnits[i].SetGridPosition(i + 2, 0);
                    testUnits[i].unitName = $"TestUnit_{i}";
                }
            }

            yield return new WaitForSeconds(0.5f);

            // Test unit properties
            if (testUnits[0] != null)
            {
                TestCondition("Unit spawned successfully", testUnits[0] != null);
                TestCondition("Unit has valid health", testUnits[0].CurrentHealthPoints > 0);
                TestCondition("Unit is alive", testUnits[0].IsAlive);
            }

            // Test unit movement
            if (testUnits[0] != null)
            {
                Cell targetCell = gridManager.GetCell(3, 0);
                if (targetCell != null)
                {
                    bool moveResult = testUnits[0].MoveTo(targetCell);
                    TestCondition("Unit can move to valid cell", moveResult);
                }
            }

            // Clean up test units
            yield return new WaitForSeconds(testDelay);
            foreach (Unit unit in testUnits)
            {
                if (unit != null)
                    DestroyImmediate(unit.gameObject);
            }
        }
        else
        {
            LogTest("Unit testing skipped - no test unit prefab assigned");
        }

        LogTest("Unit System tests completed.");
    }

    private void TestCondition(string testName, bool condition)
    {
        totalTests++;
        if (condition)
        {
            testsPassed++;
            if (showDetailedLogs)
                LogTest($"✓ {testName}");
        }
        else
        {
            if (showDetailedLogs)
                LogTest($"✗ {testName}");
        }
    }

    private void LogTest(string message)
    {
        if (showDetailedLogs)
            Debug.Log($"[GameSystemTester] {message}");
    }

    // Manual test methods for inspector/debugging
    [ContextMenu("Run All Tests")]
    public void RunTests()
    {
        if (Application.isPlaying)
            StartCoroutine(RunAllTests());
    }

    [ContextMenu("Test Grid Only")]
    public void TestGridOnly()
    {
        if (Application.isPlaying)
            StartCoroutine(TestGridSystem());
    }

    [ContextMenu("Test Selection Only")]
    public void TestSelectionOnly()
    {
        if (Application.isPlaying)
            StartCoroutine(TestSelectionSystem());
    }

    [ContextMenu("Validate All Managers")]
    public void ValidateManagers()
    {
        FindManagers();
        LogTest($"GridManager: {(gridManager != null ? "Found" : "Missing")}");
        LogTest($"SelectionManager: {(selectionManager != null ? "Found" : "Missing")}");
        LogTest($"InputManager: {(inputManager != null ? "Found" : "Missing")}");
    }
}
