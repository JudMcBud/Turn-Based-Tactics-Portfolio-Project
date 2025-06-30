using System.Linq;
using UnityEngine;

/// <summary>
/// Core unit class for turn-based tactics game
/// Handles unit stats, positioning, team affiliation, and state management
/// </summary>
public class Unit : MonoBehaviour
{
    [Header("Unit Identity")]
    public string unitName = "Unit";
    public UnitType unitType = UnitType.Infantry;
    public Team team = Team.Player;

    [Header("Core Stats")]
    [SerializeField] private int maxHealthPoints = 100;
    [SerializeField] private int currentHealthPoints;
    [SerializeField] private int attackPower = 20;
    [SerializeField] private int defense = 10;
    [SerializeField] private int resistance = 10;
    [SerializeField] private int speed = 15;

    [Header("Movement")]
    [SerializeField] private int movementDistance = 2;
    [SerializeField] private bool canMoveThisTurn = true;
    [SerializeField] private bool canActThisTurn = true;

    [Header("Grid Position")]
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private Cell occupiedCell;

    [Header("Status Effects")]
    [SerializeField] private bool isStunned = false;
    [SerializeField] private bool isPoisoned = false;
    [SerializeField] private bool isBuffed = false;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject selectionIndicator;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material defaultMaterial;
    private Renderer unitRenderer;

    [Header("References")]
    public GridManager gridManager;
    public SelectionManager selectionManager;

    // Properties for safe access to stats
    public int MaxHealthPoints => maxHealthPoints;
    public int CurrentHealthPoints => currentHealthPoints;
    public int AttackPower => GetModifiedAttack();
    public int Defense => GetModifiedDefense();
    public int Resistance => GetModifiedResistance();
    public int Speed => GetModifiedSpeed();
    public int MovementDistance => GetModifiedMovementDistance();

    public Vector2Int GridPosition => gridPosition;
    public Cell OccupiedCell => occupiedCell;
    public Team Team => team;
    public UnitType UnitType => unitType;
    public bool IsAlive => currentHealthPoints > 0;
    public bool CanMoveThisTurn => canMoveThisTurn && !isStunned && IsAlive;
    public bool CanActThisTurn => canActThisTurn && !isStunned && IsAlive;

    // Events
    public System.Action<Unit> OnUnitDeath;
    public System.Action<Unit, int> OnHealthChanged;
    public System.Action<Unit, Vector2Int> OnPositionChanged;
    public System.Action<Unit> OnUnitSelected;
    public System.Action<Unit> OnTurnStart;
    public System.Action<Unit> OnTurnEnd;

    private void Awake()
    {
        // Initialize health to max
        currentHealthPoints = maxHealthPoints;

        // Find managers if not assigned
        if (gridManager == null)
            gridManager = ComponentFinder.GetGridManager();
        if (selectionManager == null)
            selectionManager = ComponentFinder.GetSelectionManager();

        // Ensure unit has a collider for mouse detection
        if (GetComponent<Collider>() == null)
        {
            // Add a capsule collider if none exists
            CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
            collider.height = 2.0f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1, 0);
            Debug.Log($"Added collider to unit: {unitName}");
        }

        // Get the unit renderer for visual feedback
        unitRenderer = GetComponent<Renderer>();
        if (unitRenderer == null)
            unitRenderer = GetComponentInChildren<Renderer>();

