using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PermutationLink : MonoBehaviour
{

    [SerializeField] private SpriteRenderer source;
    [SerializeField] private int sourceId;
    [SerializeField] private SpriteRenderer target;
    public int targetId { get; private set; }
    private LineRenderer lineRenderer;

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

    public void SetIDs(int sourceId, int targetId)
    {
        this.sourceId = sourceId;
        this.targetId = targetId;
        source.transform.localPosition = new Vector3(0, sourceId, 0);
        target.transform.localPosition = new Vector3(2, targetId, 0);
        lineRenderer.SetPosition(0, source.transform.localPosition);
        lineRenderer.SetPosition(1, target.transform.localPosition);
    }

    public void SetColor(Color color)
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
