using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using NaughtyAttributes;
using UnityEngine.Rendering;

public class OrbitCameraInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineOrbitalFollow _orbital;
    [SerializeField] private GodCamera _freeCamera;
    [SerializeField] private MasterControllerTool _masterControllerTool;


    [Header("Settings")]
    [SerializeField] private float _verticalSensitivity;
    [SerializeField] private float _horizontalSensitivity;
    [SerializeField] private float _afkTime = 5f;
    [SerializeField] private float _autoAlignmentTime = 2f;


    private Vector2 _lookInput;
    private Coroutine _autoAlignmentCoroutine;

    private void Awake()
    {
        if (_orbital == null)
            _orbital = GetComponentInChildren<CinemachineOrbitalFollow>();
    }

    private void Update()
    {
        if (_masterControllerTool && _masterControllerTool._hasGodMode)
        {
            _freeCamera.FreeCamera(_masterControllerTool.MasterGamepad);
            return;
        }

        if (_orbital == null)
            return;

        if (_lookInput == Vector2.zero)
        {
            _autoAlignmentCoroutine ??= StartCoroutine(TimerCoroutine(AlignCamera));
            return;
        }
        if (_autoAlignmentCoroutine != null)
        {
            StopCoroutine(_autoAlignmentCoroutine);
            _autoAlignmentCoroutine = null;
        }
        
        _orbital.HorizontalAxis.Value += _lookInput.x * _horizontalSensitivity / 100;
        _orbital.VerticalAxis.Value += _lookInput.y * _verticalSensitivity / 100;

        _orbital.HorizontalAxis.Value = _orbital.HorizontalAxis.GetClampedValue();
        _orbital.VerticalAxis.Value = _orbital.VerticalAxis.GetClampedValue();
    }

    private void AlignCamera()
    {
        Vector3 fwd = _orbital.LookAtTarget.forward;

        float horizontalRotation = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg;
        float verticalRotation = -Mathf.Asin(fwd.y) * Mathf.Rad2Deg;

        _autoAlignmentCoroutine =
            StartCoroutine(SmoothCameraRotation(horizontalRotation, verticalRotation, _autoAlignmentTime));
    }

    private IEnumerator SmoothCameraRotation(float horizontalTarget, float verticalTarget, float time)
    {
        float timer = time;
        float horizontal = _orbital.HorizontalAxis.Value;
        float vertical = _orbital.VerticalAxis.Value;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            float t = timer / time;

            _orbital.HorizontalAxis.Value = Mathf.Lerp(horizontalTarget, horizontal , Mathf.SmoothStep(0,1,t));
            _orbital.VerticalAxis.Value = Mathf.Lerp(verticalTarget, vertical, Mathf.SmoothStep(0,1,t));
            
            yield return null;
        }

        _orbital.HorizontalAxis.Value = horizontalTarget;
        _orbital.VerticalAxis.Value = verticalTarget;
    }

    private IEnumerator TimerCoroutine(Action action)
    {
        float timer = _afkTime;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        action.Invoke();
    }
    
    // Input System callback
    public void OnLook(InputValue value)
    {
        _lookInput = value.Get<Vector2>();
    }
}