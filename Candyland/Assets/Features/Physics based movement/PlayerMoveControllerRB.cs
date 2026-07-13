using System;
using FMODUnity;
using ImprovedTimers;
using Seb.Fluid.Simulation;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityUtils;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMoveControllerRB : MonoBehaviour
{
    public enum MonsterSelector{Minty, Rainbow, Marsh, Prul}
    public MonsterSelector _currentMonster;

    [Header("Movement")] 
    [SerializeField] private bool _alignWithView = true;
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _crouchSpeed = 3f;
    [SerializeField] private float _rotationSpeed = 30f;
    [SerializeField] private float _slopeLimit = 15f;
    [SerializeField] private float _airSpeedMultiplier;

    [Header("Jumping")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _groundCheckDistance = 0.2f;
    [SerializeField] private float _landingCheckDistance = 0.2f;
    [SerializeField] private float _groundSphereRadius = 0.2f;
    [SerializeField] private float _landingSphereRadius = 1f;
    [SerializeField] private float _fallSpeed = 1f;

    [Header("Crouching")]
    [SerializeField] private float _standingHeight = 2f;
    [SerializeField] private float _crouchingHeight = 1f;
    [SerializeField] private float _crouchTransitionSpeed = 10f;

    [Header("Swimming")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private float _swimSpeed = 4f;
    [SerializeField] private float _swimUpAcceleration = 8f;
    [SerializeField] private float _swimDownAcceleration = 3f;
    [SerializeField] private float _maxSwimUpSpeed = 3f;
    [SerializeField] private float _maxSwimDownSpeed = -2f;
    [SerializeField] private float _swimDrag = 3f;
    [SerializeField] private float _waterExitDelay = 0.5f;
    private float _waterExitTimer;
    private bool _isSwimming;
    private bool _jumpHeld;
    private bool _crouchHeld;
    private bool _isUsingUnderwaterCamera;

    [Header("Water Particle Check")]
    [SerializeField] private FluidSim _waterSimulation;
    [SerializeField] private int _waterCheckEveryFrames = 10;
    [SerializeField] private float _waterTouchExtraRadius = 0.35f;

    private Vector3[] _waterParticlePositions;
    private int _waterCheckCounter;
    private bool _isInWaterVolume; 

    [Header("Animations")]
    [SerializeField] private Animator animator;

    [Header("Sounds")]
    [SerializeField] private GameObject walking;
    [SerializeField] private EventReference jumping;

    [Header("References")]
    public CinemachineOrbitalFollow OrbitalFollow;
    [SerializeField] private CinemachineCamera _normalCam;
    [SerializeField] private CinemachineCamera _underwaterCam;
    [SerializeField] private Transform _playerItems;

    private Rigidbody _rb;
    private CapsuleCollider _capsule;
    private Vector2 _moveInput;
    private bool _isCrouching;
    private RaycastHit _hitInfo;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if(_playerInput == null)
            _playerInput = GetComponent<PlayerInput>();

        var jumpAction = _playerInput.actions["Jump"];
        jumpAction.performed += OnJumpInput;
        jumpAction.canceled += OnJumpInput;

        var crouchAction = _playerInput.actions["Crouch"];
        crouchAction.performed += OnCrouchInput;
        crouchAction.canceled += OnCrouchInput;
    }

    private void OnJumpInput(InputAction.CallbackContext context)
    {
        _jumpHeld = context.ReadValueAsButton();
    }

    private void OnCrouchInput(InputAction.CallbackContext context)
    {
        _crouchHeld = context.ReadValueAsButton();
    }

    private void Start()
    {
        _playerItems.transform.position +=  new Vector3(0, _capsule.height/ 2, 0);
        _standingHeight = _capsule.height;

        if (_currentMonster == MonsterSelector.Prul && _waterSimulation != null)
        {
            _waterSimulation.IgnoreCollider(_capsule);
        }
    }

    private void FixedUpdate()
    {
        if (_currentMonster == MonsterSelector.Prul)
        {
            if (!_isInWaterVolume && _isUsingUnderwaterCamera)
            {
                _waterExitTimer -= Time.fixedDeltaTime;

                if (_waterExitTimer <= 0f)
                {
                    _isUsingUnderwaterCamera = false;

                    _underwaterCam.Priority = 0;
                    _normalCam.Priority = 20;
                }
            }
            if (_isInWaterVolume)
            {
                _isSwimming = true;
            }
            else
            {
                _waterCheckCounter++;

                if (_waterCheckCounter >= _waterCheckEveryFrames)
                {
                    _waterCheckCounter = 0;
                    _isSwimming = IsTouchingWaterParticles();

                }
            }
            
        
            if (!_isSwimming)
            {
                _rb.useGravity = true;
                _rb.linearDamping = 0f;
            }
        }
        switch(_currentMonster)
        {
            case MonsterSelector.Marsh:
                if(_alignWithView && _moveInput != Vector2.zero)
                    UpdateRotation();
                Move();
                SmoothCrouch();
                break;
            case MonsterSelector.Minty:
                if(_alignWithView && _moveInput != Vector2.zero)
                    UpdateRotation();
                Move();
                SmoothCrouch();
                break;
            case MonsterSelector.Rainbow:
                if(_alignWithView && _moveInput != Vector2.zero)
                    UpdateRotation();
                Move();
                SmoothCrouch();
                break;
            case MonsterSelector.Prul:
                if (_isSwimming)
                {
                    UpdateRotation();
                    Swimming();
                }
                else
                {
                    if (_moveInput != Vector2.zero)
                        UpdateRotation();
                    Move();
                    SmoothCrouch();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        UpdateAnimations();
        UpdateSounds();
    }
    
    
    private void UpdateRotation()
    {
        transform.rotation =
            Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, OrbitalFollow.HorizontalAxis.Value, 0), _rotationSpeed * Time.deltaTime);
    }

    private void Move()
    {
        _rb.linearDamping = 0f;

        float speed = _isCrouching ? _crouchSpeed : _moveSpeed;

        Vector3 inputDirection =
            transform.forward * _moveInput.y +
            transform.right * _moveInput.x;


        Vector3 targetVelocity = inputDirection * speed;
        Vector3 currentVelocity = _rb.linearVelocity;

        Vector3 velocityChange = new Vector3(
            targetVelocity.x - currentVelocity.x,
            0,
            targetVelocity.z - currentVelocity.z
        );


        if (IsGroundTooSteep())
        {
            Vector3 pointDownVector = Vector3.ProjectOnPlane(_hitInfo.normal, transform.up).normalized;
            velocityChange = VectorMath.RemoveDotVector(velocityChange, pointDownVector);
        }

        if (!IsGrounded()) velocityChange *= _airSpeedMultiplier;
        
        _rb.AddForce(velocityChange, ForceMode.VelocityChange);
        
        if(IsFalling())
            _rb.AddForce(Vector3.down * _fallSpeed, ForceMode.Force);
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

    private void Swimming()
    {
        Debug.Log("swimming");
        _rb.linearDamping = _swimDrag;
        _rb.useGravity = false;

        Vector3 direction = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        Vector3 targetHorizontalVelocity = direction * _swimSpeed;
        Vector3 currentVelocity = _rb.linearVelocity;

        Vector3 velocityChange = new Vector3(targetHorizontalVelocity.x - currentVelocity.x, 0, targetHorizontalVelocity.z - currentVelocity.z);

        _rb.AddForce(velocityChange, ForceMode.VelocityChange);

        if (_jumpHeld)
        {
            Debug.Log("Jump held");
            _rb.AddForce(Vector3.up * _swimUpAcceleration, ForceMode.Acceleration);
        }
        else if (_crouchHeld)
        {
            _rb.AddForce(Vector3.down * _swimDownAcceleration, ForceMode.Acceleration);
        }
        else
        {
            _rb.AddForce(Vector3.down * (_swimDownAcceleration * 0.5f), ForceMode.Acceleration);
        }

        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, Mathf.Clamp(_rb.linearVelocity.y, _maxSwimDownSpeed, _maxSwimUpSpeed), _rb.linearVelocity.z);
    }

    private bool IsTouchingWaterParticles()
    {
        if (_waterSimulation == null || _waterSimulation.positionBuffer == null)
            return false;

        if (_waterParticlePositions == null ||
            _waterParticlePositions.Length != _waterSimulation.positionBuffer.count)
        {
            _waterParticlePositions = new Vector3[_waterSimulation.positionBuffer.count];
        }

        _waterSimulation.positionBuffer.GetData(_waterParticlePositions);

        float radius = _capsule.radius + _waterTouchExtraRadius;

        Vector3 bottom = transform.position + Vector3.up * radius;
        Vector3 top = transform.position + Vector3.up * (_capsule.height - radius);

        for (int i = 0; i < _waterParticlePositions.Length; i++)
        {
            Vector3 particle = _waterParticlePositions[i];

            Vector3 closestPoint = ClosestPointOnLineSegment(bottom, top, particle);
            float distance = Vector3.Distance(particle, closestPoint);

            if (distance < radius)
                return true;
        }

        return false;
    }

    private Vector3 ClosestPointOnLineSegment(Vector3 a, Vector3 b, Vector3 point)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }
    
    bool IsGroundTooSteep() => !IsGrounded() || Vector3.Angle(_hitInfo.normal, transform.up) > _slopeLimit;
    
    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * (_groundSphereRadius + 0.1f);
        Ray ray = new Ray(origin, Vector3.down);
        return Physics.SphereCast(ray, _groundSphereRadius, out _hitInfo, _groundCheckDistance + (_groundSphereRadius + 0.1f));
    }

    private bool IsFalling() => !IsGrounded() && _rb.linearVelocity.y <= 0;

    // Input System Callbacks
    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!_isSwimming && value.isPressed && IsGrounded() && !IsGroundTooSteep())
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
            RuntimeManager.PlayOneShot(jumping, transform.position);        
        }
    }

    public void OnCrouch(InputValue value)
    {
        if (!_isSwimming && value.isPressed)
        {
            _isCrouching = !_isCrouching;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            _isInWaterVolume = true;
            _waterExitTimer = 0f;
            _isUsingUnderwaterCamera = true;

            _normalCam.Priority = 0;
            _underwaterCam.Priority = 20;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            _isInWaterVolume = false;
            _waterExitTimer = _waterExitDelay;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Ray ray = new Ray(origin, Vector3.down);
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin + Vector3.down * _groundCheckDistance, _groundSphereRadius);
    }

    private void UpdateAnimations()
    {
        animator.SetBool("IsMoving", _moveInput.magnitude > 0.1f);
        animator.SetBool("IsGrounded", IsGrounded());
        
        
        Vector3 origin = transform.position + Vector3.up * (_landingSphereRadius + _landingSphereRadius * 0.1f);
        Ray ray = new Ray(origin, Vector3.down); 
        if(Physics.SphereCast(ray, _landingSphereRadius, out _hitInfo, _landingCheckDistance) && _rb.linearVelocity.y <= 0.2f)
        {
            animator.SetTrigger("Land");
        }
        else
        {
            animator.ResetTrigger("Land");
        }
    }

    private void UpdateSounds()
    {
        bool moving = _moveInput.magnitude > 0.1f;
        bool grounded = IsGrounded();

        //walking.SetActive(moving && grounded);
    }
}