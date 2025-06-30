using UnityEngine;
using UnityEditor;

/// <summary>
/// Setup utility for the Unit Selection and Movement System
/// Use this to quickly set up the system in a new scene
/// </summary>
public class UnitSelectionSystemSetup : MonoBehaviour
{
    [Header("System Setup")]
    [SerializeField] private bool autoSetupOnStart = false;

    [Header("Prefabs")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject unitPrefab;

    [Header("Grid Configuration")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 8;

    [Header("Test Configuration")]
    [SerializeField] private bool spawnTestUnits = true;
    [SerializeField] private int testUnitCount = 3;

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupSystem();
        }
    }

    [ContextMenu("Setup Complete System")]
    public void SetupSystem()
    {
        Debug.Log("=== Setting up Unit Selection System ===");

        // Create main camera if none exists
        SetupCamera();

        // Create GridManager
        SetupGridManager();

        // Create SelectionManager
        SetupSelectionManager();

        // Create TacticsInputManager
        SetupInputManager();

        // Create test script
        SetupTestScript();

        // Spawn test units if requested
        if (spawnTestUnits)
        {
            SpawnTestUnits();
        }

        Debug.Log("✓ Unit Selection System setup complete!");
        Debug.Log("You can now click on units to select them and click on highlighted cells to move them.");
    }

    private void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";

            // Position camera for tactical view
            cameraObject.transform.position = new Vector3(4, 10, 4);
            cameraObject.transform.rotation = Quaternion.Euler(45, 0, 0);

