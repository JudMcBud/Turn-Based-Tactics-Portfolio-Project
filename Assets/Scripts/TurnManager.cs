using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages turn order, phases, and unit action states in the tactics game
/// Controls when units can be selected based on current phase
/// </summary>
public class TurnManager : MonoBehaviour
{
    [Header("Turn Settings")]
    [SerializeField] private int currentTurn = 1;
    [SerializeField] private TurnPhase currentPhase = TurnPhase.PlayerPhase;
    [SerializeField] private bool autoRefreshActions = true;
    [SerializeField] private bool showTurnDebug = true;

    [Header("Phase Duration")]
    [SerializeField] private float phaseTransitionDelay = 1.0f;
    [SerializeField] private bool requireConfirmation = true;

    [Header("References")]
    [SerializeField] private SelectionManager selectionManager;
    [SerializeField] private GridManager gridManager;

    // Events for other systems to subscribe to
    public System.Action<int> OnTurnChanged;
    public System.Action<TurnPhase> OnPhaseChanged;
    public System.Action<TurnPhase> OnPhaseStarted;
    public System.Action<TurnPhase> OnPhaseEnded;
    public System.Action OnAllUnitsRefreshed;

    // Properties
    public int CurrentTurn => currentTurn;
    public TurnPhase CurrentPhase => currentPhase;
    public bool IsPlayerPhase => currentPhase == TurnPhase.PlayerPhase;
    public bool IsEnemyPhase => currentPhase == TurnPhase.EnemyPhase;

    // Cached unit lists for performance
    private List<Unit> playerUnits = new List<Unit>();
    private List<Unit> enemyUnits = new List<Unit>();
    private List<Unit> allUnits = new List<Unit>();

    private void Awake()
    {
        // Find managers if not assigned
        if (selectionManager == null)
            selectionManager = ComponentFinder.GetSelectionManager();
        if (gridManager == null)
            gridManager = ComponentFinder.GetGridManager();
    }

