using UnityEngine;
using System.Collections;

/// <summary>
/// Comprehensive test script for the Unit class
/// Tests all major functionality including stats, movement, combat, and status effects
/// </summary>
public class UnitTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = true;
    [SerializeField] private bool showDetailedLogs = true;
    [SerializeField] private float testDelay = 1f;
    [SerializeField] private bool cleanupAfterTests = false;

    [Header("Unit Prefab")]
    [SerializeField] private GameObject unitPrefab;

    [Header("Test Units")]
    [SerializeField] private Unit testUnit1;
    [SerializeField] private Unit testUnit2;
    [SerializeField] private Unit enemyUnit;

    [Header("Test References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private SelectionManager selectionManager;

    private int testsPassed = 0;
    private int totalTests = 0;

    private void Start()
    {
        // Find references if not assigned
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();
        if (selectionManager == null)
            selectionManager = FindFirstObjectByType<SelectionManager>();

        if (runTestsOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }

    /// <summary>
    /// Runs all unit tests with delays between them
    /// </summary>
    private IEnumerator RunAllTests()
    {
        LogTest("=== STARTING UNIT TESTS ===");

        // Wait a moment for everything to initialize
        yield return new WaitForSeconds(1f);

        // Create test units if not assigned
        if (testUnit1 == null || testUnit2 == null || enemyUnit == null)
        {
            CreateTestUnits();
            yield return new WaitForSeconds(0.5f);
        }

        // Run all test categories
        yield return StartCoroutine(TestBasicProperties());
        yield return new WaitForSeconds(testDelay);

        yield return StartCoroutine(TestHealthSystem());
        yield return new WaitForSeconds(testDelay);

        yield return StartCoroutine(TestPositionSystem());
        yield return new WaitForSeconds(testDelay);

        yield return StartCoroutine(TestMovementSystem());
        yield return new WaitForSeconds(testDelay);

        yield return StartCoroutine(TestCombatSystem());
        yield return new WaitForSeconds(testDelay);

        yield return StartCoroutine(TestStatusEffects());
        yield return new WaitForSeconds(testDelay);

        yield return StartCoroutine(TestTurnManagement());
        yield return new WaitForSeconds(testDelay);

        // Final results
        LogTest($"=== TEST RESULTS: {testsPassed}/{totalTests} PASSED ===");

        if (testsPassed == totalTests)
        {
            LogTest("🎉 ALL TESTS PASSED! Unit system is working correctly.");
        }
        else
        {
            LogTest($"⚠️ {totalTests - testsPassed} tests failed. Check the logs above.");
        }

        // Cleanup if requested
        if (cleanupAfterTests)
        {
            yield return new WaitForSeconds(2f);
            CleanupTestUnits();
        }
    }

    /// <summary>
    /// Creates test units for testing using the unit prefab
    /// </summary>
    private void CreateTestUnits()
    {
        LogTest("Creating test units from prefab...");

        if (unitPrefab == null)
        {
            LogTest("⚠️ Unit prefab not assigned! Creating empty GameObjects instead.");
            CreateTestUnitsLegacy();
            return;
        }

        // Create test units from prefab
        if (testUnit1 == null)
        {
            GameObject unit1Obj = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity);
            unit1Obj.name = "TestUnit1";
            testUnit1 = unit1Obj.GetComponent<Unit>();

            if (testUnit1 == null)
            {
                LogTest("❌ Unit prefab doesn't have Unit component! Adding one...");
                testUnit1 = unit1Obj.AddComponent<Unit>();
            }
        }

        if (testUnit2 == null)
        {
            GameObject unit2Obj = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity);
            unit2Obj.name = "TestUnit2";
            testUnit2 = unit2Obj.GetComponent<Unit>();

            if (testUnit2 == null)
            {
                LogTest("❌ Unit prefab doesn't have Unit component! Adding one...");
                testUnit2 = unit2Obj.AddComponent<Unit>();
            }
        }

        if (enemyUnit == null)
        {
            GameObject enemyObj = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity);
            enemyObj.name = "EnemyUnit";
            enemyUnit = enemyObj.GetComponent<Unit>();

            if (enemyUnit == null)
            {
                LogTest("❌ Unit prefab doesn't have Unit component! Adding one...");
                enemyUnit = enemyObj.AddComponent<Unit>();
            }
        }

        // Configure test units with different properties
        ConfigureTestUnits();

        LogTest("✅ Test units created successfully from prefab!");
    }

    /// <summary>
    /// Fallback method for creating test units without prefab
    /// </summary>
    private void CreateTestUnitsLegacy()
    {
        if (testUnit1 == null)
        {
            GameObject unit1Obj = new GameObject("TestUnit1");
            testUnit1 = unit1Obj.AddComponent<Unit>();
        }

        if (testUnit2 == null)
        {
            GameObject unit2Obj = new GameObject("TestUnit2");
            testUnit2 = unit2Obj.AddComponent<Unit>();
        }

        if (enemyUnit == null)
        {
            GameObject enemyObj = new GameObject("EnemyUnit");
            enemyUnit = enemyObj.AddComponent<Unit>();
        }

        ConfigureTestUnits();
    }

    /// <summary>
    /// Configures the test units with specific properties for testing
    /// </summary>
    private void ConfigureTestUnits()
    {
        // Configure test units
        testUnit1.unitName = "Test Player Unit";
        testUnit1.team = Team.Player;
        testUnit1.unitType = UnitType.Infantry;

        testUnit2.unitName = "Test Player Unit 2";
        testUnit2.team = Team.Player;
        testUnit2.unitType = UnitType.Archer;

        enemyUnit.unitName = "Test Enemy Unit";
        enemyUnit.team = Team.Enemy;
        enemyUnit.unitType = UnitType.Knight;

        // Position units on grid with some spacing for visibility
        if (gridManager != null)
        {
            testUnit1.SetGridPosition(1, 1);
            testUnit2.SetGridPosition(3, 3);
            enemyUnit.SetGridPosition(5, 5);

            LogTest($"Units positioned: Player1({1},{1}), Player2({3},{3}), Enemy({5},{5})");
        }
        else
        {
            // Position in world space if no grid manager
            testUnit1.transform.position = new Vector3(1, 0, 1);
            testUnit2.transform.position = new Vector3(3, 0, 3);
            enemyUnit.transform.position = new Vector3(5, 0, 5);

            LogTest("Units positioned in world space (no GridManager found)");
        }
    }

    /// <summary>
    /// Cleans up test units after testing
    /// </summary>
    private void CleanupTestUnits()
    {
        LogTest("Cleaning up test units...");

        if (testUnit1 != null && testUnit1.gameObject != null)
        {
            DestroyImmediate(testUnit1.gameObject);
            testUnit1 = null;
        }

        if (testUnit2 != null && testUnit2.gameObject != null)
        {
            DestroyImmediate(testUnit2.gameObject);
            testUnit2 = null;
        }

        if (enemyUnit != null && enemyUnit.gameObject != null)
        {
            DestroyImmediate(enemyUnit.gameObject);
            enemyUnit = null;
        }

        LogTest("✅ Test units cleaned up!");
    }

    #region Test Categories

    private IEnumerator TestBasicProperties()
    {
        LogTest("--- Testing Basic Properties ---");

        // Test unit identity
        AssertTrue(testUnit1.unitName == "Test Player Unit", "Unit name property");
        AssertTrue(testUnit1.Team == Team.Player, "Team property");
        AssertTrue(testUnit1.UnitType == UnitType.Infantry, "Unit type property");
        AssertTrue(testUnit1.IsAlive, "Unit should be alive initially");

        // Test stat properties
        AssertTrue(testUnit1.MaxHealthPoints > 0, "Max health should be positive");
        AssertTrue(testUnit1.CurrentHealthPoints > 0, "Current health should be positive");
        AssertTrue(testUnit1.AttackPower > 0, "Attack power should be positive");
        AssertTrue(testUnit1.Defense >= 0, "Defense should be non-negative");
        AssertTrue(testUnit1.MovementDistance > 0, "Movement distance should be positive");

        yield return null;
    }

    private IEnumerator TestHealthSystem()
    {
        LogTest("--- Testing Health System ---");

        int initialHealth = testUnit1.CurrentHealthPoints;
        int maxHealth = testUnit1.MaxHealthPoints;

        // Test taking damage
        testUnit1.TakeDamage(20, DamageType.Physical);
        AssertTrue(testUnit1.CurrentHealthPoints < initialHealth, "Unit should take damage");
        AssertTrue(testUnit1.IsAlive, "Unit should still be alive after moderate damage");

        // Test healing
        int healthAfterDamage = testUnit1.CurrentHealthPoints;
        testUnit1.Heal(10);
        AssertTrue(testUnit1.CurrentHealthPoints > healthAfterDamage, "Unit should heal");
        AssertTrue(testUnit1.CurrentHealthPoints <= maxHealth, "Health shouldn't exceed maximum");

        // Test death (using a copy to avoid breaking other tests)
        Unit tempUnit = CreateTempUnit();
        tempUnit.TakeDamage(1000, DamageType.Physical); // Massive damage
        AssertTrue(!tempUnit.IsAlive, "Unit should die from massive damage");

        DestroyImmediate(tempUnit.gameObject);

        yield return null;
    }

    private IEnumerator TestPositionSystem()
    {
        LogTest("--- Testing Position System ---");

        if (gridManager == null)
        {
            LogTest("⚠️ GridManager not found, skipping position tests");
            yield break;
        }

        // Test setting grid position
        Vector2Int originalPos = testUnit2.GridPosition;
        testUnit2.SetGridPosition(2, 2);
        AssertTrue(testUnit2.GridPosition.x == 2 && testUnit2.GridPosition.y == 2, "Grid position should update");

        // Test cell occupation
        Cell occupiedCell = testUnit2.OccupiedCell;
        AssertTrue(occupiedCell != null, "Unit should occupy a cell");
        AssertTrue(occupiedCell.occupant == testUnit2.gameObject, "Cell should reference the unit");

        yield return null;
    }

    private IEnumerator TestMovementSystem()
    {
        LogTest("--- Testing Movement System ---");

        if (gridManager == null)
        {
            LogTest("⚠️ GridManager not found, skipping movement tests");
            yield break;
        }

        // Test movement range calculation
        Cell[] movementRange = testUnit1.GetMovementRange();
        AssertTrue(movementRange.Length > 0, "Unit should have movement range");

        // Visual feedback: Highlight movement range
        if (movementRange.Length > 0)
        {
            LogTest($"Highlighting {movementRange.Length} movement cells for visual feedback...");
            gridManager.HighlightCells(movementRange, true);
            yield return new WaitForSeconds(1f); // Show highlights
        }

        // Test valid movement
        testUnit1.StartTurn(); // Ensure unit can move
        Vector2Int startPos = testUnit1.GridPosition;
        LogTest($"Unit starting position: ({startPos.x}, {startPos.y})");

        if (movementRange.Length > 0)
        {
            Cell targetCell = movementRange[0];
            LogTest($"Attempting to move to: ({targetCell.gridX}, {targetCell.gridY})");

            bool moveSuccess = testUnit1.MoveTo(targetCell);
            AssertTrue(moveSuccess, "Valid movement should succeed");
            AssertTrue(testUnit1.GridPosition != startPos, "Position should change after movement");
            AssertTrue(!testUnit1.CanMoveThisTurn, "Unit shouldn't be able to move again this turn");

            if (moveSuccess)
            {
                LogTest($"✅ Unit successfully moved to: ({testUnit1.GridPosition.x}, {testUnit1.GridPosition.y})");
                yield return new WaitForSeconds(1f); // Show movement result
            }
        }

        // Clear highlights
        if (movementRange.Length > 0)
        {
            gridManager.HighlightCells(movementRange, false);
        }

        yield return null;
    }

    private IEnumerator TestCombatSystem()
    {
        LogTest("--- Testing Combat System ---");

        // Position units adjacent to each other for combat with visual feedback
        LogTest("Positioning units for combat test...");
        testUnit1.SetGridPosition(4, 4);
        enemyUnit.SetGridPosition(4, 5); // Adjacent

        yield return new WaitForSeconds(0.5f); // Allow positioning to be visible

        testUnit1.StartTurn(); // Reset turn state
        enemyUnit.StartTurn();

        int enemyInitialHealth = enemyUnit.CurrentHealthPoints;
        LogTest($"Enemy initial health: {enemyInitialHealth}");

        // Visual feedback: Highlight the units involved in combat
        if (testUnit1.OccupiedCell != null) testUnit1.OccupiedCell.Select(true);
        if (enemyUnit.OccupiedCell != null) enemyUnit.OccupiedCell.Highlight(true);

        yield return new WaitForSeconds(0.5f);

        // Test attack conditions
        AssertTrue(testUnit1.CanAttack(enemyUnit), "Player unit should be able to attack adjacent enemy");
        AssertTrue(!testUnit1.CanAttack(testUnit2), "Unit shouldn't be able to attack teammate");

        // Test attack execution with visual feedback
        LogTest($"{testUnit1.unitName} attacking {enemyUnit.unitName}!");
        testUnit1.Attack(enemyUnit);

        AssertTrue(enemyUnit.CurrentHealthPoints < enemyInitialHealth, "Enemy should take damage from attack");
        AssertTrue(!testUnit1.CanActThisTurn, "Unit shouldn't be able to act again this turn");

        LogTest($"Enemy health after attack: {enemyUnit.CurrentHealthPoints} (damage: {enemyInitialHealth - enemyUnit.CurrentHealthPoints})");

        yield return new WaitForSeconds(1f); // Show combat result

        // Clear visual feedback
        if (testUnit1.OccupiedCell != null) testUnit1.OccupiedCell.Select(false);
        if (enemyUnit.OccupiedCell != null) enemyUnit.OccupiedCell.Highlight(false);

        yield return null;
    }

    private IEnumerator TestStatusEffects()
    {
        LogTest("--- Testing Status Effects ---");

        Unit tempUnit = CreateTempUnit();

        // Test applying status effects
        int originalAttack = tempUnit.AttackPower;
        tempUnit.ApplyStatusEffect(StatusEffect.Buffed);
        AssertTrue(tempUnit.AttackPower > originalAttack, "Buffed status should increase attack");

        // Test removing status effects
        tempUnit.RemoveStatusEffect(StatusEffect.Buffed);
        AssertTrue(tempUnit.AttackPower == originalAttack, "Removing buff should restore original attack");

        // Test stun effect
        tempUnit.StartTurn();
        AssertTrue(tempUnit.CanMoveThisTurn, "Unit should be able to move normally");

        tempUnit.ApplyStatusEffect(StatusEffect.Stunned);
        AssertTrue(!tempUnit.CanMoveThisTurn, "Stunned unit shouldn't be able to move");
        AssertTrue(!tempUnit.CanActThisTurn, "Stunned unit shouldn't be able to act");

        DestroyImmediate(tempUnit.gameObject);

        yield return null;
    }

    private IEnumerator TestTurnManagement()
    {
        LogTest("--- Testing Turn Management ---");

        Unit tempUnit = CreateTempUnit();

        // Use up movement and action
        tempUnit.StartTurn();
        AssertTrue(tempUnit.CanMoveThisTurn, "Unit should be able to move at turn start");
        AssertTrue(tempUnit.CanActThisTurn, "Unit should be able to act at turn start");

        // Simulate using movement (manually set since we can't easily move in test)
        tempUnit.GetType().GetField("canMoveThisTurn",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(tempUnit, false);

        tempUnit.GetType().GetField("canActThisTurn",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(tempUnit, false);

        AssertTrue(!tempUnit.CanMoveThisTurn, "Unit shouldn't be able to move after using movement");
        AssertTrue(!tempUnit.CanActThisTurn, "Unit shouldn't be able to act after using action");

        // Test turn reset
        tempUnit.StartTurn();
        AssertTrue(tempUnit.CanMoveThisTurn, "Turn start should reset movement");
        AssertTrue(tempUnit.CanActThisTurn, "Turn start should reset action");

        DestroyImmediate(tempUnit.gameObject);

        yield return null;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a temporary unit for testing using the prefab
    /// </summary>
    private Unit CreateTempUnit()
    {
        GameObject tempObj;

        if (unitPrefab != null)
        {
            tempObj = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity);
            tempObj.name = "TempTestUnit";
        }
        else
        {
            tempObj = new GameObject("TempTestUnit");
        }

        Unit tempUnit = tempObj.GetComponent<Unit>();
        if (tempUnit == null)
        {
            tempUnit = tempObj.AddComponent<Unit>();
        }

        tempUnit.unitName = "Temp Test Unit";
        tempUnit.team = Team.Player;
        return tempUnit;
    }

    /// <summary>
    /// Asserts that a condition is true and logs the result
    /// </summary>
    private void AssertTrue(bool condition, string testName)
    {
        totalTests++;
        if (condition)
        {
            testsPassed++;
            if (showDetailedLogs)
                LogTest($"✅ PASS: {testName}");
        }
        else
        {
            LogTest($"❌ FAIL: {testName}");
        }
    }

    /// <summary>
    /// Logs a test message
    /// </summary>
    private void LogTest(string message)
    {
        Debug.Log($"[UnitTester] {message}");
    }

    #endregion

    #region Public Test Methods (Can be called from Inspector)

    [ContextMenu("Run All Tests")]
    public void RunTests()
    {
        StartCoroutine(RunAllTests());
    }

    [ContextMenu("Test Health System")]
    public void TestHealthOnly()
    {
        StartCoroutine(TestHealthSystem());
    }

    [ContextMenu("Test Movement System")]
    public void TestMovementOnly()
    {
        StartCoroutine(TestMovementSystem());
    }

    [ContextMenu("Test Combat System")]
    public void TestCombatOnly()
    {
        StartCoroutine(TestCombatSystem());
    }

    [ContextMenu("Create Test Units")]
    public void CreateTestUnitsManually()
    {
        CreateTestUnits();
    }

    [ContextMenu("Cleanup Test Units")]
    public void CleanupTestUnitsManually()
    {
        CleanupTestUnits();
    }

    [ContextMenu("Show Movement Range")]
    public void ShowMovementRange()
    {
        if (testUnit1 != null && gridManager != null)
        {
            Cell[] range = testUnit1.GetMovementRange();
            gridManager.HighlightCells(range, true);
            LogTest($"Highlighted {range.Length} movement cells for {testUnit1.unitName}");
        }
    }

    [ContextMenu("Clear All Highlights")]
    public void ClearHighlights()
    {
        if (gridManager != null)
        {
            gridManager.ClearAllHighlights();
            LogTest("Cleared all cell highlights");
        }
    }

    /// <summary>
    /// Quick test to verify unit setup
    /// </summary>
    [ContextMenu("Quick Unit Info")]
    public void LogUnitInfo()
    {
        if (testUnit1 != null)
        {
            LogTest($"Unit 1: {testUnit1.unitName} | HP: {testUnit1.CurrentHealthPoints}/{testUnit1.MaxHealthPoints} | " +
                   $"Pos: {testUnit1.GridPosition} | Team: {testUnit1.Team} | Alive: {testUnit1.IsAlive}");
        }

        if (testUnit2 != null)
        {
            LogTest($"Unit 2: {testUnit2.unitName} | HP: {testUnit2.CurrentHealthPoints}/{testUnit2.MaxHealthPoints} | " +
                   $"Pos: {testUnit2.GridPosition} | Team: {testUnit2.Team} | Alive: {testUnit2.IsAlive}");
        }

        if (enemyUnit != null)
        {
            LogTest($"Enemy: {enemyUnit.unitName} | HP: {enemyUnit.CurrentHealthPoints}/{enemyUnit.MaxHealthPoints} | " +
                   $"Pos: {enemyUnit.GridPosition} | Team: {enemyUnit.Team} | Alive: {enemyUnit.IsAlive}");
        }
    }

    #endregion
}
