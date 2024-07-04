using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ChainPermutation : MonoBehaviour
{

    [SerializeField] private int size = 5;
    [SerializeField] private PermutationLines[] lines;
    [SerializeField] private DragablePermutation[] connections;
    [SerializeField] private ConnectionType[] connectionTypes;

    [SerializeField] private Permutation[] permutations;

    private Color[] currentColors;

    public enum ConnectionType{
        TO_LEFT = -1,
        TO_BOTH = 0,
        TO_RIGHT = 1
    }

    private void Awake()
    {
        currentColors = PipesRoom.colors;
        input = new Permutation(size);
        int n = lines.Length;
        if (connections.Length - 1 != n || connectionTypes.Length - 1 != n)
        {
            throw new System.Exception("There should be one more connection than lines");
        }

        for (int i = 0; i < n+1; i++)
        {
            connectionTypes[i] = ConnectionType.TO_BOTH;
        }

        for (int i = 0; i < n; i++)
        {
            DragablePermutation leftConnection = null;
            if ((connections[i] != null) && (0 <= (int)connectionTypes[i]))
                leftConnection = connections[i];

            DragablePermutation rightConnection = null;
            if ((connections[i+1] != null) && (0 >= (int)connectionTypes[i+1]))
                rightConnection = connections[i+1];

            if (leftConnection!=null || rightConnection!=null)
            {
                TwoSidedDraggablePermutation lineConnectors = lines[i].AddComponent<TwoSidedDraggablePermutation>();
                lineConnectors.SetConnectors(leftConnection, rightConnection);
            }
        }
    }

    private void Start()
    {
        SetColors(currentColors);
    }

    private void OnEnable()
    {
        foreach (var line in lines)
        {
            line.OnPermutationChanged += OnPermutationChanged;
        }
    }

    private void OnDisable()
    {
        foreach (var line in lines)
        {
            line.OnPermutationChanged -= OnPermutationChanged;
        }
    }

    Permutation input;

    private void OnPermutationChanged(object sender, Permutation e)
    {
        SetColors(currentColors);
    }

    public void SetColors(Color[] colors)
    {
        currentColors = colors;
        colors = input.Inverse().InverseOutput().Select(i => colors[i]).ToArray();
        foreach (var connection in connections)
        {
            if (connection != null)
            {
                connection.SetColors(colors);
            }
        }

        if (connections[0] != null)
        {
            Permutation p = connections[0].GetPermutation();
            colors = p.InverseOutput().Select(i => colors[i]).ToArray();
        }

        foreach (var line in lines)
        {
            line.SetColors(colors);
            Permutation p = line.GetPermutation();
            colors = p.InverseOutput().Select(i => colors[i]).ToArray();
        }
    }
}
