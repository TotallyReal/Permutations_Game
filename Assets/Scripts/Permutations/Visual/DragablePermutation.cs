using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Represents draggable objects in a column.
/// These objects can be dragged to swap positions, and then invoke events.
/// </summary>
public class DragablePermutation : MonoBehaviour
{

    [Header("logic")]
    [Range(2,12)]
    [SerializeField] private int size = 5;
    [SerializeField] private string permutationString = "";
    [SerializeField] private bool isActive = true;
    [Header("Visual")]
    [SerializeField] private DragAndDrop draggablePrefab;
    [SerializeField] private float heightDiff = 1f;
    [SerializeField] private float latchingDistance = 0.5f;

    private Permutation permutation;
    private DragAndDrop[] elements;
    private EventHandler<Vector2>[] pressedHandlers;
    private EventHandler<Vector2>[] startDraggingHandlers;
    private EventHandler<Vector2>[] draggingHandlers;
    private EventHandler<Vector2>[] endDraggingHandlers;

    private Vector3[] positions;

    private void Awake()
    {
        Assert.IsTrue(size >= 2);

        
        CreatePositions();

        permutation = Permutation.FromString(size, permutationString);

        elements = new DragAndDrop[size];

        pressedHandlers = new EventHandler<Vector2>[size];
        startDraggingHandlers = new EventHandler<Vector2>[size];
        draggingHandlers = new EventHandler<Vector2>[size];
        endDraggingHandlers = new EventHandler<Vector2>[size];

        for (int i = 0; i < size; i++)
        {
            elements[i] = Instantiate(draggablePrefab, transform);
            elements[i].transform.localPosition = positions[permutation[i]];
            elements[i].name = $"{i}-th element";
            elements[i].GetComponent<SpriteRenderer>().color = PipesRoom.colors[i]; // TODO: I don't like this assumption

            int currentIndex = i;
            pressedHandlers[i] = (object sender, Vector2 position) =>
            {
                OnPressed?.Invoke(this, new DraggingArgs()
                {
                    position = elements[currentIndex].transform.position,
                    //index = currentIndex,
                    positionIndex = permutation[currentIndex]
                });
            };
            startDraggingHandlers[i] = (object sender, Vector2 position) =>
            {
                OnStartDragging?.Invoke(this, new DraggingArgs()
                {
                    position = elements[currentIndex].transform.position,
                    //index = currentIndex,
                    positionIndex = permutation[currentIndex]
                });
            };
            draggingHandlers[i] = (object sender, Vector2 position) =>
            {
                OnPositionChanged?.Invoke(this, new DraggingArgs() { 
                    position = elements[currentIndex].transform.position, 
                    //index = currentIndex , 
                    positionIndex = permutation[currentIndex] });
            };
            endDraggingHandlers[i] = (object sender, Vector2 position) => OnSimpleEndDragging(sender, position, currentIndex);
        }
    }

    private void Start()
    {
        SetActive(isActive);
    }

    #region ------------------ Positions ------------------
    // TODO: Can decouple this section from the class, to support more general positions.
    private void CreatePositions()
    {
        positions = new Vector3[size];
        for (int i = 0; i < size; i++)
        {
            positions[i] = new Vector3(0, heightDiff * i, 0);
        }
    }

    private int GetClosestPositionIndex(Vector2 point)
    {
        int secondTargetId = Mathf.RoundToInt(point.y / heightDiff);
        if (0 <= secondTargetId && secondTargetId < size &&
            (point - new Vector2(0, secondTargetId * heightDiff)).magnitude < latchingDistance)
        {
            return permutation.Inverse(secondTargetId);
        }
        return -1;
    }

    internal float GetHeightDiff()
    {
        return heightDiff;
    }

    internal void SetHeightDiff(float heightDiff)
    {
        this.heightDiff = heightDiff;
        CreatePositions();
        for (int i = 0; i < size; i++)
        {
            elements[i].transform.localPosition = positions[permutation[i]];
        }
    }

    #endregion

    #region ------------------ Enable \ Disable ------------------

    private void OnEnable()
    {
        for (int i = 0; i < size; i++)
        {
            elements[i].OnPressed += pressedHandlers[i];
            elements[i].OnStartDragging += startDraggingHandlers[i];
            elements[i].OnDragging += draggingHandlers[i];
            elements[i].OnEndDragging += endDraggingHandlers[i];
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < size; i++)
        {
            elements[i].OnPressed -= pressedHandlers[i];
            elements[i].OnStartDragging -= startDraggingHandlers[i];
            elements[i].OnDragging -= draggingHandlers[i];
            elements[i].OnEndDragging -= endDraggingHandlers[i];
        }
    }

    #endregion

    #region ------------------ Dragging ------------------

    public void SetActive(bool active)
    {
        isActive = active;
        foreach (DragAndDrop elem in elements)
        {
            elem.dragIsActive = active;
        }
    }

    public class DraggingArgs
    {
        public Vector2 position;
        public int positionIndex;
    }

    public class EndDraggingArgs
    {
        public Vector2 position;            // Position where the dragging finished.
        public bool changed;                // True if the dragging cause a change.
        public Permutation transposition;   // The transposition the dragging created,
                                            // or the identity if it wasn't successfull.
        //public int source;
        public int target;                  // The previous index of the object that was dragged
    }

    public event EventHandler<DraggingArgs> OnPositionChanged;

    public event EventHandler<DraggingArgs> OnPressed;

    public event EventHandler<DraggingArgs> OnStartDragging;

    public event EventHandler<DraggingArgs> OnDragging;
    // When permutation is changed from dragging, it is done in the EndDragging phase.
    public event EventHandler<EndDraggingArgs> OnEndDragging;


    private void OnSimpleEndDragging(object sender, Vector2 position, int index)
    {
        // TODO: move draggable element to the front of the scene, so it will always be
        //       before the other elements.

        Vector2 localPosition = transform.InverseTransformPoint(position);

        int secondIndex = GetClosestPositionIndex(localPosition);

        if (secondIndex != -1 && secondIndex != index)
        {
            int targetId = permutation[index];
            int secondTargetId = permutation[secondIndex];

            elements[index].transform.localPosition = positions[secondTargetId];
            elements[secondIndex].transform.localPosition = positions[targetId];

            Permutation transposition = Permutation.Cycle(size, targetId, secondTargetId);
            permutation = transposition * permutation;
            permutationString = permutation.ToString();

            OnEndDragging?.Invoke(this, new EndDraggingArgs() { 
                position = position,
                transposition = transposition,
                //source = index, 
                target = secondTargetId, 
                changed = (targetId != secondTargetId) 
            });
        }
        else
        {
            // return to previous position
            int targetId = permutation[index];
            elements[index].transform.localPosition = positions[targetId];

            OnEndDragging?.Invoke(this, new EndDraggingArgs()
            {
                position = position,
                transposition = new Permutation(size),
                //source = index,
                target = targetId,
                changed = false
            });
        }

    }

    #endregion

    public void SetColors(Color[] colors)
    {
        for (int i = 0; i < elements.Length; i++)
        {
            elements[i].GetComponent<SpriteRenderer>().color = colors[permutation[i]];
        }
    }
/*
    public Permutation GetPermutation()
    {
        return permutation; // TODO: should not be able to change the permutation from outside. Return a copy, or make it a struct?
    }
    
    internal void SetPermutation(Permutation p)
    {
        if (p != null && p.size == size)
        {
            permutation = p;
            permutationString = p.ToString();

            for (int i = 0;i < size; i++)
            {
                elements[i].transform.localPosition = positions[permutation[i]];
            }
        }
    }*/
}
