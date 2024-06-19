using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastSelector2D : MonoBehaviour
{
    public static RaycastSelector2D Instance { get; private set; }

    [SerializeField] private bool mouseRaycastActive = true;
    [SerializeField] private bool mouseLogs = false;


    #region --------------- register mouse events ---------------

    private PlayerInput input;


    void Awake()
    {
        Instance = this;

        input = new PlayerInput();
        input.Player.Enable();
    }

    private void OnEnable()
    {
        input.Player.MouseSelect.performed += MousePressed;
    }

    private void OnDisable()
    {
        input.Player.MouseSelect.performed -= MousePressed;
    }

    #endregion

    #region --------------- mouse raycast event ---------------

    public void SetMouseRaycastActive(bool active)
    {
        mouseRaycastActive = active;
    }

    /// <summary>
    /// These events are invoked whenever an object with collider is pressed with the mouse,
    /// as long as the mouseRaycastActive is on.
    /// </summary>
    public event EventHandler<Transform> OnObjectPressed;

    /// <summary>
    /// As with OnObjectPressed, but also sends out the mouse position.
    /// </summary>
    public event EventHandler<(Transform, Vector2)> OnObjectPressedPlus;

    /// <summary>
    /// Return the position of the mouse on screen, used for raycasting.
    /// </summary>
    /// <returns></returns>
    public Vector2 MousePosition()
    {
        return Mouse.current.position.ReadValue();
    }

    /// <summary>
    /// Return the position of the mouse in the world coordinates
    /// </summary>
    /// <returns></returns>
    public Vector2 MousePositionWorld()
    {
        return Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    private void MousePressed(InputAction.CallbackContext obj)
    {
        if (!mouseRaycastActive)
            return;

        Vector2 position = MousePosition();
        Transform objPressed = ObjectAtPosition(position);
        if (objPressed != null) 
        { 
            if (mouseLogs)
                Debug.Log($"{objPressed.gameObject.name} pressed at position {position}");
            OnObjectPressed?.Invoke(this, objPressed);
            OnObjectPressedPlus?.Invoke(this, (objPressed, position));
        }
    }

    #endregion

    #region --------------- Object from position ---------------

    public static Transform ObjectAtPosition(Vector2 screenPoint)
    {
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(screenPoint));        

        // TODO: read about GetRayIntersectionNonAlloc

        if (!rayHit.collider)
            return null;
        return rayHit.transform;
    }

    public Transform ObjectAtMousePosition()
    {
        return ObjectAtPosition(MousePosition());
    }

    #endregion

}
