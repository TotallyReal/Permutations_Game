using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gate : MonoBehaviour
{

    /// <summary>
    /// Tries to open the gate. Returns true if the gate is open after this call.
    /// </summary>
    /// <returns></returns>
    abstract public bool TryOpen();

    abstract public void Close();
}
