using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class RainbowStartPoint : MonoBehaviour, IInteractable
{
    [Header("References")] 
    [SerializeField] private Transform _pivot;
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private Sprite _texture;

    [Header("Ray")] [SerializeField] private float _rainbowWidth;
    [SerializeField] private float _maxDistance;
    [SerializeField] private float _maxRaycastAmounts;
    
    private bool _active;

    [SerializeField] private List<Vector3> _rainbowPathPoints = new List<Vector3>();
    private bool _rainbowChanged;
    
    
    public void Started(PlayerInput playerInput)
    {
        _active = true;
    }
    
    public void Canceled(PlayerInput playerInput)
    {
        _active = false;
        _meshFilter.mesh = null;
    }
    
    
    public void LateUpdate()
    {
        if(!_active)
            return;
        

        CalculateRainbowPath();
        if (_rainbowChanged)
            Visualize();
    }

    private void CalculateRainbowPath()
    {
        List<Vector3> rainbowPathPoints = new List<Vector3>();
        int rayCasts = 0;


        rainbowPathPoints.Add(_pivot.position);

        Vector3 position = _pivot.position;
        Vector3 direction = _pivot.forward;


        while (rayCasts < _maxRaycastAmounts)
        {
            rayCasts++;
            if (Physics.Raycast(position, direction, out var hit, _maxDistance))
            {
                if (hit.transform.CompareTag("RainbowMirror"))
                {
                    direction = Vector3.Reflect(direction, hit.normal);
                    position = hit.point;
                    rainbowPathPoints.Add(hit.point);
                    continue;
                }

                if (hit.transform.CompareTag("RainbowEndpoint"))
                {
                    hit.collider.gameObject.GetComponent<RainbowEndpoint>().Hit();
                }

                position = hit.point;
                rainbowPathPoints.Add(hit.point);
                break;
            }
            else
            {
                rainbowPathPoints.Add(rainbowPathPoints[^1] + direction * _maxDistance);
                break;
            }
        }

        _rainbowChanged = false;
        if (rainbowPathPoints.Count == _rainbowPathPoints.Count)
        {
            for (int i = 0; i < rainbowPathPoints.Count; i++)
            {
                if (rainbowPathPoints[i] != _rainbowPathPoints[i])
                {
                    _rainbowChanged = true;
                    break;
                }
            }
        }
        else
        {
            _rainbowChanged = true;
        }

        if (_rainbowChanged)
            _rainbowPathPoints = rainbowPathPoints;
    }

    private void Visualize()
    {
        //Create Mesh
        var mesh = new Mesh();
        mesh.name = "LaserBeam";

        int segmentCount = _rainbowPathPoints.Count - 1;

        var vertices = new List<Vector3>(segmentCount * 4);
        var uvs = new List<Vector2>(segmentCount * 4);
        var triangles = new List<int>(segmentCount * 6);

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 p0 = _rainbowPathPoints[i];
            Vector3 p1 = _rainbowPathPoints[i + 1];
            
            Vector3 side = Vector3.up * _rainbowWidth * 0.5f;

            // Vertices
            // V1-------V3
            // |        |
            // |        |
            // V0-------V2
            int baseIndex = vertices.Count;

            vertices.Add(p0 - side); // v0
            vertices.Add(p0 + side); // v1
            vertices.Add(p1 - side); // v2
            vertices.Add(p1 + side); // v3

            // UVs 
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));

            // Triangles (front)
            triangles.Add(baseIndex + 0);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);

            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 3);
        }

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles,0);
        mesh.RecalculateNormals();
        
        _meshFilter.mesh = mesh;
    }
    
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 1; i < _rainbowPathPoints.Count; i++)
        {
            Gizmos.DrawLine(_rainbowPathPoints[i-1], _rainbowPathPoints[i]);
        }
    }
}