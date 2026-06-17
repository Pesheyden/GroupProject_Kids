using UnityEngine;
using UnityEngine.InputSystem;

public class GodCamera : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 0.1f;
    [SerializeField] private float _controllerRotationSpeed = 120f;
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _rotationSmoothness = 10f;

    private float xRot, yRot;
    private Quaternion targetRotation;

    private void OnEnable()
    {
        Vector3 currentRot = transform.rotation.eulerAngles;
        xRot = currentRot.x;
        yRot = currentRot.y;
        targetRotation = transform.rotation;
    }

    public void FreeCamera(Gamepad gamepad = null)
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        Vector3 moveDirection = Vector3.zero;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) moveDirection += transform.forward;
            if (keyboard.sKey.isPressed) moveDirection -= transform.forward;
            if (keyboard.dKey.isPressed) moveDirection += transform.right;
            if (keyboard.aKey.isPressed) moveDirection -= transform.right;
            if (keyboard.eKey.isPressed) moveDirection += transform.up;
            if (keyboard.qKey.isPressed) moveDirection -= transform.up;
        }

        if (mouse != null && mouse.rightButton.isPressed)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();

            xRot -= mouseDelta.y * _rotationSpeed;
            yRot += mouseDelta.x * _rotationSpeed;
        }

        if (gamepad != null)
        {
            Vector2 leftStick = gamepad.leftStick.ReadValue();
            Vector2 rightStick = gamepad.rightStick.ReadValue();

            moveDirection += transform.forward * leftStick.y;
            moveDirection += transform.right * leftStick.x;

            float upDown = gamepad.rightTrigger.ReadValue() - gamepad.leftTrigger.ReadValue();
            moveDirection += transform.up * upDown;

            yRot += rightStick.x * _controllerRotationSpeed * Time.deltaTime;
            xRot -= rightStick.y * _controllerRotationSpeed * Time.deltaTime;
        }

        xRot = Mathf.Clamp(xRot, -80f, 80f);

        targetRotation = Quaternion.Euler(xRot, yRot, 0);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * _rotationSmoothness
        );

        float multiplier = 1f;

        if (keyboard != null &&
            (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed))
        {
            multiplier = 3f;
        }

        if (gamepad != null && gamepad.leftStickButton.isPressed)
        {
            multiplier = 3f;
        }

        if (moveDirection != Vector3.zero)
        {
            transform.position += moveDirection.normalized * _moveSpeed * multiplier * Time.deltaTime;
        }
    }
}