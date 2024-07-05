using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPermutation : MonoBehaviour
{

    [SerializeField] private string permutationString = "";
    [SerializeField] private float heightDiff = 1f;
    [Range(2, 12)]
    [SerializeField] private int size = 5;
    [SerializeField] private DragAndDrop draggablePrefab;

    private Permutation permutation;
}