        // Initialize selection indicator as hidden
        UpdateSelectionIndicator(false);
    }

    private void Start()
    {
        // Set initial grid position based on world position
        UpdateGridPositionFromWorldPosition();

        // Register with the cell we're occupying
        if (occupiedCell != null)
        {
            occupiedCell.SetOccupant(gameObject);
        }
    }

    private void OnEnable()
    {
        // Reset visual state
        if (unitRenderer != null && defaultMaterial != null)
        {
            unitRenderer.material = defaultMaterial;
        }

        // Update the selection indicator state
        UpdateSelectionIndicator();
    }

    private void OnDestroy()
    {
        // Clean up cell occupation when unit is destroyed
        if (occupiedCell != null)
        {
            occupiedCell.SetOccupant(null);
        }

        // Remove from selection if currently selected
        if (selectionManager != null && selectionManager.CurrentSelectedUnit == this)
        {
            selectionManager.DeselectCurrentUnit();
        }
    }

    #region Health Management

    /// <summary>
    /// Takes damage and handles death if health reaches 0
    /// </summary>
    public void TakeDamage(int damage, DamageType damageType = DamageType.Physical)
    {
        if (!IsAlive) return;

        int finalDamage = CalculateDamage(damage, damageType);
        currentHealthPoints = Mathf.Max(0, currentHealthPoints - finalDamage);

        Debug.Log($"{unitName} took {finalDamage} damage. Health: {currentHealthPoints}/{maxHealthPoints}");

        OnHealthChanged?.Invoke(this, -finalDamage);

        if (currentHealthPoints <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals the unit
    /// </summary>
    public void Heal(int healAmount)
    {
        if (!IsAlive) return;

        int actualHeal = Mathf.Min(healAmount, maxHealthPoints - currentHealthPoints);
        currentHealthPoints += actualHeal;

        Debug.Log($"{unitName} healed for {actualHeal} HP. Health: {currentHealthPoints}/{maxHealthPoints}");

        OnHealthChanged?.Invoke(this, actualHeal);
    }

    /// <summary>
    /// Calculates final damage after defense/resistance
    /// </summary>
    private int CalculateDamage(int baseDamage, DamageType damageType)
    {
        int defense = damageType == DamageType.Physical ? Defense : Resistance;
        int finalDamage = Mathf.Max(1, baseDamage - defense); // Minimum 1 damage
        return finalDamage;
    }

    /// <summary>
    /// Handles unit death
    /// </summary>
    private void Die()
    {
        Debug.Log($"{unitName} has died!");

        // Clear cell occupation
        if (occupiedCell != null)
        {
            occupiedCell.SetOccupant(null);
        }

        OnUnitDeath?.Invoke(this);

        // Visual death effect could go here
        gameObject.SetActive(false);
    }

    #endregion

    #region Position Management

    /// <summary>
    /// Updates grid position based on current world position
    /// </summary>
    public void UpdateGridPositionFromWorldPosition()
    {
        if (gridManager == null) return;

        Cell newCell = gridManager.GetCellAtWorldPosition(transform.position);
        if (newCell != null)
        {
            SetGridPosition(newCell.gridX, newCell.gridY);
        }
    }

    /// <summary>
    /// Sets the unit's grid position and updates references
    /// </summary>
    public void SetGridPosition(int x, int y)
    {
        Vector2Int oldPosition = gridPosition;
        gridPosition = new Vector2Int(x, y);

        // Clear old cell
        if (occupiedCell != null)
        {
            occupiedCell.SetOccupant(null);
        }

        // Set new cell
        if (gridManager != null)
        {
            occupiedCell = gridManager.GetCell(x, y);
            if (occupiedCell != null)
            {
                occupiedCell.SetOccupant(gameObject);
                // Update world position to match cell
                transform.position = occupiedCell.GetWorldPosition();
                transform.position += Vector3.up * 0.8f;
            }
        }

        OnPositionChanged?.Invoke(this, oldPosition);
    }

    /// <summary>
    /// Moves unit to target cell if possible
    /// </summary>
    public bool MoveTo(Cell targetCell)
    {
        if (!CanMoveThisTurn || targetCell == null || !targetCell.CanMoveTo())
        {
            return false;
        }

        // Check if target is within movement range
        Cell[] validCells = GetMovementRange();
        if (validCells == null || validCells.Length == 0)
        {
            Debug.Log($"{unitName} has no valid movement range!");
            return false;
        }

        if (!validCells.Contains(targetCell))
        {
            Debug.Log($"{unitName} cannot move to the target cell - out of range!");
            return false;
        }

        SetGridPosition(targetCell.gridX, targetCell.gridY);
        canMoveThisTurn = false; // Used movement for this turn

        Debug.Log($"{unitName} moved to ({targetCell.gridX}, {targetCell.gridY})");
        return true;
    }

    #endregion

    #region Turn Management

    /// <summary>
    /// Called at the start of this unit's turn
    /// </summary>
    public void StartTurn()
    {
        canMoveThisTurn = true;
        canActThisTurn = true;

        // Handle status effects
        ProcessStatusEffects();

        OnTurnStart?.Invoke(this);
        Debug.Log($"{unitName}'s turn started");
    }

    /// <summary>
    /// Called at the end of this unit's turn
    /// </summary>
    public void EndTurn()
    {
        OnTurnEnd?.Invoke(this);
        Debug.Log($"{unitName}'s turn ended");
    }

    /// <summary>
    /// Process status effects at turn start
    /// </summary>
    private void ProcessStatusEffects()
    {
        StatusEffectUtilities.ProcessTurnEffects(this, isPoisoned);
    }

    #endregion

    #region Stat Modifiers

    /// <summary>
    /// Gets attack power with modifiers applied
    /// </summary>
    private int GetModifiedAttack()
    {
        return StatusEffectUtilities.ApplyStatusModifiers(attackPower, isBuffed, isStunned, StatType.Combat);
    }

    /// <summary>
    /// Gets defense with modifiers applied
    /// </summary>
    private int GetModifiedDefense()
    {
        return StatusEffectUtilities.ApplyStatusModifiers(defense, isBuffed, isStunned, StatType.Combat);
    }

    /// <summary>
    /// Gets resistance with modifiers applied
    /// </summary>
    private int GetModifiedResistance()
    {
        return StatusEffectUtilities.ApplyStatusModifiers(resistance, isBuffed, isStunned, StatType.Combat);
    }

    /// <summary>
    /// Gets speed with modifiers applied
    /// </summary>
    private int GetModifiedSpeed()
    {
        return StatusEffectUtilities.ApplyStatusModifiers(speed, isBuffed, isStunned, StatType.Combat);
    }

    /// <summary>
    /// Gets movement distance with modifiers applied
    /// </summary>
    private int GetModifiedMovementDistance()
    {
        return StatusEffectUtilities.ApplyStatusModifiers(movementDistance, isBuffed, isStunned, StatType.Movement);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Applies a status effect to the unit
    /// </summary>
    public void ApplyStatusEffect(StatusEffect effect, int duration = 1)
    {
        switch (effect)
        {
            case StatusEffect.Stunned:
                isStunned = true;
                break;
            case StatusEffect.Poisoned:
                isPoisoned = true;
                break;
            case StatusEffect.Buffed:
                isBuffed = true;
                break;
        }

        Debug.Log($"{unitName} is now {effect}");

        // You could implement a timer system for temporary effects
    }

    /// <summary>
    /// Removes a status effect from the unit
    /// </summary>
    public void RemoveStatusEffect(StatusEffect effect)
    {
        switch (effect)
        {
            case StatusEffect.Stunned:
                isStunned = false;
                break;
            case StatusEffect.Poisoned:
                isPoisoned = false;
                break;
            case StatusEffect.Buffed:
                isBuffed = false;
                break;
        }

        Debug.Log($"{unitName} is no longer {effect}");
    }

    /// <summary>
    /// Gets all cells within movement range
    /// </summary>
    public Cell[] GetMovementRange()
    {
        if (gridManager == null) return new Cell[0];

        Cell[] allCellsInRange = GridUtilities.GetCellsInRange(gridPosition, MovementDistance, gridManager, false);

        // Filter for walkable cells only
        return allCellsInRange.Where(cell => cell.CanMoveTo()).ToArray();
    }

    /// <summary>
    /// Checks if this unit can attack another unit
    /// </summary>
    public bool CanAttack(Unit target)
    {
        if (!CanActThisTurn || target == null || !target.IsAlive || target.Team == team)
            return false;

        // Check if target is within attack range (adjacent for now)
        int distance = GridUtilities.GetManhattanDistance(gridPosition, target.GridPosition);

        return distance == 1; // Adjacent cells only
    }

    /// <summary>
    /// Attacks another unit
    /// </summary>
    public void Attack(Unit target)
    {
        if (!CanAttack(target)) return;

        target.TakeDamage(AttackPower, DamageType.Physical);
        canActThisTurn = false;

        Debug.Log($"{unitName} attacks {target.unitName}!");
    }

    /// <summary>
    /// Shows or hides the selection indicator
    /// </summary>
    public void SetSelected(bool selected)
    {
        UpdateSelectionIndicator(selected);

        // Change material if materials are assigned
        if (unitRenderer != null && selectedMaterial != null && defaultMaterial != null)
        {
            unitRenderer.material = selected ? selectedMaterial : defaultMaterial;
        }
    }

    /// <summary>
    /// Updates the selection indicator visual
    /// </summary>
    private void UpdateSelectionIndicator(bool show = false)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(show);
        }
    }

    #endregion

    #region Mouse Detection

    private void OnMouseDown()
    {
        // Check if this unit can be selected (only select our own units)
        if (team == Team.Player && selectionManager != null)
        {
            selectionManager.SelectUnit(this);
        }
        else if (selectionManager != null)
        {
            Debug.Log($"Cannot select {unitName} - not a player unit or no selection manager");
        }
    }

    // ...existing code...
}
#endregion
#region Enums

public enum Team
{
    Player,
    Enemy,
    Neutral
}

public enum UnitType
{
    Infantry,
    Archer,
    Mage,
    Knight,
    Healer,
    Scout
}

public enum DamageType
{
    Physical,
    Magical
}

public enum StatusEffect
{
    Stunned,
    Poisoned,
    Buffed,
    Slowed,
    Hasted
}

#endregion
