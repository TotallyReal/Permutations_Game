using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

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
    [SerializeField] private ConnectionType[] connectionTypes;
    [SerializeField] private ConnectionData[] connectionData;

    private Color[] currentColors;
    
    [Serializable]
    public class ConnectionData
    {
        public bool connectLeft = true;
        public bool connectRight = true;
        public bool active = true;
    }

    public enum ConnectionType{
        TO_LEFT = -1,
        TO_BOTH = 0,
        TO_RIGHT = 1
    }

    private void Awake()
    {
        if (!generateFromExistingObjects)
        {
            Assert.AreEqual(linePermutationCount, widths.Length);
            Assert.AreEqual(linePermutationCount, connectionTypes.Length-1);

            lines = new PermutationLines[linePermutationCount];
            connections = new DragablePermutation[linePermutationCount+1];

            connections[0] = Instantiate(draggablePermutationPrefab, transform);
            connections[0].transform.localPosition = Vector3.zero;
            connections[0].SetHeightDiff(heightDiff);
            connections[0].SetActive(connectionData[0].active);

            float x = 0;
            for (int i = 0; i < linePermutationCount; i++)
            {
                lines[i] = Instantiate(permutationLinePrefab, transform);
                lines[i].SetDimension(widths[i], heightDiff);
                lines[i].transform.localPosition = new Vector3(x, 0, 0);

                x += widths[i];

                connections[i+1] = Instantiate(draggablePermutationPrefab, transform);
                connections[i+1].transform.localPosition = new Vector3(x, 0, 0);
                connections[i+1].SetHeightDiff(heightDiff);
                connections[i+1].SetActive(connectionData[i+1].active);
            }
        }
        CreateChain(permutationSize, lines, connections, connectionTypes, permutations);
    }


    protected void CreateChain(int size, PermutationLines[] lines, DragablePermutation[] connections, ConnectionType[] connectionTypes, Permutation[] permutations)
    {
        currentColors = PipesRoom.colors;
        input = new Permutation(size);
        int n = lines.Length;
        if (connections.Length - 1 != n || connectionTypes.Length - 1 != n)
        {
            throw new System.Exception("There should be one more connection than lines");
        }

        for (int i = 0; i < n + 1; i++)
        {
            connectionTypes[i] = ConnectionType.TO_BOTH;
        }

        for (int i = 0; i < n; i++)
        {
            DragablePermutation leftConnection = null;
            //if ((connections[i] != null) && (0 <= (int)connectionTypes[i]))
            if ((connections[i] != null) && connectionData[i].connectRight)
                leftConnection = connections[i];

            DragablePermutation rightConnection = null;
            //if ((connections[i + 1] != null) && (0 >= (int)connectionTypes[i + 1]))
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
        UpdateConnections();
        SetColors(currentColors);
    }

    private void OnEnable()
    {
        if (lines != null)
        {
            foreach (var line in lines)
            {
                line.OnPermutationChanged += OnPermutationChanged;
            }
        }
    }

    private void OnDisable()
    {
        if (lines != null)
        {
            foreach (var line in lines)
            {
                line.OnPermutationChanged -= OnPermutationChanged;
            }
        }
    }

    Permutation input;

    private void UpdateConnections()
    {
        /*Permutation p = input;

        if (connections[0] != null)
        {
            connections[0].SetPermutation(p);
        }
        for (var i = 1; i < lines.Length; i++)
        {
            p *= lines[i - 1].GetPermutation();
            if (connections[i] != null)
                connections[i].SetPermutation(p);
        }*/

    }

    private void OnPermutationChanged(object sender, Permutation e)
    {

        SetColors(currentColors);
    }

    public void SetColors(Color[] colors)
    {
        if (lines == null)
            return; 

        currentColors = colors;
        colors = input.Inverse().InverseOutput().Select(i => colors[i]).ToArray();
        if (connections[0] != null)
            connections[0].SetColors(colors);

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].SetColors(colors);
            Permutation p = lines[i].GetPermutation();
            colors = p.InverseOutput().Select(i => colors[i]).ToArray();
            connections[i + 1].SetColors(colors);
        }
    }
}
