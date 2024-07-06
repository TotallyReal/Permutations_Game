using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using static ChainPermutation;

public class ChainPermutation : MonoBehaviour
{

    [SerializeField] private int permutationSize = 5;

    [SerializeField] private bool generateFromExistingObjects = true;
    // TODO: consider separating into the permutation chain logic, and the creation.

    [Header("Use Existing Objects:")]
    [SerializeField] private PermutationLines[] lines;
    [SerializeField] private DragablePermutation[] connections;


    [SerializeField] private Permutation[] permutations;
    [Header("Generate Objects:")]
    [SerializeField] private PermutationLines permutationLinePrefab;
    [SerializeField] private int linePermutationCount;
    [SerializeField] private float[] widths;
    [SerializeField] private float heightDiff;
    [SerializeField] private DragablePermutation draggablePermutationPrefab;

    [Header("Connection types")]
    [SerializeField] private ConnectionData[] connectionData;

    private Color[] currentColors;
    
    [Serializable]
    public class ConnectionData
    {
        public bool show = true;
        public bool connectLeft = true;
        public bool connectRight = true;
        public bool active = true;
    }

    private void Awake()
    {
        if (!generateFromExistingObjects)
        {
            Assert.AreEqual(linePermutationCount, widths.Length);
            Assert.AreEqual(linePermutationCount, connectionData.Length-1);

            lines = new PermutationLines[linePermutationCount];
            connections = new DragablePermutation[linePermutationCount+1];

            connections[0] = Instantiate(draggablePermutationPrefab, transform);
            connections[0].transform.localPosition = Vector3.zero;
            connections[0].SetHeightDiff(heightDiff);
            connections[0].SetActive(connectionData[0].active);
            connections[0].gameObject.SetActive(connectionData[0].show);

            float x = 0;
            for (int i = 0; i < linePermutationCount; i++)
            {
                lines[i] = Instantiate(permutationLinePrefab, transform);
                lines[i].SetDimension(widths[i], heightDiff);
                lines[i].transform.localPosition = new Vector3(x, 0, 0);

                x += widths[i];

                connections[i + 1] = Instantiate(draggablePermutationPrefab, transform);
                connections[i + 1].transform.localPosition = new Vector3(x, 0, 0);
                connections[i + 1].SetHeightDiff(heightDiff);
                connections[i + 1].SetActive(connectionData[i+1].active);
                connections[i + 1].gameObject.SetActive(connectionData[i + 1].show);
            }
        }
        CreateChain(permutationSize, lines, connections, connectionData, permutations);
    }


    protected void CreateChain(int size, PermutationLines[] lines, DragablePermutation[] connections, ConnectionData[] connectionData, Permutation[] permutations)
    {
        currentColors = PipesRoom.colors;
        int n = lines.Length;
        if (connections.Length - 1 != n || connectionData.Length - 1 != n)
        {
            throw new System.Exception("There should be one more connection than lines");
        }

        for (int i = 0; i < n; i++)
        {
            DragablePermutation leftConnection = null;
            if ((connections[i] != null) && connectionData[i].connectRight)
                leftConnection = connections[i];

            DragablePermutation rightConnection = null;
            if ((connections[i] != null) && connectionData[i + 1].connectLeft)
                rightConnection = connections[i + 1];

            if (leftConnection != null || rightConnection != null)
            {
                TwoSidedDraggablePermutation lineConnectors = lines[i].AddComponent<TwoSidedDraggablePermutation>();
                lineConnectors.SetConnectors(leftConnection, rightConnection);
            }
        }

    }

    private void Start()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i] != null)
            {
                connections[i].SetActive(connectionData[i].active);
            }
        }
        UpdatePermutation();
    }

    private void OnEnable()
    {
        if (lines != null)
        {
            foreach (var line in lines)
            {
                line.OnPermutationChanged += OnLinePermutationChanged;
            }
        }
    }

    private void OnDisable()
    {
        if (lines != null)
        {
            foreach (var line in lines)
            {
                line.OnPermutationChanged -= OnLinePermutationChanged;
            }
        }
    }

    private Permutation permutation;

    public event EventHandler<Permutation> OnPermutationChanged;

    private void OnLinePermutationChanged(object sender, Permutation e)
    {
        UpdatePermutation();
    }

    private void UpdatePermutation()
    {
        permutation = new Permutation(lines[0].GetPermutation().size); // TODO: this is awful. Add the size as parameter
        foreach (var line in lines)
        {
            permutation = line.GetPermutation() * permutation;
        }
        SetLeftColors(currentColors);

        OnPermutationChanged?.Invoke(this, permutation);
    }

    public Permutation GetPermutation()
    {
        return permutation;
    }

    public void SetPermutation(int index, Permutation permutation)
    {
        lines[index].UpdatePermutation(permutation);
    }

    public void SetLeftColors(Color[] colors)
    {
        if (lines == null)
            return;

        currentColors = colors;
        if (connections[0] != null)
            connections[0].SetColors(colors);

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].SetColors(colors);
            Permutation p = lines[i].GetPermutation();
            colors = p.InvApplyTo(colors);

            if (connections[i + 1] != null)
                connections[i + 1].SetColors(colors);
        }
    }

    public Color[] GetLeftColors()
    {
        return currentColors;
    }

    public void SetRightColors(Color[] colors)
    {
        SetLeftColors(permutation.ApplyTo(colors));
    }

    public Color[] GetRightColors()
    {
        return permutation.InvApplyTo(currentColors);
    }
}
