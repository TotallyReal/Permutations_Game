using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoSidedDraggablePermutation : MonoBehaviour
{

    [SerializeField] private DragablePermutation left;
    [SerializeField] private DragablePermutation right;
    [SerializeField] private Transform linesParent;
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private int size;

    private Permutation permutation;
    private LineRenderer[] lines;

    private float leftX, leftHeight, rightX, rightHeight;

    void Awake()
    {
        lines = new LineRenderer[size];

        leftX = left.transform.localPosition.x;
        leftHeight = left.GetHeightDiff();
        rightX = right.transform.localPosition.x;
        rightHeight = right.GetHeightDiff();

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = Instantiate(linePrefab, linesParent);
            lines[i].name = $"line {i}";
            lines[i].transform.localPosition = Vector3.zero;
            lines[i].positionCount = 2;

        }

    }

    private void Start()
    {
        UpdatePermutation();
    }

    private void UpdatePermutation()
    {
        permutation = right.GetPermutation() * left.GetPermutation().Inverse();
        Permutation leftP = left.GetPermutation();
        Permutation rightP = right.GetPermutation();

        for (int i = 0; i < lines.Length; i++)
        {
            // The i-th line is of the form left[i] -> right[i]
            lines[i].SetPosition(0, new Vector3(leftX, leftP[i] * leftHeight, 0));
            lines[i].SetPosition(1, new Vector3(rightX, rightP[i] * leftHeight, 0));
        }

    }

    private void OnEnable()
    {
        right.OnPositionChanged += Right_OnDragging;
        right.OnEndDragging += OnEndDragging;
        left.OnPositionChanged += Left_OnDragging;
        left.OnEndDragging += OnEndDragging;
    }

    private void OnDisable()
    {
        right.OnPositionChanged -= Right_OnDragging;
        right.OnEndDragging -= OnEndDragging;
        left.OnPositionChanged -= Left_OnDragging;
        left.OnEndDragging -= OnEndDragging;
    }

    private void Left_OnDragging(object sender, DragablePermutation.DraggingArgs args)
    {
        lines[args.index].SetPosition(0, transform.InverseTransformPoint(args.position));
    }

    private void Right_OnDragging(object sender, DragablePermutation.DraggingArgs args)
    {
        lines[args.index].SetPosition(1, transform.InverseTransformPoint(args.position));
    }

    private void OnEndDragging(object sender, DragablePermutation.EndDraggingArgs args)
    {
        UpdatePermutation();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
