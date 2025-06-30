# Unit Selection and Movement System

This system allows you to select Units with your mouse and select cells for them to move to using Unity's new Input System.

## How It Works

### 1. Unit Selection

- Click on any player unit (Team.Player) to select it
- The selected unit will be visually highlighted
- The unit's movement range will be shown as highlighted cells
- Only one unit can be selected at a time

### 2. Unit Movement

- After selecting a unit, click on any highlighted cell within movement range
- The unit will move to that cell if the movement is valid
- Movement range updates after each move

### 3. Selection Management

- Click on empty space to deselect everything
- Click on a different unit to switch selection
- Enemy units cannot be selected (but this can be changed in Unit.OnMouseDown())

## Setup Requirements

### Scene Setup

1. **GridManager**: Manages the grid and cells

   - Assign a Cell prefab
   - Set grid width and height
   - Reference to SelectionManager

2. **SelectionManager**: Handles selection logic

   - Reference to GridManager
   - Configure selection settings (highlighting, debug, etc.)

3. **TacticsInputManager**: Handles mouse input

   - Assign Input Actions Asset
   - Reference to camera, GridManager, and SelectionManager

4. **Units**: Unit GameObjects with the Unit component
   - Must have a Collider for mouse detection (auto-added if missing)
   - Set team to Team.Player for selectable units
   - References to GridManager and SelectionManager

### Prefab Requirements

#### Cell Prefab

- GameObject with Cell component
- Renderer component for visual feedback
- Collider for mouse detection (if using OnMouseDown)

#### Unit Prefab

- GameObject with Unit component
- Collider component (CapsuleCollider auto-added if missing)
- Renderer component for visual feedback
- Optional: Selection indicator GameObject
- Optional: Selected/Default materials for visual feedback

## Code Architecture

### Core Components

1. **Unit.cs**: Core unit class with stats, positioning, and mouse detection
2. **Cell.cs**: Grid cell with properties, occupancy, and visual states
3. **SelectionManager.cs**: Central selection logic and state management
4. **GridManager.cs**: Grid creation, cell management, and highlighting
5. **TacticsInputManager.cs**: Input handling with raycast detection

### Key Features

- **Raycast Detection**: Properly detects clicks on units vs cells
- **Visual Feedback**: Selected units and movement ranges are highlighted
- **Event System**: Components communicate via events for loose coupling
- **Robust State Management**: Proper selection/deselection handling
- **Flexible Grid System**: Supports different cell types and properties

## Testing

Use the **UnitSelectionTest** script to verify functionality:

- Spawns test units automatically
- Provides debug output for all selection events
- Includes context menu methods for manual testing
- On-screen UI for easy testing

## Configuration

### SelectionManager Settings

- `allowMultipleSelection`: Enable/disable multi-selection (currently false)
- `showSelectionDebug`: Enable debug logging
- `showMovementRange`: Show/hide movement range highlighting

### Unit Settings

- `team`: Set to Team.Player for selectable units
- `movementDistance`: How far the unit can move in one turn
- Visual feedback materials and selection indicators

## Troubleshooting

1. **Units not selectable**: Check that units have Team.Player and colliders
2. **No visual feedback**: Ensure units have Renderer components
3. **Input not working**: Verify Input Actions asset is assigned and enabled
4. **Movement not working**: Check that cells have proper walkability settings

## Extension Points

- Add different selection rules (e.g., select enemy units for info)
- Implement multi-unit selection
- Add drag selection
- Enhance visual feedback with animations
- Add sound effects for selection/movement
- Implement formation movement for multiple units
