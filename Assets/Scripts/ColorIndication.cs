using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorIndication : MonoBehaviour
{

    [SerializeField] private SpriteRenderer[] objects;
    [SerializeField] private SpriteRenderer[] separators;

    public void Awake()
    {
        SetColors(new Color[] {
            PipesRoom.colors[0], PipesRoom.colors[1], PipesRoom.colors[2], 
            PipesRoom.colors[3], PipesRoom.colors[4]
        });
    }

    public void SetColors(Color[] colors)
    {
        if (colors == null || colors.Length != objects.Length)
            return;

        for (int i = 0; i < colors.Length; i++)
        {
            objects[i].color = colors[i];
        }
    }

    public void SeparateByColor(Color[] colors)
    {
        if (colors == null || colors.Length != objects.Length)
            return;

        for (int i = 0; i < colors.Length; i++)
        {
            separators[i].enabled = (objects[i].color == colors[i]);
        }
    }
}
