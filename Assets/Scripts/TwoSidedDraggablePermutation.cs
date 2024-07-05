using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

[RequireComponent(typeof(PermutationLines))]
public class TwoSidedDraggablePermutation : MonoBehaviour
{
    // x position, height, getPermutation, positionChanged, permutation changed
    [SerializeField] private DragablePermutation left; 
    [SerializeField] private DragablePermutation right;
    private PermutationLines lines;

    private Permutation id;

    private void Awake()
    {
        lines = GetComponent<PermutationLines>();
        id = new Permutation(lines.GetSize());
    }

    private void Start()
    {
        UpdatePermutation();
    }

    private void UpdatePermutation()
    {
        Permutation leftP = (left != null) ? left.GetPermutation() : id;
        Permutation rightP = (right != null) ? right.GetPermutation() : id;
        lines.UpdatePermutation(rightP * leftP.Inverse());
    }

    public void SetConnectors(DragablePermutation left, DragablePermutation right)
    {
        this.enabled = false;
        // TODO: is OnDisabled called before the next line?
        this.left = left;
        this.right = right;
        this.enabled = true;
    }

    private void OnEnable()
    {
        if (right != null)
        {
            right.OnPositionChanged += Right_OnDragging;
            right.OnEndDragging += OnEndDragging;
        }
        if (left != null)
        {
            left.OnPositionChanged += Left_OnDragging;
            left.OnEndDragging += OnEndDragging;
        }
    }

    private void OnDisable()
    {
        if (right != null)
        {
            right.OnPositionChanged -= Right_OnDragging;
            right.OnEndDragging -= OnEndDragging;
        }
        if (left != null)
        {
            left.OnPositionChanged -= Left_OnDragging;
            left.OnEndDragging -= OnEndDragging;
        }
    }

    private void Left_OnDragging(object sender, DragablePermutation.DraggingArgs args)
    {
        lines.SetEndPoint(left: true, index: args.positionIndex, position: args.position);
        //lines[args.index].SetPosition(0, transform.InverseTransformPoint(args.position));
    }

    private void Right_OnDragging(object sender, DragablePermutation.DraggingArgs args)
    {
        lines.SetEndPoint(left: false, index: args.positionIndex, position: args.position);
        //lines[args.index].SetPosition(1, transform.InverseTransformPoint(args.position));
    }

    private void OnEndDragging(object sender, DragablePermutation.EndDraggingArgs args)
    {
        UpdatePermutation();
    }

}
