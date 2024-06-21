using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PermutationLink : MonoBehaviour
{

    [SerializeField] private SpriteRenderer source;
    [SerializeField] protected int sourceId; // TODO: private?
    [SerializeField] private SpriteRenderer target;
    public int targetId { get; protected set; }
    private LineRenderer lineRenderer;

    public float linkWidth = 2;
    public float heightDiff = 1;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    private void Start()
    {
        target.GetComponent<DragAndDrop>().OnDragging += PermutationLink_OnDragging;
    }

    private void PermutationLink_OnDragging(object sender, Vector2 e)
    {
        lineRenderer.SetPosition(1, target.transform.localPosition);
    }

    virtual public void SetIDs(int sourceId, int targetId)
    {
        this.sourceId = sourceId;
        this.targetId = targetId;
        source.transform.localPosition = new Vector3(0, sourceId * heightDiff, 0);
        target.transform.localPosition = new Vector3(linkWidth, targetId * heightDiff, 0);
        lineRenderer.SetPosition(0, source.transform.localPosition);
        lineRenderer.SetPosition(1, target.transform.localPosition);
    }

    virtual public void SetColor(Color color)
    {
        source.color = color;
        target.color = color;   
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    public DragAndDrop GetTarget()
    {
        return target.GetComponent<DragAndDrop>();
    }

}
