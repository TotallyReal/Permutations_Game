using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PermutationWithInput
{
    private Permutation permutation;
    private int[] input;

    public PermutationWithInput(int size)
    {
        size = Mathf.Max(size, 1);
        permutation = new Permutation(size);
        input = Enumerable.Range(0,size).ToArray();
    }

    public PermutationWithInput(Permutation permutation, params int[] input)
    {
        // TODO: chane input into another permutation
        if (permutation == null || input == null || permutation.size != input.Length)
            throw new Exception("Invalid parameters for PermutationWithInput");

        this.permutation = permutation;
        this.input = input;
    }

    #region ---------------------- input\output ----------------------

    public IEnumerable<int> GetInput()
    {
        return input;
    }

    public void SetInput(params int[] input)
    {
        if (input.Length != this.input.Length)
            throw new Exception($"The new input must have length {this.input.Length}");

        for (int i = 0; i < input.Length; i++)
        {
            this.input[i] = input[i];
        }
        OnInputChanged?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerable<int> GetOutput()
    {
        return input.Select(i => permutation[i]);
    }

    private void UpdateInputFromOutput(int[] output)
    {
        if (output.Length != this.input.Length)
            throw new Exception($"The new output must have length {this.input.Length}");

        for (int i = 0; i < input.Length; i++)
        {
            this.input[i] = permutation.Inverse(output[i]);
        }
    }

    public void SetOutput(params int[] output)
    {
        UpdateInputFromOutput(output);
        OnInputChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler OnInputChanged;

    #endregion

    #region ---------------------- permutation ----------------------

    public Permutation GetPermutation()
    {
        return permutation;
    }

    public enum UpdateType
    {
        UPDATE_OUTPUT, UPDATE_INPUT
    }

    public void SetPermutation(Permutation permutation, UpdateType updateType = UpdateType.UPDATE_OUTPUT)
    {
        if (permutation == null || (permutation.size != this.permutation.size))
            throw new Exception($"Invalid permutation {permutation}");

        if (updateType == UpdateType.UPDATE_OUTPUT)
        {
            int[] oldOutput = GetOutput().ToArray();
            this.permutation = permutation;
            UpdateInputFromOutput(oldOutput);
        } else
        {
            this.permutation = permutation;
        }

        OnPermutationChanged?.Invoke(this, EventArgs.Empty);
    }

    // Note that a permutation change always changes the input or output as well (though not both)
    public event EventHandler OnPermutationChanged;

    #endregion

}

public class PermutationWithInputChain
{

    private PermutationWithInput[] elements;

    public PermutationWithInputChain(params PermutationWithInput[] elements)
    {
        this.elements = elements;
    }

}
