using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PipesRoom : MonoBehaviour
{

    public static Color[] colors = {
        Color.red, Color.green, Color.blue,
        Color.yellow, Color.magenta, Color.cyan,
        Color.black, Color.white
    };

    public static Color[] ToColors(Permutation permutation)
    {
        return permutation.Select(i => colors[i]).ToArray();
    }

    /*public static Color[] colors = {
        new Color(0.5f, 0, 0),    new Color(0, 0.5f, 0),    new Color(0, 0, 0.5f),
        new Color(0.5f, 0.5f, 0), new Color(0, 0.5f, 0.5f), new Color(0.5f, 0, 0.5f),
        new Color(0, 0, 0), new Color(0.5f, 0.5f, 0.5f), new Color(0.5f, 0, 0.5f)
    };*/


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


    private void Awake()
    {
        int size = permutation.size;        

        //int permutationSize = permWithInput.permutationSize;
        pipes = new PermutationPipe[size];
        for (int i = 0; i < size; i++)
        {
            pipes[i] = Instantiate(pipePrefab, transform);
            pipes[i].SetIdName("" + i);
            pipes[i].SetColor(colors[i]);
            pipes[i].SetDimensions(widthLeft, widthPermutation, widthRight, heightDiff);
            pipes[i].GetTarget().OnEndDragging += OnEndDragging;

        }
        UpdateVisualComponent(permutation);
                
        PermutationLines permutationLines = Instantiate(linesPrefab, transform);
        permutationLines.gameObject.name = "permutation lines";
        permutationLines.transform.localPosition = new Vector3(widthLeft, 4, 0);

        permutationLines.SetSize(size);
        permutationLines.SetDimension(widthPermutation, heightDiff);

        DragablePermutation leftPermutation = Instantiate(dragPermutation, transform);
        DragablePermutation rightPermutation = Instantiate(dragPermutation, transform);

    }

    private void PermWithInput_OnInputChanged(object sender, EventArgs e)
    {
        /*foreach (var (index, pipe) in Enumerable.Zip(permWithInput.GetInput(), pipes, (a, b) => (a, b)))
        {
            pipe.SetColor(colors[index]);
        }*/
    }

    #region ------------- permutation handling -------------

    // Called when the permutation changes from the drag and drop mechanism
    public event EventHandler<Permutation> OnInnerPermutationChanged;

    private void OnEndDragging(object sender, Vector2 position)
    {
        // extract from the visual component a permutation change.

        int n = permutation.size;

        // Find the pipe that was moved. Note that it can (though should not) raise an
        // exception if there is not pipe corresponding to the sender.
        int sourceId = Enumerable.Range(0, n)
            .First(i => (object)pipes[i].GetTarget() == sender);

        int targetId = pipes[sourceId].targetId;
                
        Vector2 localPosition = transform.InverseTransformPoint(position);
        Debug.Log($"End dragging {sender} {sourceId} at position {position}, local position {localPosition}");
        int secondTargetId = Mathf.RoundToInt(localPosition.y/heightDiff);
        Vector2 targetCenter = new Vector2(widthLeft + widthPermutation, secondTargetId * heightDiff);

        if (0 <= secondTargetId && secondTargetId < pipes.Length && 
            (localPosition - targetCenter).magnitude < heightDiff/2 &&
            !isStatic)
        {
            Permutation transposition = Permutation.Cycle(n, targetId, secondTargetId);
            int secondSourceId = permutation.Inverse(secondTargetId);
            Permutation newPermutation = transposition * permutation;
            UpdatePermutation(newPermutation);
            OnInnerPermutationChanged?.Invoke(this, newPermutation);
        }
        else
        {
            // don't apply transposition, return to previous visual
            pipes[sourceId].SetIDs(sourceId, targetId);
        }
    }

    public Permutation GetPermutation()
    {
        return permutation;
    }

    public Permutation GetInput()
    {
        return input;
    }

    public void UpdatePermutation(Permutation p)
    {
        if (p.size != permutation.size)
            return;

        permutation = p;

        UpdateVisualComponent(permutation);
    }

    private void UpdateVisualComponent(Permutation permutation)
    {
        for (int i = 0; i < pipes.Length; i++)
        {
            pipes[i].SetIDs(i, permutation[i]);
        }
    }


    #endregion

    #region ------------- color scheme -------------

    public void SetLeftColors(Color[] leftColors)
    {
        if (leftColors.Length != permutation.size)
            return;
        for (int i = 0; i < leftColors.Length;i++)
        {
            pipes[i].SetColor(leftColors[i]);
        }
    }

    public void SetRightColors(Color[] rightColors)
    {
        if (rightColors.Length != permutation.size)
            return;
        for (int i = 0; i < rightColors.Length; i++)
        {
            pipes[permutation.Inverse(i)].SetColor(rightColors[i]);
            //lines[permutation.Inverse(i)].startColor = rightColors[i];
            //lines[permutation.Inverse(i)].endColor = rightColors[i];
            //links[permutation.Inverse(i)].SetColor(rightColors[i]);
        }
    }

    public Color[] GetLeftColors()
    {
        return pipes.Select(pipe => pipe.GetColor()).ToArray();
    }

    public Color[] GetRightColors()
    {
        return permutation.InverseOutput().Select(i => pipes[i].GetColor()).ToArray();
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
