using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A permutation room consists of a permutation and its input (represented by another permutation).???
/// </summary>
abstract public class PermutationRoom : MonoBehaviour
{
    abstract public void SetPermutation(Permutation permutation);

    abstract public Permutation GetPermutation();

    abstract public void SetInput(Permutation input);

    abstract public Permutation GetInput();

    abstract public void SetOutput(Permutation input);

    abstract public Permutation GetOutput();
}
