using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ChainPermutationGenerator : ChainPermutation
{

    [SerializeField] private int size = 5;
    [SerializeField] private PermutationLines[] lines;
    [SerializeField] private DragablePermutation[] connections;
    [SerializeField] private ConnectionType[] connectionTypes;

    [SerializeField] private Permutation[] permutations;


    private void Awake()
    {
        CreateChain(size, lines, connections, connectionTypes, permutations);
    }

}
