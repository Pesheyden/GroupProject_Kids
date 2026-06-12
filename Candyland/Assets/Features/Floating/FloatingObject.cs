using Seb.Fluid.Simulation;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FloatingObject : MonoBehaviour
{
    private Rigidbody _rb;
    private Collider _collider;

    [SerializeField] private FluidSim _waterSimulation;
    [SerializeField] private float _floatDrag = 3f;
    [SerializeField] private float _floatUpAcceleration = 5f;
    [SerializeField] private float _sinkAcceleration = 1f;
    [SerializeField] private float _maxUpSpeed = 2f;
    [SerializeField] private float _maxDownSpeed = -1f;
    [SerializeField] private int _waterCheckEveryFrames = 10;

    private Vector3[] _waterParticlePositions;
    private int _waterCheckCounter;
    private bool _isInWater;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    private void Start()
    {
        if (_waterSimulation != null)
        {
            _waterSimulation.IgnoreCollider(_collider);
        }
    }

    private void FixedUpdate()
    {
        _waterCheckCounter++;

        if (_waterCheckCounter >= _waterCheckEveryFrames)
        {
            _waterCheckCounter = 0;
            _isInWater = IsTouchingWaterParticles();
        }

        if (_isInWater)
        {
            Floating();
        }
        else
        {
            _rb.useGravity = true;
            _rb.linearDamping = 0f;
        }
    }

    private void Floating()
    {
        _rb.useGravity = false;
        _rb.linearDamping = _floatDrag;

        _rb.AddForce(Vector3.up * _floatUpAcceleration, ForceMode.Acceleration);
        _rb.AddForce(Vector3.down * _sinkAcceleration, ForceMode.Acceleration);

        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, Mathf.Clamp(_rb.linearVelocity.y, _maxDownSpeed, _maxUpSpeed), _rb.linearVelocity.z);
    }

    private bool IsTouchingWaterParticles()
    {
        if (_waterSimulation == null || _waterSimulation.positionBuffer == null)
            return false;

        if (_waterParticlePositions == null || _waterParticlePositions.Length != _waterSimulation.positionBuffer.count)
        {
            _waterParticlePositions = new Vector3[_waterSimulation.positionBuffer.count];
        }

        _waterSimulation.positionBuffer.GetData(_waterParticlePositions);

        Vector3 center = _collider.bounds.center;
        float radius = _collider.bounds.extents.magnitude;

        for (int i = 0; i < _waterParticlePositions.Length; i++)
        {
            if (Vector3.Distance(center, _waterParticlePositions[i]) < radius)
                return true;
        }

        return false;
    }
}