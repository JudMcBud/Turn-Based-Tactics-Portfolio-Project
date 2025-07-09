using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for UnitSpawner to provide enhanced inspector interface
/// </summary>
#if UNITY_EDITOR
[CustomEditor(typeof(UnitSpawner))]
public class UnitSpawnerEditor : Editor
{
    private UnitSpawner spawner;
    private bool showAdvancedOptions = false;
    private bool showBatchSpawnOptions = false;

    private void OnEnable()
    {
        spawner = (UnitSpawner)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawHeader();
        GUILayout.Space(10);

        DrawQuickActions();
        GUILayout.Space(10);

        DrawBasicConfiguration();
        GUILayout.Space(10);

        DrawAdvancedOptions();
        GUILayout.Space(10);

        DrawBatchSpawnOptions();

        serializedObject.ApplyModifiedProperties();
    }

    private new void DrawHeader()
    {
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        GUILayout.Label("Unit Spawner", headerStyle);
        GUILayout.Label("Test utility for spawning units with custom stats", EditorStyles.centeredGreyMiniLabel);
    }

    private void DrawQuickActions()
    {
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        // Large spawn button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("SPAWN UNIT", GUILayout.Height(30)))
        {
            spawner.SpawnUnit();
        }
        GUI.backgroundColor = Color.white;

        // Clear button
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Clear All", GUILayout.Height(30), GUILayout.Width(80)))
        {
            if (EditorUtility.DisplayDialog("Clear Units", "Clear all spawned units?", "Yes", "Cancel"))
            {
                spawner.ClearSpawnedUnits();
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Select Target Cell"))
        {
            spawner.SelectTargetCell();
        }

        if (GUILayout.Button("Use Selected Cell"))
        {
            spawner.UseSelectedCellAsTarget();
        }

        GUILayout.EndHorizontal();
    }

    private void DrawBasicConfiguration()
    {
        EditorGUILayout.LabelField("Basic Configuration", EditorStyles.boldLabel);

        // Target position
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Target Position", EditorStyles.miniLabel);

        SerializedProperty useWorldPosProp = serializedObject.FindProperty("useWorldPosition");
        EditorGUILayout.PropertyField(useWorldPosProp);

        if (useWorldPosProp.boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetWorldPosition"));
        }
        else
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetGridX"), GUIContent.none, GUILayout.Width(150));
            EditorGUILayout.LabelField(",", GUILayout.Width(10));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetGridY"), GUIContent.none, GUILayout.Width(150));
            EditorGUILayout.LabelField("(Grid X, Y)");
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        // Unit configuration
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Unit Configuration", EditorStyles.miniLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("unitPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("customUnitName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFaction"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedUnitType"));
        EditorGUILayout.EndVertical();

        // Unit stats
        DrawUnitStats();
    }

    private void DrawUnitStats()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Unit Stats", EditorStyles.miniLabel);

        var statsProperty = serializedObject.FindProperty("unitStats");

        // Health
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Health:", GUILayout.Width(80));
        EditorGUILayout.PropertyField(statsProperty.FindPropertyRelative("maxHealthPoints"), GUIContent.none);
        EditorGUILayout.EndHorizontal();

        // Combat stats
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Attack:", GUILayout.Width(80));
        EditorGUILayout.PropertyField(statsProperty.FindPropertyRelative("attackPower"), GUIContent.none);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Defense:", GUILayout.Width(80));
        EditorGUILayout.PropertyField(statsProperty.FindPropertyRelative("defense"), GUIContent.none);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Resistance:", GUILayout.Width(80));
        EditorGUILayout.PropertyField(statsProperty.FindPropertyRelative("resistance"), GUIContent.none);
        EditorGUILayout.EndHorizontal();

        // Movement stats
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Speed:", GUILayout.Width(80));
        EditorGUILayout.PropertyField(statsProperty.FindPropertyRelative("speed"), GUIContent.none);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Movement:", GUILayout.Width(80));
        EditorGUILayout.PropertyField(statsProperty.FindPropertyRelative("movementDistance"), GUIContent.none);
        EditorGUILayout.EndHorizontal();

        // Preset buttons
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Weak", GUILayout.Height(20)))
        {
            SetStatPreset(statsProperty, 50, 10, 5, 5, 10, 1);
        }
        if (GUILayout.Button("Normal", GUILayout.Height(20)))
        {
            SetStatPreset(statsProperty, 100, 20, 10, 10, 15, 2);
        }
        if (GUILayout.Button("Strong", GUILayout.Height(20)))
        {
            SetStatPreset(statsProperty, 150, 30, 15, 15, 20, 3);
        }
        if (GUILayout.Button("Boss", GUILayout.Height(20)))
        {
            SetStatPreset(statsProperty, 300, 50, 25, 25, 10, 2);
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void SetStatPreset(SerializedProperty statsProperty, int health, int attack, int defense, int resistance, int speed, int movement)
    {
        statsProperty.FindPropertyRelative("maxHealthPoints").intValue = health;
        statsProperty.FindPropertyRelative("attackPower").intValue = attack;
        statsProperty.FindPropertyRelative("defense").intValue = defense;
        statsProperty.FindPropertyRelative("resistance").intValue = resistance;
        statsProperty.FindPropertyRelative("speed").intValue = speed;
        statsProperty.FindPropertyRelative("movementDistance").intValue = movement;
    }

    private void DrawAdvancedOptions()
    {
        showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options", true, EditorStyles.foldoutHeader);

        if (showAdvancedOptions)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoAssignManagers"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnOnStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showDebugInfo"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visual Configuration", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoColorByFaction"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerMaterial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyMaterial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("neutralMaterial"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showPreview"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("previewColor"));

            EditorGUI.indentLevel--;
        }
    }

    private void DrawBatchSpawnOptions()
    {
        showBatchSpawnOptions = EditorGUILayout.Foldout(showBatchSpawnOptions, "Batch Spawn", true, EditorStyles.foldoutHeader);

        if (showBatchSpawnOptions)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableBatchSpawn"));

            if (serializedObject.FindProperty("enableBatchSpawn").boolValue)
            {
                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("Spawn Batch", GUILayout.Height(25)))
                {
                    spawner.SpawnBatchUnits();
                }
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("Add Current Config", GUILayout.Height(25)))
                {
                    AddCurrentConfigToBatch();
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("batchSpawnList"));
            }

            EditorGUI.indentLevel--;
        }
    }

    private void AddCurrentConfigToBatch()
    {
        var batchListProperty = serializedObject.FindProperty("batchSpawnList");
        batchListProperty.arraySize++;
        var newElement = batchListProperty.GetArrayElementAtIndex(batchListProperty.arraySize - 1);

        // Copy current config to new batch element
        newElement.FindPropertyRelative("gridX").intValue = serializedObject.FindProperty("targetGridX").intValue;
        newElement.FindPropertyRelative("gridY").intValue = serializedObject.FindProperty("targetGridY").intValue;
        newElement.FindPropertyRelative("unitName").stringValue = serializedObject.FindProperty("customUnitName").stringValue;
        newElement.FindPropertyRelative("faction").enumValueIndex = serializedObject.FindProperty("selectedFaction").enumValueIndex;
        newElement.FindPropertyRelative("unitType").enumValueIndex = serializedObject.FindProperty("selectedUnitType").enumValueIndex;

        // Copy stats
        var currentStats = serializedObject.FindProperty("unitStats");
        var newStats = newElement.FindPropertyRelative("stats");
        newStats.FindPropertyRelative("maxHealthPoints").intValue = currentStats.FindPropertyRelative("maxHealthPoints").intValue;
        newStats.FindPropertyRelative("attackPower").intValue = currentStats.FindPropertyRelative("attackPower").intValue;
        newStats.FindPropertyRelative("defense").intValue = currentStats.FindPropertyRelative("defense").intValue;
        newStats.FindPropertyRelative("resistance").intValue = currentStats.FindPropertyRelative("resistance").intValue;
        newStats.FindPropertyRelative("speed").intValue = currentStats.FindPropertyRelative("speed").intValue;
        newStats.FindPropertyRelative("movementDistance").intValue = currentStats.FindPropertyRelative("movementDistance").intValue;

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
