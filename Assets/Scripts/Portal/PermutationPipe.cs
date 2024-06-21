using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;
using static Unity.VisualScripting.Member;

public class PermutationPipe : MonoBehaviour
// TODO: consider joining the classes, instead of inherit.
//  using it like this to not destroy the older puzzle.
{
    [Header("Dimensions")]
    public float heightDiff = 1;
    [SerializeField] private float leftWidth;
    public float linkWidth = 2;
    [SerializeField] private float rightWidth;

    [Header("Link")]
    [SerializeField] private SpriteRenderer source;
    [SerializeField] private int sourceId;
    [SerializeField] private SpriteRenderer target;
    public int targetId { get; protected set; }
    [SerializeField] private LineRenderer linkLine;

    [Header("Pipe extensions")]
    [SerializeField] private LineRenderer leftLine;
    [SerializeField] private LineRenderer rightLine;

    private void Awake()
    {
        leftLine.positionCount = 2; 
        linkLine.positionCount = 2;
        rightLine.positionCount = 2;
    }

    private void Start()
    {
        target.GetComponent<DragAndDrop>().OnDragging += OnDragging;
    }

    private void OnDragging(object sender, Vector2 e)
    {
        linkLine.SetPosition(1, target.transform.localPosition);
    }

    public DragAndDrop GetTarget()
    {
        return target.GetComponent<DragAndDrop>();
    }

    public Color GetColor()
    {
        return leftLine.startColor;
    }

    public void SetColor(Color color)
    {
        source.color = color;
        target.color = color;

        leftLine.startColor = color;
        leftLine.endColor = color;

        rightLine.startColor = color;
        rightLine.endColor = color;

        linkLine.startColor = color;
        linkLine.endColor = color;
    }

    public void SetIDs(int sourceId, int targetId)
    {
        this.sourceId = sourceId;
        this.targetId = targetId;

        source.transform.localPosition = new Vector3(leftWidth, sourceId * heightDiff, 0);
        target.transform.localPosition = new Vector3(leftWidth + linkWidth, targetId * heightDiff, 0);

        leftLine.SetPosition(0, new Vector3(0, sourceId * heightDiff, 0));
        leftLine.SetPosition(1, source.transform.localPosition);

        linkLine.SetPosition(0, source.transform.localPosition);
        linkLine.SetPosition(1, target.transform.localPosition);

        rightLine.SetPosition(0, target.transform.localPosition);
        rightLine.SetPosition(1, new Vector3(leftWidth + linkWidth + rightWidth, targetId * heightDiff, 0));
    }

    internal void SetDimensions(float leftWidth, float linkWidth, float rightWidth, float heightDiff)
    {
        this.leftWidth = leftWidth;
        this.rightWidth = rightWidth;
        this.linkWidth = linkWidth;
        this.heightDiff = heightDiff;
        SetIDs(this.sourceId, targetId); // TODO: don't need to use IDs here. Only for now.
    }
}
