# Turn Manager System - Implementation Complete

## Overview

The Turn Manager system provides robust turn-based game flow control with the following features:

- **Phase Management**: Tracks Player and Enemy phases within each turn
- **Unit Selection Control**: Units can only be selected during their team's phase
- **Action Refresh**: Automatically refreshes unit actions at the start of each phase
- **Context Menu**: Right-click empty cells to bring up "End Turn" context menu
- **Event System**: Comprehensive events for other systems to hook into

## Core Components

### 1. TurnManager.cs

The main controller that manages turn flow and phase transitions.

**Key Features:**

- Tracks current turn number and phase (Player/Enemy)
- Maintains cached lists of player and enemy units for performance
- Automatically refreshes unit actions when phases start
- Provides phase transition methods and validation
- Integrates with SelectionManager for context menu triggering

**Public Properties:**

```csharp
public int CurrentTurn { get; }
public TurnPhase CurrentPhase { get; }
public bool IsPlayerPhase { get; }
public bool IsEnemyPhase { get; }
```

**Key Methods:**

```csharp
public void EndCurrentPhase()           // End current phase and transition
public void RefreshActivePhaseUnits()   // Refresh actions for active team
public bool CanSelectUnit(Unit unit)    // Check if unit can be selected
public List<Unit> GetActivePhaseUnits() // Get units for current phase
```

### 2. TurnContextMenu.cs

UI component that displays when clicking empty cells during gameplay.

**Features:**

- Appears at mouse cursor position when clicking empty cells
- Shows "End Turn" and "Cancel" options
- Auto-hides after delay or when clicking elsewhere
- Confirmation dialog for ending phases with remaining actions

### 3. SelectionManager.cs (Updated)

Enhanced to respect turn phases and integrate with TurnManager.

**Key Changes:**

- Uses `ComponentFinder.GetTurnManager()` for turn validation
- `SelectUnit()` method now checks `turnManager.CanSelectUnit()` before allowing selection
- Triggers context menu when empty cells are selected during active phases

## Setup Instructions

### 1. Scene Setup

1. Create an empty GameObject named "TurnManager"
2. Add the `TurnManager` component
3. Ensure SelectionManager and GridManager are in the scene
4. Add the `TurnManagerTester` component for testing

### 2. Unit Setup

Units automatically integrate with the turn system:

- Must have a `Team` assigned (Player/Enemy/Neutral)
- Implement `StartTurn()` and `EndTurn()` methods
- Use `CanMoveThisTurn` and `CanActThisTurn` properties

### 3. Testing

Use the `TurnManagerTester` component:

- Provides manual test buttons in OnGUI
- Keyboard shortcuts: T (transition), R (refresh), U (select), C (context menu)
- Can run automated tests for validation

## Usage Examples

### Basic Turn Management

```csharp
// Get the turn manager
TurnManager turnMgr = ComponentFinder.GetTurnManager();

// Check current state
if (turnMgr.IsPlayerPhase)
{
    // Player's turn logic
}

// End the current phase
turnMgr.EndCurrentPhase();
```

### Unit Selection Validation

```csharp
// The system automatically handles this, but you can check manually:
if (turnManager.CanSelectUnit(someUnit))
{
    selectionManager.SelectUnit(someUnit);
}
```

### Event Subscription

```csharp
TurnManager turnMgr = ComponentFinder.GetTurnManager();

// Subscribe to turn events
turnMgr.OnPhaseStarted += (phase) => Debug.Log($"Phase started: {phase}");
turnMgr.OnPhaseEnded += (phase) => Debug.Log($"Phase ended: {phase}");
turnMgr.OnTurnChanged += (turn) => Debug.Log($"New turn: {turn}");
```

## Game Flow

### Turn Sequence

1. **Player Phase Starts**

   - All player units get refreshed actions (`StartTurn()` called)
   - Only player units can be selected
   - Player can move/act with their units

2. **Player Phase Ends**

   - Click empty cell → Context menu appears
   - Select "End Turn" → Player phase ends
   - `EndTurn()` called on all player units

3. **Enemy Phase Starts**

   - All enemy units get refreshed actions
   - Only enemy units can be selected (typically AI controlled)
   - Enemy AI takes actions

4. **Enemy Phase Ends**
   - AI ends phase (or timer expires)
   - Turn counter increments
   - Return to Player Phase

### Context Menu Trigger

- Click any empty, walkable cell during active phase
- Menu appears near mouse cursor
- Shows current phase in menu title
- "End Turn" button transitions to next phase
- "Cancel" button closes menu

## Integration Points

### With Unit System

- Units must implement `Team` property for phase validation
- `StartTurn()` called when unit's phase begins
- `EndTurn()` called when unit's phase ends
- `CanMoveThisTurn`/`CanActThisTurn` reset each turn

### With Selection System

- `SelectionManager` checks turn phases before allowing unit selection
- Empty cell selection triggers context menu
- All selections cleared on phase transitions

### With Input System

- `TacticsInputManager` provides mouse world position for context menu
- Mouse clicks route through selection system with turn validation

## Debugging Features

### Turn Manager Debug UI

- Shows current turn and phase
- Displays active unit count
- Shows units with remaining actions
- Enable with `showTurnDebug = true`

### Context Menu Testing

- Force show context menu with `TurnContextMenu.Show(position, turnManager)`
- Manual phase transitions with `ForceNextPhase()`
- Reset turn system with `ResetTurnSystem()`

### TurnManagerTester Component

- Provides comprehensive testing interface
- Manual test buttons and keyboard shortcuts
- Automated test sequences
- Validates all component connections

## Events Reference

```csharp
// Turn Manager Events
public System.Action<int> OnTurnChanged;           // New turn started
public System.Action<TurnPhase> OnPhaseChanged;    // Phase changed
public System.Action<TurnPhase> OnPhaseStarted;    // Phase started
public System.Action<TurnPhase> OnPhaseEnded;      // Phase ended
public System.Action OnAllUnitsRefreshed;          // All units refreshed

// Selection Manager Events
public System.Action<Cell> OnCellSelected;         // Cell selected
public System.Action<Unit> OnUnitSelected;         // Unit selected
public System.Action OnSelectionCleared;           // All selections cleared
```

## Troubleshooting

### Units Not Selectable

- Check if unit's team matches current phase
- Verify TurnManager is in scene and properly initialized
- Ensure SelectionManager has reference to TurnManager

### Context Menu Not Appearing

- Verify clicking on empty, walkable cells
- Check TurnContextMenu is properly instantiated
- Ensure TurnManager.OnCellSelected event is hooked up

### Phase Not Transitioning

- Check if context menu "End Turn" button is working
- Verify TurnManager.EndCurrentPhase() is being called
- Look for errors in turn transition logic

### Actions Not Refreshing

- Ensure `autoRefreshActions = true` in TurnManager
- Check Unit.StartTurn() method implementation
- Verify unit lists are properly maintained

## Performance Notes

- Unit lists are cached and only refreshed when needed
- ComponentFinder uses cached references to avoid repeated searches
- Context menu uses OnGUI for simplicity but could be replaced with uGUI
- Event system prevents tight coupling between components

The Turn Manager system is now fully implemented and ready for use in your turn-based tactics game!