    private void Start()
    {
        // Initialize turn system
        RefreshUnitLists();
        StartTurn();

        // Subscribe to selection events to handle turn ending
        if (selectionManager != null)
        {
            selectionManager.OnCellSelected += OnCellSelected;
        }

        if (showTurnDebug)
        {
            Debug.Log($"[TurnManager] Turn system initialized - Turn {currentTurn}, {currentPhase}");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (selectionManager != null)
        {
            selectionManager.OnCellSelected -= OnCellSelected;
        }
    }

    /// <summary>
    /// Handles cell selection to potentially show end turn context menu
    /// </summary>
    private void OnCellSelected(Cell selectedCell)
    {
        Debug.Log($"[TurnManager] Inside OnCellSelected");
        // Only show context menu if clicking on empty cell during active phase
        if (selectedCell != null && !selectedCell.isOccupied)
        {
            // Check if we have any units that still have actions available
            // bool hasActionsRemaining = GetActivePhaseUnits().Any(unit => unit.CanMoveThisTurn || unit.CanActThisTurn);
            // Debug.Log($"[TurnManager] Checking for end turn context menu - Actions remaining: {hasActionsRemaining}");

            if (!TurnContextMenu.IsVisible)
            {
                ShowEndTurnContextMenu();
            }
            // if (hasActionsRemaining || !requireConfirmation)
            // {
            // }
        }
    }

    /// <summary>
    /// Shows the context menu for ending the current turn/phase
    /// </summary>
    private void ShowEndTurnContextMenu()
    {
        // Create context menu near mouse position
        Vector3 mouseWorldPos = ComponentFinder.GetInputManager()?.GetMouseWorldPosition() ?? Vector3.zero;
        TurnContextMenu.Show(mouseWorldPos, this);
    }

    /// <summary>
    /// Starts the current turn/phase
    /// </summary>
    public void StartTurn()
    {
        if (showTurnDebug)
        {
            Debug.Log($"[TurnManager] Starting Turn {currentTurn} - {currentPhase}");
        }

        // Refresh unit actions if enabled
        if (autoRefreshActions)
        {
            RefreshActivePhaseUnits();
        }

        // Clear any existing selections
        if (selectionManager != null)
        {
            selectionManager.ClearAllSelections();
        }

        // Notify systems about phase start
        OnPhaseStarted?.Invoke(currentPhase);

        // Update unit selectability based on current phase
        UpdateUnitSelectability();
    }

    /// <summary>
    /// Ends the current phase and transitions to the next
    /// </summary>
    public void EndCurrentPhase()
    {
        if (showTurnDebug)
        {
            Debug.Log($"[TurnManager] Ending {currentPhase}");
        }

        // Clear selections before phase change
        if (selectionManager != null)
        {
            selectionManager.ClearAllSelections();
        }

        // Notify about phase end
        OnPhaseEnded?.Invoke(currentPhase);

        // Transition to next phase
        TransitionToNextPhase();
    }

    /// <summary>
    /// Transitions to the next phase or turn
    /// </summary>
    private void TransitionToNextPhase()
    {
        TurnPhase previousPhase = currentPhase;

        switch (currentPhase)
        {
            case TurnPhase.PlayerPhase:
                currentPhase = TurnPhase.EnemyPhase;
                break;
            case TurnPhase.EnemyPhase:
                currentPhase = TurnPhase.PlayerPhase;
                currentTurn++;
                OnTurnChanged?.Invoke(currentTurn);
                break;
        }

        if (showTurnDebug)
        {
            Debug.Log($"[TurnManager] Phase transition: {previousPhase} → {currentPhase}");
        }

        // Notify about phase change
        OnPhaseChanged?.Invoke(currentPhase);

        // Start the new phase after a brief delay
        if (phaseTransitionDelay > 0)
        {
            Invoke(nameof(StartTurn), phaseTransitionDelay);
        }
        else
        {
            StartTurn();
        }
    }

    /// <summary>
    /// Refreshes action states for units in the active phase
    /// </summary>
    public void RefreshActivePhaseUnits()
    {
        List<Unit> activeUnits = GetActivePhaseUnits();

        foreach (Unit unit in activeUnits)
        {
            if (unit != null && unit.IsAlive)
            {
                unit.StartTurn();
            }
        }

        if (showTurnDebug)
        {
            Debug.Log($"[TurnManager] Refreshed {activeUnits.Count} {currentPhase} units");
        }

        OnAllUnitsRefreshed?.Invoke();
    }

    /// <summary>
    /// Gets all units belonging to the currently active phase
    /// </summary>
    public List<Unit> GetActivePhaseUnits()
    {
        return currentPhase == TurnPhase.PlayerPhase ? playerUnits : enemyUnits;
    }

    /// <summary>
    /// Gets all units belonging to the inactive phase
    /// </summary>
    public List<Unit> GetInactivePhaseUnits()
    {
        return currentPhase == TurnPhase.PlayerPhase ? enemyUnits : playerUnits;
    }

    /// <summary>
    /// Updates which units can be selected based on current phase
    /// </summary>
    private void UpdateUnitSelectability()
    {
        // This is handled by the Unit.OnMouseDown() method checking team
        // But we could add additional logic here if needed

        if (showTurnDebug)
        {
            Team activeTeam = currentPhase == TurnPhase.PlayerPhase ? Team.Player : Team.Enemy;
            Debug.Log($"[TurnManager] Only {activeTeam} units are now selectable");
        }
    }

    /// <summary>
    /// Refreshes the cached unit lists
    /// </summary>
    public void RefreshUnitLists()
    {
        // Find all units in the scene
        Unit[] foundUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        allUnits.Clear();
        playerUnits.Clear();
        enemyUnits.Clear();

        foreach (Unit unit in foundUnits)
        {
            if (unit != null && unit.IsAlive)
            {
                allUnits.Add(unit);

                switch (unit.Team)
                {
                    case Team.Player:
                        playerUnits.Add(unit);
                        break;
                    case Team.Enemy:
                        enemyUnits.Add(unit);
                        break;
                        // Neutral units are not added to phase-specific lists
                }
            }
        }

        if (showTurnDebug)
        {
            Debug.Log($"[TurnManager] Unit lists refreshed - Players: {playerUnits.Count}, Enemies: {enemyUnits.Count}");
        }
    }

    /// <summary>
    /// Checks if a unit can be selected based on current phase
    /// </summary>
    public bool CanSelectUnit(Unit unit)
    {
        if (unit == null || !unit.IsAlive) return false;

        // Units can only be selected during their team's phase
        switch (currentPhase)
        {
            case TurnPhase.PlayerPhase:
                return unit.Team == Team.Player;
            case TurnPhase.EnemyPhase:
                return unit.Team == Team.Enemy;
            default:
                return false;
        }
    }

    /// <summary>
    /// Forces a phase transition (for testing or special scenarios)
    /// </summary>
    [ContextMenu("Force Next Phase")]
    public void ForceNextPhase()
    {
        EndCurrentPhase();
    }

    /// <summary>
    /// Resets the turn system to the beginning
    /// </summary>
    [ContextMenu("Reset Turn System")]
    public void ResetTurnSystem()
    {
        currentTurn = 1;
        currentPhase = TurnPhase.PlayerPhase;

        RefreshUnitLists();
        StartTurn();

        if (showTurnDebug)
        {
            Debug.Log("[TurnManager] Turn system reset to Turn 1, Player Phase");
        }
    }

    /// <summary>
    /// Gets summary information about the current turn state
    /// </summary>
    public string GetTurnSummary()
    {
        List<Unit> activeUnits = GetActivePhaseUnits();
        int unitsWithActions = activeUnits.Count(u => u.CanMoveThisTurn || u.CanActThisTurn);

        return $"Turn {currentTurn} - {currentPhase}\n" +
               $"Active Units: {activeUnits.Count}\n" +
               $"Units with Actions: {unitsWithActions}";
    }

    // Debug visualization
    private void OnGUI()
    {
        if (!showTurnDebug) return;

        GUI.Box(new Rect(10, Screen.height - 120, 200, 100), "Turn Manager");
        GUI.Label(new Rect(20, Screen.height - 100, 180, 20), $"Turn: {currentTurn}");
        GUI.Label(new Rect(20, Screen.height - 80, 180, 20), $"Phase: {currentPhase}");

        List<Unit> activeUnits = GetActivePhaseUnits();
        int unitsWithActions = activeUnits.Count(u => u.CanMoveThisTurn || u.CanActThisTurn);
        GUI.Label(new Rect(20, Screen.height - 60, 180, 20), $"Active: {activeUnits.Count}");
        GUI.Label(new Rect(20, Screen.height - 40, 180, 20), $"With Actions: {unitsWithActions}");
    }
}

/// <summary>
/// Enum representing the different phases of a turn
/// </summary>
public enum TurnPhase
{
    PlayerPhase,
    EnemyPhase
}
