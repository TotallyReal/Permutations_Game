using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{

    private Camera _camera;
    private PlayerInput input;

    private void Awake()
    {
        input = new PlayerInput();
        input.Camera.Enable();
        _camera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        input.Camera.Zoom.performed += Zoom_performed;
    }

    private void OnDisable()
    {
        input.Camera.Zoom.performed -= Zoom_performed;
    }

    private void Zoom_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        float zoomValue = obj.ReadValue<float>() / 120f;
        _camera.orthographicSize *= Mathf.Exp(-zoomValue/5);
    }
}
