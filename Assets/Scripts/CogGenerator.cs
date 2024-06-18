using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CogGenerator : MonoBehaviour
{

    [SerializeField] private Cog cogPrefab;
    
    public void GenerateWheel(float radius, int steps)
    {
        
        Instantiate(cogPrefab, transform);
    }

}
