using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    PlayerInput input;
    [SerializeField] private CharacterController2D characterController;
    [SerializeField] private Animator animator;

    [SerializeField] private float horizontalSpeed = 1f;

    private void Awake()
    {
        input = new PlayerInput();
        input.Player.Enable();
    }

    private void OnEnable()
    {
        input.Player.Movement.performed += Movement_performed;
        input.Player.Jump.performed += Jump_performed;
    }

    private void OnDisable()
    {
        input.Player.Movement.performed -= Movement_performed;
        input.Player.Jump.performed -= Jump_performed;
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        characterController.Move(0, false, true);
    }

    private void Movement_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 movement = input.Player.Movement.ReadValue<Vector2>();
        animator.SetFloat("Speed", Mathf.Abs(movement.x * horizontalSpeed));
        characterController.Move(Time.fixedDeltaTime * movement.x * horizontalSpeed, false, false);
    }
}
