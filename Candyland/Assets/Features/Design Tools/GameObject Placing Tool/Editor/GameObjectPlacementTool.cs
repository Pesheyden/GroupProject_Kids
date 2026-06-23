using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEditor;

/// <summary>
/// Tool where you can select a prefab to place in scene view
/// You can modify its scale and how many to spawn
/// Hold shift and select 4 point in the scene with RMB to make a spawn area
/// Press the Spawn Random Objects In Area to spawn objects
/// Press Ctrl + Z  to reverse (remove the placed prefabs)
/// After Placing objects press Clear Points button to be able to make new area
/// </summary>
public class GameObjectPlacementTool : EditorWindow
{
    private GameObject _objectToSpawn;
    private float _objectScale = 1f;
    private int _amountToSpawn = 10;

    private List<Vector3> _clickedPlaces = new List<Vector3>();

    [MenuItem("Tools/Place Objects")]
    public static void ShowWindow()
    {
        GetWindow<GameObjectPlacementTool>("Place Objects");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Placement Tool", EditorStyles.boldLabel);

        _objectToSpawn = (GameObject)EditorGUILayout.ObjectField("Prefab", _objectToSpawn, typeof(GameObject), false);
        _objectScale = EditorGUILayout.Slider("Object Scale", _objectScale, 1f, 1000f);
        _amountToSpawn = EditorGUILayout.IntField("Amount To Spawn", _amountToSpawn);

        GUILayout.Label("Clicked points: " + _clickedPlaces.Count + "/4");

        if (GUILayout.Button("Spawn Random Objects In Area"))
        {
            SpawnObjectsInArea();
        }

        if (GUILayout.Button("Clear Points"))
        {
            _clickedPlaces.Clear();
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event _event = Event.current;

        if (_event.type == EventType.MouseDown && _event.button == 0 && _event.shift)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(_event.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                if (_clickedPlaces.Count < 4)
                {
                    _clickedPlaces.Add(hitInfo.point);
                    _event.Use();
                    SceneView.RepaintAll();
                }
            }
        }

        for (int i = 0; i < _clickedPlaces.Count; i++)
        {
            Handles.SphereHandleCap(0, _clickedPlaces[i], Quaternion.identity, 0.3f, EventType.Repaint);
            Handles.Label(_clickedPlaces[i] + Vector3.up * 0.5f, "Point " + (i + 1));
        }

        if (_clickedPlaces.Count == 4)
        {
            Handles.DrawLine(_clickedPlaces[0], _clickedPlaces[1]);
            Handles.DrawLine(_clickedPlaces[1], _clickedPlaces[2]);
            Handles.DrawLine(_clickedPlaces[2], _clickedPlaces[3]);
            Handles.DrawLine(_clickedPlaces[3], _clickedPlaces[0]);
        }
    }

    private void SpawnObjectsInArea()
    {
        if (_objectToSpawn == null)
        {
            Debug.LogWarning("No prefab selected.");
            return;
        }

        if (_clickedPlaces.Count < 4)
        {
            Debug.LogWarning("Click 4 points first. Hold Shift and left-click in the Scene view.");
            return;
        }

        for (int i = 0; i < _amountToSpawn; i++)
        {
            Vector3 randomPosition = GetRandomPointInFourPointArea();
            if (Physics.Raycast(randomPosition, Vector3.down, out var hitInfo))
            {
                randomPosition = hitInfo.point;
            }

            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(_objectToSpawn);
            newObject.transform.position = randomPosition;
            newObject.transform.localScale = Vector3.one * _objectScale;

            Undo.RegisterCreatedObjectUndo(newObject, "Spawn Random Object");
        }
    }

    private Vector3 GetRandomPointInFourPointArea()
    {
        Vector3 a = _clickedPlaces[0];
        Vector3 b = _clickedPlaces[1];
        Vector3 c = _clickedPlaces[2];
        Vector3 d = _clickedPlaces[3];

        float randomX = Random.value;
        float randomY = Random.value;

        Vector3 pointAB = Vector3.Lerp(a, b, randomX);
        Vector3 pointDC = Vector3.Lerp(d, c, randomX);

        return Vector3.Lerp(pointAB, pointDC, randomY);
    }
}
