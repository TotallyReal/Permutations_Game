using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Should have a collider
public class DragAndDrop : MonoBehaviour
{

    private Vector3 lastPosition;
    private bool isDragging = false;
    private bool dragIsActive { get; set; } = true;
    PlayerInput input;

    private void Awake()
    {
        input = new PlayerInput();
        input.Player.Enable();
    }

    private void OnEnable()
    {
        RaycastSelector2D.Instance.OnObjectPressed += Instance_OnObjectPressed;
        input.Player.MouseSelect.canceled += MouseSelect_canceled;
    }

    private void OnDisable()
    {
        RaycastSelector2D.Instance.OnObjectPressed -= Instance_OnObjectPressed;
        input.Player.MouseSelect.canceled += MouseSelect_canceled;
    }


    public event EventHandler<Vector2> OnStartDragging;
    public event EventHandler<Vector2> OnDragging;
    public event EventHandler<Vector2> OnEndDragging;

    private void Instance_OnObjectPressed(object sender, Transform obj)
    {
        if (dragIsActive && obj == transform)
        {
            lastPosition = RaycastSelector2D.Instance.MousePositionWorld();
            isDragging = true;
            OnStartDragging?.Invoke(this, lastPosition);
        }
    }

    private void MouseSelect_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isDragging)
        {
            OnEndDragging?.Invoke(this, RaycastSelector2D.Instance.MousePositionWorld());
        }
        isDragging = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragging)
        {
            Vector3 currentPosition = RaycastSelector2D.Instance.MousePositionWorld();
            transform.position += (currentPosition - lastPosition);
            lastPosition = currentPosition;
            OnDragging?.Invoke(this, lastPosition);
        }
    }
}
