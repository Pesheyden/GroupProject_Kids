using System.Collections;
using UnityEngine;

public class RespawnHandler : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private float _respawnTime;
    
    [NaughtyAttributes.ReadOnly] [SerializeField]
    private Vector3 _spawnPoint;
    
    
    public void SetSpawnPoint(Vector3 spawnPoint)
    {
        _spawnPoint = spawnPoint;
    }

    public void Respawn()
    {
        StartCoroutine(CurveMoveCoroutine(transform.position, _spawnPoint));
    }
    
    private IEnumerator CurveMoveCoroutine(Vector3 start, Vector3 end)
    {
        float t;
        float eclipse = 0; 
        while (eclipse < _respawnTime)
        {
            Vector3 control = (start + end) * 0.5f + Vector3.up * Mathf.Sqrt( (start - end).sqrMagnitude) / 2;

            t = eclipse / _respawnTime;
            
            transform.position = BezierCurve(start, control, end, t);

            eclipse += Time.deltaTime;
            yield return null;
        }
    }
    
    public static Vector3 BezierCurve(
        Vector3 start,
        Vector3 control,
        Vector3 end,
        float t)
    {
        float u = 1f - t;

        return u * u * start +
               2f * u * t * control +
               t * t * end;
    }
}
