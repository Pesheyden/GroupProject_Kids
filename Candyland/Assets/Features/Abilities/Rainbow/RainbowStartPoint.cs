using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainbowStartPoint : MonoBehaviour, IInteractable
{
    [Header("References")] 
    [SerializeField] private Transform _pivot;
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private Sprite _texture;

    [Header("Settings")] [SerializeField] private float _rainbowWidth;
    [SerializeField] private float _maxDistance;
    [SerializeField] private float _maxRaycastAmounts;

    private bool _active;

    [SerializeField] private List<Vector3> _rainbowPathPoints = new List<Vector3>();
    private bool _rainbowChanged;

    public void Activate()
    {
        _active = !_active;
    }

    private void LateUpdate()
    {
        if (!_active)
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
        Debug.Log("Creating Mesh");
        //Create Mesh
        Mesh mesh = new Mesh();

        List<Vector3> verticesList = new();
        List<Vector2> uvList = new();
        List<int> trianglesList = new();

        // First two vertices
        verticesList.Add(_rainbowPathPoints[0] + Vector3.up * _rainbowWidth);
        verticesList.Add(_rainbowPathPoints[0] + Vector3.down * _rainbowWidth);

        // Add placeholder UVs for first segment (will be overwritten)
        uvList.Add(Vector2.zero);
        uvList.Add(Vector2.one);

        int lastStartIndex = 0;

        for (int i = 1; i < _rainbowPathPoints.Count; i++)
        {
            // Create vertices
            Vector3 vUp = _rainbowPathPoints[i] + Vector3.up * _rainbowWidth;
            Vector3 vDown = _rainbowPathPoints[i] + Vector3.down * _rainbowWidth;

            verticesList.Add(vUp);
            verticesList.Add(vDown);

            // Create triangles
            trianglesList.Add(lastStartIndex + 1);
            trianglesList.Add(lastStartIndex);
            trianglesList.Add(lastStartIndex + 2);

            trianglesList.Add(lastStartIndex + 3);
            trianglesList.Add(lastStartIndex + 1);
            trianglesList.Add(lastStartIndex + 2);

            // --- UV CALCULATION FOR THIS SEGMENT ---

            // Segment direction
            Vector3 dir = (_rainbowPathPoints[i] - _rainbowPathPoints[i - 1]).normalized;

            // Laser is vertical, so use Y-up to compute right/forward
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(up, dir).normalized;
            Vector3 forward = Vector3.Cross(right, up).normalized;

            // Segment vertices (4 total)
            Vector3 v0 = verticesList[lastStartIndex];
            Vector3 v1 = verticesList[lastStartIndex + 1];
            Vector3 v2 = vUp;
            Vector3 v3 = vDown;

            // Compute U/V values
            float u0 = Vector3.Dot(v0, right);
            float v0p = Vector3.Dot(v0, forward);

            float u1 = Vector3.Dot(v1, right);
            float v1p = Vector3.Dot(v1, forward);

            float u2 = Vector3.Dot(v2, right);
            float v2p = Vector3.Dot(v2, forward);

            float u3 = Vector3.Dot(v3, right);
            float v3p = Vector3.Dot(v3, forward);

            // Normalize to 0–1
            float minU = Mathf.Min(u0, Mathf.Min(u1, Mathf.Min(u2, u3)));
            float maxU = Mathf.Max(u0, Mathf.Max(u1, Mathf.Max(u2, u3)));
            float minV = Mathf.Min(v0p, Mathf.Min(v1p, Mathf.Min(v2p, v3p)));
            float maxV = Mathf.Max(v0p, Mathf.Max(v1p, Mathf.Max(v2p, v3p)));

            float sizeU = maxU - minU;
            float sizeV = maxV - minV;

            // Overwrite UVs for previous segment (v0, v1)
            uvList[lastStartIndex] = new Vector2((u0 - minU) / sizeU, (v0p - minV) / sizeV);
            uvList[lastStartIndex + 1] = new Vector2((u1 - minU) / sizeU, (v1p - minV) / sizeV);

            // Add UVs for new vertices (v2, v3)
            uvList.Add(new Vector2((u2 - minU) / sizeU, (v2p - minV) / sizeV));
            uvList.Add(new Vector2((u3 - minU) / sizeU, (v3p - minV) / sizeV));

            lastStartIndex = verticesList.Count - 2;
        }

        mesh.vertices = verticesList.ToArray();
        mesh.uv = uvList.ToArray();
        mesh.triangles = trianglesList.ToArray();

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