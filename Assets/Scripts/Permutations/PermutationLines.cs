using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PermutationLines : MonoBehaviour
{

    /**
     * Permutation input;
     * Permutation output;
     * 
     * UpdateInput\Output:
     * input[i] -> output[i]
     * permutation = output * input^-1
     * permutation * input  = output
     */

    [Header("Logic")]
    [SerializeField] private int size;
    [SerializeField] private string permString = "";
    [Header("Visual")]
    [SerializeField] private float heightDiff;
    [SerializeField] private float width = 2f;
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private bool addInitialColors = false;

    //TODO: maybe add here the collider ?

    private Permutation permutation;
    private LineRenderer[] lines;

    virtual protected void Awake()
    {
        permutation = Permutation.FromString(size, permString);
        lines = new LineRenderer[size];
        for (int i = 0; i < size; i++)
        {
            lines[i] = Instantiate(linePrefab, transform);
            lines[i].positionCount = 2;
        }

        if (TryGetComponent<BoxCollider2D>(out BoxCollider2D collider))
        {
            collider.offset = new Vector2(width/2, (size - 1) * heightDiff / 2);
            collider.size = new Vector2(width, size * heightDiff);
        }
    }

    virtual protected void Start()
    {
        UpdatePermutation(permutation);
        if (addInitialColors)
            SetColors(PipesRoom.colors);
    }

    public event EventHandler<Permutation> OnPermutationChanged;

    public Permutation GetPermutation()
    {
        return permutation;
    }

    public int GetSize()
    {
        return size;
    }

    public void UpdatePermutation(Permutation permutation)
    {
        if (permutation == null || permutation.size != size)
            throw new System.Exception($"The permutation {permutation} is invalid");

        this.permutation = permutation;
        permString = permutation.ToString();

        for (int i = 0; i < size; i++)
        {
            lines[i].SetPosition(0, new Vector3(0, i * heightDiff, 0));
            lines[i].SetPosition(1, new Vector3(width, permutation[i] * heightDiff, 0));
        }

        OnPermutationChanged?.Invoke(this, permutation);
    }

    public void SetEndPoint(bool left, int index, Vector2 position)
    {
        position = transform.InverseTransformPoint(position);
        if (left)
        {
            lines[index].SetPosition(0, position);
        }
        else
        {
            lines[permutation.Inverse(index)].SetPosition(1, position);
        }
    }

    public float GetHeightDiff()
    {
        return heightDiff;
    }

    public void SetColors(Color[] colors)
    {
        for (int i = 0; i < size; i++)
        {
            lines[i].startColor = colors[i];
            lines[i].endColor = colors[i];
        }
    }


}
