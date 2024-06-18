using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooth : MonoBehaviour
{

    [SerializeField] private SpriteRenderer renderer;

    
    public void SetColor(Color color)
    {
        renderer.color = color;
    }
}
