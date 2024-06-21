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

    [SerializeField] private string permutationString = "(1,2,3)(4,5)(6)";
    [SerializeField] private PermutationPipe pipePrefab;
    private PermutationPipe[] pipes;
    //[SerializeField] private PermutationLink linkPrefab;
    [SerializeField] private float heightDiff = 1;
    [SerializeField] private float widthLeft = 6;
    [SerializeField] private float widthPermutation = 4;
    [SerializeField] private float widthRight = 6;
    //[SerializeField] private LineRenderer linePrefab;

    [SerializeField] private bool isStatic = false;
    private Permutation permutation = new Permutation(5);
    //private LineRenderer[] lines;

    //private PermutationLink[] links;
    //private DragAndDrop[] targets;

    private void Awake()
    {
        int size = permutation.size;
        //lines = new LineRenderer[size];
        pipes = new PermutationPipe[size];
        for (int i = 0; i < size; i++)
        {
            pipes[i] = Instantiate(pipePrefab, transform);
            pipes[i].name = $"Pipe {i}";
            pipes[i].SetColor(colors[i]);
            pipes[i].SetDimensions(widthLeft, widthPermutation, widthRight, heightDiff);
            pipes[i].GetTarget().OnEndDragging += OnEndDragging;

            /*lines[i] = Instantiate(linePrefab, transform);
            lines[i].positionCount = 4;
            lines[i].startColor = colors[i];
            lines[i].endColor = colors[i];
            lines[i].useWorldSpace = false;*/
        }
        UpdateLinePositions();

        /*links = new PermutationLink[size];
        targets = new DragAndDrop[size];

        for (int i = 0; i < size; i++)
        {
            links[i] = Instantiate(linkPrefab, transform);
            links[i].transform.localPosition = new Vector3(widthLeft, 0, 0);
            links[i].linkWidth = widthPermutation;
            links[i].heightDiff = heightDiff;
            links[i].name = $"Link {i}";
            links[i].SetIDs(i, i);
            links[i].SetColor(lines[i].startColor);
            targets[i] = links[i].GetTarget();
            targets[i] = links[i].GetTarget();
            targets[i].OnEndDragging += OnEndDragging;
        }*/
    }

    #region ------------- permutation handling -------------

    public event EventHandler<Permutation> OnPermutationChanged;

    private void OnEndDragging(object sender, Vector2 position)
    {
        for (int i = 0; i < permutation.size; i++)
        {

            if ((object)pipes[i].GetTarget() == sender)
            {
                int targetId = pipes[i].targetId;
                
                Vector2 localPosition = transform.InverseTransformPoint(position);
                Debug.Log($"End dragging {sender} {i} at position {position}, local position {localPosition}");
                int yInt = Mathf.RoundToInt(localPosition.y/heightDiff);
                Vector2 targetCenter = new Vector2(widthLeft + widthPermutation, yInt * heightDiff);
                if (0 <= yInt && yInt < pipes.Length && 
                    (localPosition - targetCenter).magnitude < heightDiff/2 &&
                    !isStatic)
                {
                    int sigma = permutation.Inverse(yInt);
                    // i     => targetId
                    // sigma => yInt
                    //pipes[i].SetIDs(i, yInt);
                    //pipes[sigma].SetIDs(sigma, targetId);  // happens in the Update below
                    UpdatePermutation(Permutation.Cycle(permutation.size, targetId, yInt) * permutation);
                }
                else
                {
                    pipes[i].SetIDs(i, targetId);
                }
                return;
            }
        }
    }

    public Permutation GetPermutation()
    {
        return permutation;
    }

    public void UpdatePermutation(Permutation p)
    {
        if (p.size != permutation.size)
            return;

        permutation = p;

        UpdateLinePositions();

        for (int i = 0; i < permutation.size; i++)
        {
            pipes[i].SetIDs(i, p[i]);
        }

        OnPermutationChanged?.Invoke(this, p);

    }

    private void UpdateLinePositions()
    {
        for (int i = 0; i < pipes.Length; i++)
        {
            pipes[i].SetIDs(i, permutation[i]);
            /*lines[i].SetPositions(new Vector3[] {
                new Vector3(0, i * heightDiff, -1),
                new Vector3(widthLeft, i * heightDiff, -1),
                new Vector3(widthLeft + widthPermutation, permutation[i] * heightDiff, -1),
                new Vector3(widthLeft + widthPermutation + widthRight, permutation[i] * heightDiff, -1)
            });*/
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
            //lines[i].startColor = leftColors[i];
            //lines[i].endColor = leftColors[i];
            //links[i].SetColor(leftColors[i]);
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

    public void CopyRoom(PipesRoom toCopy)
    {
        //TODO: right now only copy color scheme
        SetLeftColors(toCopy.GetLeftColors());
    }
}

public class Permutation
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
}
