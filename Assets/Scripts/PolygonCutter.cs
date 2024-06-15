using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor.U2D.Path;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using static Polygon;

[RequireComponent(typeof(LineRenderer))]
public class PolygonCutter : MonoBehaviour
{

    [SerializeField] private Camera mainCamera;
    [SerializeField] private CuttableSurface surface;

    private bool updateLine = false;
    private LineRenderer lineRenderer;

    PlayerInput input;

    private void Awake()
    {
        input = new PlayerInput();
        input.Player.Enable();

        lineRenderer = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        input.Player.MouseSelect.started += MouseSelect_started;
        input.Player.MouseSelect.performed += MouseSelect_performed;
        input.Player.MouseSelect.canceled += MouseSelect_canceled;
    }

    private void OnDisable()
    {
        input.Player.MouseSelect.started -= MouseSelect_started;
        input.Player.MouseSelect.performed -= MouseSelect_performed;
        input.Player.MouseSelect.canceled -= MouseSelect_canceled;
    }


    private Polygon selectedPolygon = null;
    private int selectedVertexId = -1;

    private void MouseSelect_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (selectedPolygon == null)
        {
            Vector3 mousePosition = MousePosition();            

            // TODO: check differences between tuple, ValueTuple and Anonymous class
            var minVertexInfo = surface.GetPolygons()
                .Select(poly => new { Poly = poly, ClosestVertexInfo = poly.GetClosestVertex(mousePosition) })
                .Aggregate((min, current) => current.ClosestVertexInfo.distance < min.ClosestVertexInfo.distance ? current : min);

            Vector3 vertex = minVertexInfo.ClosestVertexInfo.vertex;
            selectedPolygon = minVertexInfo.Poly;
            selectedVertexId = minVertexInfo.ClosestVertexInfo.vertexId;
            vertex.z = -1;
            lineRenderer.enabled = true;
            updateLine = true;
            lineRenderer.SetPosition(0, vertex);

            /*
            float distance = 10000;
            Vector3 closestVertex = default(Vector3);
            foreach (Polygon polygon in surface.GetPolygons())
            {
                Vector3 vertex = polygon.GetClosestVertex(mousePosition);
                float d = Vector2.Distance(closestVertex, mousePosition);
                if (d < distance)
                {
                    distance = d;
                    selectedPolygon = polygon;
                    closestVertex = vertex;
                }
            }

            closestVertex.z = -1;
            lineRenderer.enabled = true;
            updateLine = true;
            lineRenderer.SetPosition(0, closestVertex);*/
        }
        
    }


    private void MouseSelect_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
    }

    private void MouseSelect_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (selectedPolygon != null)
        {
            ClosestVertexInfo closestVertexInfo = selectedPolygon.GetClosestVertex(MousePosition());
            Vector3 closestVertex = closestVertexInfo.vertex;
            closestVertex.z = -1;
            lineRenderer.SetPosition(1, closestVertex);
            updateLine = false;

            if (surface.TryCutAt(selectedPolygon, selectedVertexId, closestVertexInfo.vertexId))
            {
                Debug.Log($"succesfully cut polygon {selectedPolygon}");
                lineRenderer.enabled = false;
            }

            selectedPolygon = null;
        }
    }


    private Vector2 MousePosition()
    {
        return mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }


    // Update is called once per frame
    void Update()
    {
        if (updateLine)
        {
            Vector3 position = MousePosition();
            position.z = -1;
            lineRenderer.SetPosition(1, position);
        }
    }
}
