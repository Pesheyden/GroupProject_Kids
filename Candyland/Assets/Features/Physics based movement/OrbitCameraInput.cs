using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class OrbitCameraInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineOrbitalFollow orbital;

    [SerializeField] private float _verticalSensitivity;
    [SerializeField] private float _horizontalSensitivity;
    private Vector2 lookInput;

    private void Awake()
    {
        if (orbital == null)
            orbital = GetComponentInChildren<CinemachineOrbitalFollow>();
    }

    private void Update()
    {
        if (orbital == null)
            return;

        orbital.HorizontalAxis.Value += lookInput.x * _horizontalSensitivity / 100;
        orbital.VerticalAxis.Value += lookInput.y * _verticalSensitivity / 100;

        orbital.VerticalAxis.Value = orbital.VerticalAxis.GetClampedValue();
    }

    // Input System callback
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
}