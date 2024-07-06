using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(ChainPermutation))]
public class PipesRoom : MonoBehaviour
{

    public static Color[] colors = {
        Color.red, Color.green, Color.blue,
        Color.yellow, Color.magenta, Color.cyan,
        Color.black, Color.white
    };

    public static Color[] ToColors(Permutation permutation)
    {
        return permutation.ApplyTo(colors);
    }



    [Header("Visual component")]
    [SerializeField] private PermutationLines linesPrefab;
    [SerializeField] private DragablePermutation dragPermutation;
    [SerializeField] private PermutationPipe pipePrefab;
    private PermutationPipe[] pipes;
    [SerializeField] private float heightDiff = 1;
    [SerializeField] private float widthLeft = 6;
    [SerializeField] private float widthPermutation = 4;
    [SerializeField] private float widthRight = 6;

    [Header("Logic")]
    [SerializeField] private bool isStatic = false;
    private Permutation permutation = new Permutation(5);
    private Permutation input = new Permutation(5);

    private ChainPermutation chainPermutation;

    private void Awake()
    {
        int size = permutation.size;
        if (!TryGetComponent<ChainPermutation>(out chainPermutation))
        {
            throw new Exception("Should have a ChainPermutation");
        }

        chainPermutation.OnPermutationChanged += (obj, permutation) => {
            this.permutation = permutation;
            OnInnerPermutationChanged?.Invoke(this, permutation);
        };              

    }

    #region ------------- permutation handling -------------

    // Called when the permutation changes from the drag and drop mechanism
    public event EventHandler<Permutation> OnInnerPermutationChanged;

    public Permutation GetPermutation()
    {
        return chainPermutation.GetPermutation();
    }

    public Permutation GetInput()
    {
        return input;
    }

    public void UpdatePermutation(Permutation p)
    {
        chainPermutation.SetPermutation(1, p);
    }

    #endregion

    #region ------------- color scheme -------------

    public void SetLeftColors(Color[] leftColors)
    {
        chainPermutation.SetLeftColors(leftColors);
    }

    public void SetRightColors(Color[] rightColors)
    {
        chainPermutation.SetRightColors(rightColors);
    }

    public Color[] GetLeftColors()
    {
        return chainPermutation.GetLeftColors();
    }

    public Color[] GetRightColors()
    {
        return chainPermutation.GetRightColors();
    }

    #endregion


    internal void CreateFrom(PortalApartment.RoomInfo info, PortalApartment.PortalSide side)
    {
        var input = info.middleInput;
        switch (side)
        {
            case PortalApartment.PortalSide.LEFT:
                SetRightColors(PipesRoom.ToColors(input));
                break;
            case PortalApartment.PortalSide.MIDDLE:
                SetLeftColors(PipesRoom.ToColors(input));
                break;
            case PortalApartment.PortalSide.RIGHT:
                var output = input * info.permutation.Inverse();
                SetLeftColors(PipesRoom.ToColors(output));
                if (info.finishRoom)
                    UpdatePermutation(new Permutation(5));
                break;
        }
    }

    public void CopyRoom(PipesRoom toCopy)
    {
        //TODO: right now only copy color scheme
        SetLeftColors(toCopy.GetLeftColors());
    }
}
