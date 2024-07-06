using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayCrossingNumber : MonoBehaviour
{

    [SerializeField] private NumberLabel numberLabel;
    private PermutationLines mPermutation;

    private void Awake()
    {
        mPermutation = GetComponentInChildren<PermutationLines>();
    }

    private void OnEnable()
    {
        mPermutation.OnPermutationChanged += OnPermutationChanged;
    }

    private void OnDisable()
    {
        mPermutation.OnPermutationChanged -= OnPermutationChanged;
    }

    private void OnPermutationChanged(object sender, Permutation p)
    {
        numberLabel.SetNumber(p.CrossingNumber());
    }

}
