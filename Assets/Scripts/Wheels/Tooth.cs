using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooth : MonoBehaviour
{

    [SerializeField] private SpriteRenderer toothRenderer;

    
    public void SetColor(Color color)
    {
        toothRenderer.color = color;
    }
}
