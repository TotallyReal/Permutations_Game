using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NumberLabel))]
public class CrossingSum : MonoBehaviour
{
    [SerializeField] private PermutationLines[] tsPermutations;
    private NumberLabel numberLabel;

    private void Awake()
    {
        numberLabel = GetComponent<NumberLabel>();
    }

    private void Start()
    {
        UpdateSum();
    }

    private void OnEnable()
    {
        foreach (var tsPermutation in tsPermutations)
        {
            tsPermutation.OnPermutationChanged += OnPermutationUpdated;
        }
    }

    private void OnDisable()
    {
        foreach (var tsPermutation in tsPermutations)
        {
            tsPermutation.OnPermutationChanged -= OnPermutationUpdated;
        }
    }

    private void OnPermutationUpdated(object sender, Permutation e)
    {
        UpdateSum();
    }

    private void UpdateSum()
    {
        int sum = 0;
        foreach (var tsPermutation in tsPermutations)
        {
            sum += tsPermutation.GetPermutation().CrossingNumber();
        }
        numberLabel.SetNumber(sum);
    }
}
