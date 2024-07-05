using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OnOffColors : MonoBehaviour
{

    [SerializeField] private DragablePermutation dPermutation;
    [SerializeField] private ChainPermutation chain;
    [SerializeField] private int size = 5;

    private Color invisible = new Color(0, 0, 0, 0);

    private bool[] colorsOn;

    private void Awake()
    {
        colorsOn = new bool[size];
        for (int i = 0; i < size; i++)
        {
            colorsOn[i] = true;
        }
    }

    private void OnEnable()
    {
        dPermutation.OnPressed += OnPressed;
    }

    private void OnDisable()
    {
        dPermutation.OnPressed -= OnPressed;
    }

    private void OnPressed(object sender, DragablePermutation.DraggingArgs args)
    {
        colorsOn[args.positionIndex] = !colorsOn[args.positionIndex];
        chain.SetColors(colorsOn.Select((b, index) => b ? PipesRoom.colors[index] : invisible).ToArray());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
