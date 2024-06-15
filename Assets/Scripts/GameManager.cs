using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using EdgeType = DirectedEdge.EdgeType;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        Instance = this;
    }


    public Transform ObjectAtMouse()
    {
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));

        // TODO: read about GetRayIntersectionNonAlloc

        if (!rayHit.collider)
            return null;
        return rayHit.transform;
    }


}
