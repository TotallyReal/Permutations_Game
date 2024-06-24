using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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


    // visual component
    [SerializeField] private PermutationPipe pipePrefab;
    private PermutationPipe[] pipes;
    [SerializeField] private float heightDiff = 1;
    [SerializeField] private float widthLeft = 6;
    [SerializeField] private float widthPermutation = 4;
    [SerializeField] private float widthRight = 6;

    // logic
    [SerializeField] private bool isStatic = false;
    private Permutation permutation = new Permutation(5);
    private Permutation input = new Permutation(5);


    private void Awake()
    {
        int size = permutation.size;        

        //int size = permWithInput.size;
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

public class Permutation : IEnumerable<int>
{

    private int[] map;
    private int[] inverse;
    public int size { get; private set; }

    public static Permutation Cycle(int size, params int[] elements)
    {
        Permutation p = new Permutation(size);

        if (elements.Length == 0)
            return p;

        foreach (int element in elements)
        {
            if (element < 0 || size <= element)
                throw new Exception($"elements must be in [0,...,{size}-1]");
        }

        int last = elements[0];

        for (int i = 1; i < elements.Length; i++)
        {
            if (p.map[elements[i]] != elements[i] )
                throw new Exception($"elements {elements} do not define a cycle");

            p.map[last] = elements[i];
            p.inverse[elements[i]] = last;
            last = elements[i];
        }

        p.map[last] = elements[0];
        p.inverse[elements[0]] = last;

        return p;
    }

    public Permutation(int n)
    {
        size = Mathf.Max(1, n);
        map = new int[size];
        inverse = new int[size];
        for (int i = 0; i < size; i++)
        {
            map[i] = i;
            inverse[i] = i;
        }
    }

    public int this[int i]
    {
        get { return map[i]; }
    }

    public int Inverse(int i)
    {
        return inverse[i]; 
    }

    public Permutation Inverse()
    {
        Permutation inverseP = new Permutation(size);
        for (int i = 0;i < size;i++)
        {
            int target = this[i];
            inverseP.map[target] = i;
            inverseP.inverse[i] = target;
        }
        return inverseP;
    }

    public static Permutation operator *(Permutation a, Permutation b)
    {
        if (a.size != b.size)
            throw new Exception("Permutations must be over the same set size.");

        int size = a.size;
        Permutation p = new Permutation(size);
        for (int i = 0; i < size; i++)
        {
            p.map[i] = a.map[b.map[i]];
            p.inverse[p.map[i]] = i;
        }
        return p;
    }

    public static bool operator ==(Permutation a, Permutation b)
    {
        if (a is null)
            return (b is null);
        if (b is null) return false;
        // now both Permutations are not null

        if (a.size != b.size)
            throw new Exception("Cannot compare permutations on different sets");

        for (int i = 0; i < a.size; i++)
        {
            if (a[i] != b[i])
                return false;
        }

        return true;
    }

    public static bool operator !=(Permutation a, Permutation b)
    {
        return !(a == b);
    }

    public bool IsIdentity()
    {
        for (int i = 0; i < size; i++)
        {
            if (this[i] != i)
                return false;
        }
        return true;
    }

    public int Order()
    {
        //TODO : consider implementing using cycle orders.
        int order = 1;
        Permutation p = new Permutation(size) * this;
        while (!p.IsIdentity())
        {
            order += 1;
            p *= this;
        }
        return order;
    }

    override public String ToString()
    {
        bool[] used = new bool[size];
        string name = "";
        string cycle = "";
        for (int i = 0; i < size; i++)
        {
            if (!used[i])
            {
                used[i] = true;
                int startingPoint = i;
                int position = i;
                cycle = "" + i;
                while (this[position] != startingPoint)
                {
                    position = this[position];
                    cycle += "," + position;
                    used[position] = true;
                }
                name += $"({cycle})";
            }
        }

        return name;
    }

    public IEnumerable<int> Output()
    {
        for (int i = 0; i < size; i++)
        {
            yield return this[i];
        }
    }

    public IEnumerable<int> InverseOutput()
    {
        for (int i = 0; i < size; i++)
        {
            yield return Inverse(i);
        }
    }

    IEnumerator<int> IEnumerable<int>.GetEnumerator()
    {
        for (int i = 0; i < size; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<int>)this).GetEnumerator();
    }

}
