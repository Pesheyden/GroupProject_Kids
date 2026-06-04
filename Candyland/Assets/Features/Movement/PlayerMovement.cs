using System;
using System.Collections;
using BSOAP.Variables;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private FloatVariable movementSpeed;
    [SerializeField] private FloatVariable jumpHeight;
    [SerializeField] private FloatVariable gravity;
    [SerializeField] private LayerMask obstacleMask;

    private CharacterController controller;
    
    private Vector2 moveInput;
    private Vector3 _velocity;

    private bool isCrouching;
    private bool wantToStand;

    private float originalSpeed;
    private float originalHeight;
    private float standingHeght = 2f;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        originalSpeed = movementSpeed.Value;
        originalHeight = controller.height;
    }


    void Update()
    {
        float targetHeight = isCrouching ? 1f : originalHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, 4f * Time.deltaTime);

        if(wantToStand && CanStand())
        {
            isCrouching = false;
            wantToStand = false;
        }
    }

    public void LateUpdate()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        controller.Move(movement * movementSpeed.Value * Time.deltaTime);

        if(controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
            movementSpeed.Value = originalSpeed;
        }

        _velocity.y += gravity.Value * Time.deltaTime;
        controller.Move(_velocity * Time.deltaTime);
    }

    public void AddVelocity(Vector3 velocity)
    {
        _velocity += velocity;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if(value.isPressed && controller.isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight.Value * -2f * gravity.Value);
            movementSpeed.Value -= 2f;
        }
    }

    public void OnCrouch(InputValue value)
    {
        if(value.isPressed && controller.isGrounded && isCrouching == false)
        {
            movementSpeed.Value -= 2f;

            isCrouching = true;
            wantToStand = false;
        }
        else if(value.isPressed && controller.isGrounded && isCrouching == true )
        {
            movementSpeed.Value = originalSpeed;

            wantToStand = true;
        }
    }

    private bool CanStand()
    {
        float heightDiference = standingHeght - controller.height;

        return !Physics.CheckCapsule(transform.position, Vector3.up, heightDiference, obstacleMask);
    }
}
