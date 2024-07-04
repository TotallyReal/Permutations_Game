using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(DragAndDrop))]
public class PuzzlePiece : MonoBehaviour
{
    [Range(1, 10)]
    [SerializeField] private int width = 1;
    [Range(1, 10)]
    [SerializeField] private int height = 1;

    private DragAndDrop dragAndDrop;
    private Vector3 beforeDragging;

    private void Awake()
    {
        dragAndDrop = GetComponent<DragAndDrop>();
        beforeDragging = transform.position;
    }

    private void OnEnable()
    {
        dragAndDrop.OnStartDragging += OnStartDragging;
        dragAndDrop.OnEndDragging += OnEndDragging;
    }

    private void OnDisable()
    {
        dragAndDrop.OnStartDragging -= OnStartDragging;
        dragAndDrop.OnEndDragging -= OnEndDragging;
    }

    public event EventHandler<Vector2> OnPieceLifted;

    

    private void OnStartDragging(object sender, Vector2 e)
    {
        beforeDragging = transform.position;
        OnPieceLifted?.Invoke(this, transform.position);
    }

    public event EventHandler<Vector2> OnPieceDropped;

    private void OnEndDragging(object sender, Vector2 v)
    {
        OnPieceDropped?.Invoke(this, transform.position);
    }

    public (int, int) GetDimension()
    {
        return (width, height);
    }

    // In global coordinates
    internal void SetBottomLeft(Vector3 position)
    {
        transform.position = position;
    }

    public void ToBeforeDragging()
    {
        transform.position = beforeDragging;
    }
}
