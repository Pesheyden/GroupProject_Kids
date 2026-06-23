using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Rotatable : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private CinemachineCamera _camera;
    
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float smoothness = 0.15f;
    [SerializeField] private float pitchSensititvity = 10f;
    [SerializeField] private float yawlSensititvity = 40f;
    [SerializeField] private float minPitch = -45f;
    [SerializeField] private float maxPitch = 45f;

    private InputAction _lookAction;
    private Vector2 _input;
    private float _pitch;
    private float _yaw;
    private bool _active;

    private CinemachineCamera _playerCamera;
    
    private void Start()
    {
        Vector3 euler = transform.localEulerAngles;
        _yaw = euler.y;
        _pitch = euler.x;
    }
    
    public void Started(PlayerInput playerInput)
    {
        _active = true;
        _camera.enabled = true;

        _playerCamera = playerInput.GetComponent<PlayerMoveControllerRB>().OrbitalFollow.GetComponent<CinemachineCamera>();
        _playerCamera.Priority = -100;
        _lookAction = playerInput.actions["Look"];
        _lookAction.started += OnLookInput;
        _lookAction.canceled += OnLookInput;
    }
    
    public void Canceled(PlayerInput playerInput)
    {
        _active = false;
        _camera.enabled = false;
        _lookAction.started -= OnLookInput;
        _lookAction.canceled -= OnLookInput;
        _playerCamera.Priority = 0;
    }
    
    private void OnLookInput(InputAction.CallbackContext obj)
    {
        _input = obj.ReadValue<Vector2>();
    }
    
    public void LateUpdate()
    {
        if(!_active)
            return;
        
        Rotate();
    }
    
    private void Rotate()
    {
        _yaw += _input.x * rotationSpeed * Time.deltaTime * yawlSensititvity / 100;
        _pitch -= _input.y * rotationSpeed * Time.deltaTime * pitchSensititvity / 100;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        
        transform.rotation =  Quaternion.Euler(_pitch, _yaw, 0f);
    }
}
