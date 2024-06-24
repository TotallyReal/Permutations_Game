using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class DragablePermutation : MonoBehaviour
{
    
    [SerializeField] private DragAndDrop draggablePrefab;
    [SerializeField] private float heightDiff = 1f;
    [SerializeField] private float latchingDistance = 0.5f;
    [Range(2,12)]
    [SerializeField] private int size = 5;

    private Permutation permutation;
    private DragAndDrop[] elements;
    private EventHandler<Vector2>[] startDraggingHandlers;
    private EventHandler<Vector2>[] draggingHandlers;
    private EventHandler<Vector2>[] endDraggingHandlers;

    private void Awake()
    {
        Assert.IsTrue(size >= 2);
        permutation = new Permutation(size);
        elements = new DragAndDrop[size];
        startDraggingHandlers = new EventHandler<Vector2>[size];
        draggingHandlers = new EventHandler<Vector2>[size];
        endDraggingHandlers = new EventHandler<Vector2>[size];

        for (int i = 0; i < size; i++)
        {
            elements[i] = Instantiate(draggablePrefab, transform);
            elements[i].transform.localPosition = new Vector3(0, heightDiff * i ,0);
            elements[i].name = $"{i}-th element";
            elements[i].GetComponent<SpriteRenderer>().color = PipesRoom.colors[i]; // TODO: I don't like this assumption

            int currentIndex = i;
            startDraggingHandlers[i] = (object sender, Vector2 position) =>
            {
                OnStartDragging?.Invoke(this, new DraggingArgs() { position = position, index = currentIndex });
            };
            draggingHandlers[i] = (object sender, Vector2 position) =>
            {
                OnPositionChanged?.Invoke(this, new DraggingArgs() { position = elements[currentIndex].transform.position, index = currentIndex });
            };
            endDraggingHandlers[i] = (object sender, Vector2 position) => OnSimpleEndDragging(sender, position, currentIndex);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < size; i++)
        {
            elements[i].OnStartDragging += startDraggingHandlers[i];
            elements[i].OnDragging += draggingHandlers[i];
            elements[i].OnEndDragging += endDraggingHandlers[i];
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < size; i++)
        {
            elements[i].OnStartDragging -= startDraggingHandlers[i];
            elements[i].OnDragging -= draggingHandlers[i];
            elements[i].OnEndDragging -= endDraggingHandlers[i];
        }
    }

    public class DraggingArgs
    {
        public Vector2 position;
        public int index;
    }
    public class EndDraggingArgs
    {
        public Vector2 position;
        public int source;
        public int target;
        public bool changed;
    }

    public event EventHandler<DraggingArgs> OnPositionChanged;

    public event EventHandler<DraggingArgs> OnStartDragging;
    public event EventHandler<DraggingArgs> OnDragging;
    // When permutation is changed from dragging, it is done in the EndDragging phase.
    public event EventHandler<EndDraggingArgs> OnEndDragging;

    private void OnSimpleEndDragging(object sender, Vector2 position, int index)
    {
        // TODO: move draggable element to the front of the scene, so it will always be
        //       before the other elements.
        int n = permutation.size;
        int targetId = permutation[index];

        Vector2 localPosition = transform.InverseTransformPoint(position);

        int secondTargetId = Mathf.RoundToInt(localPosition.y / heightDiff);
        Vector2 targetCenter = new Vector2(0, secondTargetId * heightDiff);

        if (0 <= secondTargetId && secondTargetId < n && secondTargetId != targetId &&
            (localPosition - targetCenter).magnitude < latchingDistance )
        {
            elements[index].transform.localPosition = new Vector3(0, heightDiff * secondTargetId, 0);
            int secondIndex = permutation.Inverse(secondTargetId);
            elements[secondIndex].transform.localPosition = new Vector3(0, heightDiff * targetId, 0);

            Permutation transposition = Permutation.Cycle(n, targetId, secondTargetId);
            permutation = transposition * permutation;

            OnEndDragging?.Invoke(this, new EndDraggingArgs() { 
                position=position, 
                source = index, 
                target= secondTargetId, 
                changed=(targetId!= secondTargetId) 
            });
        }
        else
        {
            // return to previous position
            elements[index].transform.localPosition = new Vector3(0, heightDiff * targetId, 0);

            OnEndDragging?.Invoke(this, new EndDraggingArgs()
            {
                position = position,
                source = index,
                target = targetId,
                changed = false
            });
        }

    }

    public Permutation GetPermutation()
    {
        return permutation; // TODO: should not be able to change the permutation from outside. Return a copy, or make it a struct?
    }

    internal float GetHeightDiff()
    {
        return heightDiff;
    }
}
