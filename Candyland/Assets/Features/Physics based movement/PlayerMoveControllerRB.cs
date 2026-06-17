using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMoveControllerRB : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _crouchSpeed = 3f;
    [SerializeField] private float _rotationSpeed = 30f;
    [SerializeField] private float _maxSlopeAngle = 45f;
    [SerializeField] private float _slopeSpeedMultiplier = 0.03f;
    [SerializeField] private float _airSpeedMultiplier = 0.03f;


    [Header("Jumping")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _groundCheckDistance = 0.2f;

    [Header("Crouching")]
    [SerializeField] private float _standingHeight = 2f;
    [SerializeField] private float _crouchingHeight = 1f;
    [SerializeField] private float _crouchTransitionSpeed = 10f;

    [Header("References")]
    [SerializeField] private CinemachineOrbitalFollow _orbitalFollow;
    [SerializeField] private Transform _playerItems;

    private Rigidbody _rb;
    private CapsuleCollider _capsule;
    private Vector2 _moveInput;
    private bool _isGrounded;
    private bool _isCrouching;
    private bool _slope;
    private RaycastHit _groundHit;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        _playerItems.transform.position +=  new Vector3(0, _capsule.height/ 2, 0);
    }

    private void FixedUpdate()
    {
        CheckGround();
        if(_moveInput != Vector2.zero)
            UpdateRotation();
        Move();
        SmoothCrouch();
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(origin, Vector3.down, out _groundHit, _groundCheckDistance + 0.1f))
        {
            float angle = Vector3.Angle(_groundHit.normal, Vector3.up);
            _isGrounded = true;
            _slope = angle >= _maxSlopeAngle;
            Debug.Log($"{gameObject.transform.parent.name}: " + angle);
        }
        else
        {
            _isGrounded = false;
            _slope = false;
        }
    }
    
    private void UpdateRotation()
    {
        transform.rotation =
            Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, _orbitalFollow.HorizontalAxis.Value, 0), _rotationSpeed * Time.deltaTime);
    }

    private void Move()
    {
        float speed = _isCrouching ? _crouchSpeed : _moveSpeed;

        Vector3 inputDirection =
            transform.forward * _moveInput.y +
            transform.right * _moveInput.x;

        // ✅ Project movement onto ground plane (prevents climbing)
        Vector3 moveDirection = Vector3.ProjectOnPlane(inputDirection, _groundHit.normal).normalized;

        Vector3 targetVelocity = moveDirection * speed;
        Vector3 currentVelocity = _rb.linearVelocity;

        Vector3 velocityChange = new Vector3(
            targetVelocity.x - currentVelocity.x,
            0,
            targetVelocity.z - currentVelocity.z
        );

        // ✅ Reduce control in air
        float controlMultiplier = (_isGrounded ? 1f : _airSpeedMultiplier) * (_slope ? _slopeSpeedMultiplier : 1f);

        _rb.AddForce(velocityChange * controlMultiplier, ForceMode.VelocityChange);
    }

    private void SmoothCrouch()
    {
        float targetHeight = _isCrouching ? _crouchingHeight : _standingHeight;

        _capsule.height = Mathf.Lerp(
            _capsule.height,
            targetHeight,
            Time.deltaTime * _crouchTransitionSpeed
        );

        _capsule.center = new Vector3(0, _capsule.height/ 2, 0);
    }
    
    
    
    // Input System Callbacks
    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && _isGrounded)
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }

    public void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            _isCrouching = !_isCrouching;
        }
    }
}