            Debug.Log("✓ Created Main Camera");
        }
    }

    private void SetupGridManager()
    {
        GridManager gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null)
        {
            GameObject gridObject = new GameObject("GridManager");
            gridManager = gridObject.AddComponent<GridManager>();

            // Configure grid settings
            gridManager.gridWidth = gridWidth;
            gridManager.gridHeight = gridHeight;
            gridManager.cellPrefab = cellPrefab;

            Debug.Log($"✓ Created GridManager ({gridWidth}x{gridHeight})");
        }
        else
        {
            Debug.Log("✓ GridManager already exists");
        }
    }

    private void SetupSelectionManager()
    {
        SelectionManager selectionManager = FindFirstObjectByType<SelectionManager>();
        if (selectionManager == null)
        {
            GameObject selectionObject = new GameObject("SelectionManager");
            selectionManager = selectionObject.AddComponent<SelectionManager>();

            // Link to GridManager
            GridManager gridManager = FindFirstObjectByType<GridManager>();
            selectionManager.gridManager = gridManager;

            Debug.Log("✓ Created SelectionManager");
        }
        else
        {
            Debug.Log("✓ SelectionManager already exists");
        }
    }

    private void SetupInputManager()
    {
        TacticsInputManager inputManager = FindFirstObjectByType<TacticsInputManager>();
        if (inputManager == null)
        {
            GameObject inputObject = new GameObject("TacticsInputManager");
            inputManager = inputObject.AddComponent<TacticsInputManager>();

            // Find and assign Input Actions asset
            var inputActionsAssets = Resources.FindObjectsOfTypeAll<UnityEngine.InputSystem.InputActionAsset>();
            if (inputActionsAssets.Length > 0)
            {
                inputManager.inputActions = inputActionsAssets[0];
                Debug.Log($"✓ Assigned Input Actions: {inputActionsAssets[0].name}");
            }
            else
            {
                Debug.LogWarning("⚠ No Input Actions asset found. Please assign one manually.");
            }

            // Link to other managers
            inputManager.mainCamera = Camera.main;
            inputManager.gridManager = FindFirstObjectByType<GridManager>();
            inputManager.selectionManager = FindFirstObjectByType<SelectionManager>();

            Debug.Log("✓ Created TacticsInputManager");
        }
        else
        {
            Debug.Log("✓ TacticsInputManager already exists");
        }
    }

    private void SetupTestScript()
    {
        UnitSelectionTest testScript = FindFirstObjectByType<UnitSelectionTest>();
        if (testScript == null)
        {
            GameObject testObject = new GameObject("UnitSelectionTest");
            testScript = testObject.AddComponent<UnitSelectionTest>();

            // Configure test script
            testScript.unitPrefab = unitPrefab;
            testScript.gridManager = FindFirstObjectByType<GridManager>();
            testScript.selectionManager = FindFirstObjectByType<SelectionManager>();
            testScript.inputManager = FindFirstObjectByType<TacticsInputManager>();
            testScript.testUnitsToSpawn = testUnitCount;
            testScript.spawnUnitsOnStart = false; // We'll spawn them manually

            Debug.Log("✓ Created UnitSelectionTest");
        }
        else
        {
            Debug.Log("✓ UnitSelectionTest already exists");
        }
    }

    private void SpawnTestUnits()
    {
        if (unitPrefab == null)
        {
            Debug.LogWarning("⚠ No unit prefab assigned for test units");
            return;
        }

        GridManager gridManager = FindFirstObjectByType<GridManager>();
        SelectionManager selectionManager = FindFirstObjectByType<SelectionManager>();

        for (int i = 0; i < testUnitCount; i++)
        {
            Vector3 spawnPosition = new Vector3(i * 2, 0, 0);
            GameObject unitObject = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
            unitObject.name = $"TestUnit_{i}";

            Unit unit = unitObject.GetComponent<Unit>();
            if (unit != null)
            {
                unit.unitName = $"Test Unit {i + 1}";
                unit.team = Team.Player;
                unit.gridManager = gridManager;
                unit.selectionManager = selectionManager;

                Debug.Log($"✓ Spawned {unit.unitName} at {spawnPosition}");
            }
        }
    }

    [ContextMenu("Create Basic Cell Prefab")]
    public void CreateBasicCellPrefab()
    {
#if UNITY_EDITOR
        // Create a basic cell prefab if none exists
        GameObject cellObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cellObject.name = "Cell";
        cellObject.transform.localScale = new Vector3(1, 0.1f, 1);
        
        // Add Cell component
        Cell cellComponent = cellObject.AddComponent<Cell>();
        
        // Configure renderer
        Renderer renderer = cellObject.GetComponent<Renderer>();
        cellComponent.cellRenderer = renderer;
        
        // Create material
        Material cellMaterial = new Material(Shader.Find("Standard"));
        cellMaterial.color = Color.white;
        renderer.material = cellMaterial;
        
        // Save as prefab
        string prefabPath = "Assets/Prefabs/Cell.prefab";
        
        // Create Prefabs folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cellObject, prefabPath);
        DestroyImmediate(cellObject);
        
        cellPrefab = prefab;
        Debug.Log($"✓ Created basic cell prefab at {prefabPath}");
#endif
    }

    [ContextMenu("Create Basic Unit Prefab")]
    public void CreateBasicUnitPrefab()
    {
#if UNITY_EDITOR
        // Create a basic unit prefab if none exists
        GameObject unitObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        unitObject.name = "Unit";
        unitObject.transform.localScale = new Vector3(0.8f, 1, 0.8f);
        
        // Add Unit component
        Unit unitComponent = unitObject.AddComponent<Unit>();
        
        // Configure renderer
        Renderer renderer = unitObject.GetComponent<Renderer>();
        
        // Create material
        Material unitMaterial = new Material(Shader.Find("Standard"));
        unitMaterial.color = Color.blue;
        renderer.material = unitMaterial;
        
        // The Unit component will add a collider automatically
        
        // Save as prefab
        string prefabPath = "Assets/Prefabs/Unit.prefab";
        
        // Create Prefabs folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(unitObject, prefabPath);
        DestroyImmediate(unitObject);
        
        unitPrefab = prefab;
        Debug.Log($"✓ Created basic unit prefab at {prefabPath}");
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(UnitSelectionSystemSetup))]
    public class UnitSelectionSystemSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            GUILayout.Space(10);
            
            UnitSelectionSystemSetup setup = (UnitSelectionSystemSetup)target;
            
            if (GUILayout.Button("Setup Complete System", GUILayout.Height(30)))
            {
                setup.SetupSystem();
            }
            
            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Cell Prefab"))
            {
                setup.CreateBasicCellPrefab();
            }
            if (GUILayout.Button("Create Unit Prefab"))
            {
                setup.CreateBasicUnitPrefab();
            }
            GUILayout.EndHorizontal();
        }
    }
#endif
}
