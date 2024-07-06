using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;

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
            if (p.map[elements[i]] != elements[i])
                throw new Exception($"elements {elements} do not define a cycle");

            p.map[last] = elements[i];
            p.inverse[elements[i]] = last;
            last = elements[i];
        }

        p.map[last] = elements[0];
        p.inverse[elements[0]] = last;

        return p;
    }

    public static Permutation FromString(int size, string permString)
    {
        Assert.IsTrue(size >= 1);

        // Remove all whitespace from the input
        permString = Regex.Replace(permString, @"\s+", "");

        // Check if the input matches the expected format
        if (!Regex.IsMatch(permString, @"^(\(\d+(,\d+)*\))*$"))
        {
            throw new ArgumentException("Invalid permutation string format");
        }

        List<List<int>> cycles = new List<List<int>>();

        // Split the input into individual cycles
        string[] cycleStrings = permString.Split(')');

        // Remove the last empty string that results from splitting
        Array.Resize(ref cycleStrings, cycleStrings.Length - 1);

        Permutation p = new Permutation(size);
        foreach (string cycleString in cycleStrings)
        {
            // Remove the opening parenthesis and split by comma
            string[] elements = cycleString.Trim('(').Split(',');

            int[] cycleNumbers = cycleString.Trim('(').Split(',').Select(str => int.Parse(str)).ToArray();

            if (cycleNumbers.Any(i => (i < 0 || size <= i)))
            {
                throw new ArgumentException($"Invalid number in permutation string {permString} on {size} elements.");
            }
            p *= Permutation.Cycle(size, cycleNumbers);
        }

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
        for (int i = 0; i < size; i++)
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
            throw new Exception("Permutations must be over the same set permutationSize.");

        int size = a.size;
        Permutation p = new Permutation(size);
        for (int i = 0; i < size; i++)
        {
            p.map[i] = a.map[b.map[i]];
            p.inverse[p.map[i]] = i;
        }
        return p;
    }

    public static bool Equals(Permutation a, Permutation b)
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

    public static bool operator ==(Permutation a, Permutation b)
    {
        return Permutation.Equals(a, b);
    }

    public static bool operator !=(Permutation a, Permutation b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;
        return Permutation.Equals(this, obj as Permutation);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }


    /// <summary>
    /// Given a permutation sig:[n]->[n], returns a new permuttation tau:[permutationSize]->[permutationSize]
    /// such that tau[i+k] = sig[i]+k for i=0,...,n-1, and tau is the identity everywhere else
    /// </summary>
    /// <param name="p"></param>
    /// <param name="k"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public Permutation TranslateUp(int k, int size)
    {
        if (this.size + k > size)
        {
            throw new Exception("Cannot translate up the permutation");
        }
        Permutation tau = new Permutation(size);
        for (int i = 0; i < this.size; i++)
        {
            tau.map[i + k] = this[i] + k;
            tau.inverse[this[i] + k] = i + k;
        }
        return tau;
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

    public int CrossingNumber()
    {
        int number = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = i + 1; j < size; j++)
            {
                if (this[i] > this[j])
                    number++;
            }
        }
        return number;
    }

    /// <summary>
    /// Returns the permutation as a cycle decomposition string.
    /// The next cycle always starts with the smallest integer not appearing in a previous
    /// cycle. In particular, two equivalent permutations always have the same string output.
    /// 
    /// TODO: consider adding the size of the permutation.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Given an array [a_0,a_1,...,a_{n-1}], returns
    /// [a_{p(0)} , a_{p(1)} , ... , a_{p(n-1)}] .
    /// In other words, returns the right action map i->p(i)->a(p(i)).
    /// 
    /// As this is a right action, we have that 
    /// p.ApplyTo(q.Apply(arr)) = (q*p).ApplyTo(arr)
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <returns></returns>
    public T[] ApplyTo<T>(T[] arr)
    {
        return Output().Select(i => arr[i]).ToArray();
    }

    /// <summary>
    /// Given an array [a_0,a_1,...,a_{n-1}], returns
    /// [a_{p^-1(0)} , a_{p^-1(1)} , ... , a_{p^-1(n-1)}] .
    /// In other words, returns the left action map i->p^-1(i)->a(p^-1(i)).
    /// 
    /// As this is a left action, we have that 
    /// p.InvApplyTo(q.InvApplyTo(arr)) = (p*q).InvApplyTo(arr)
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <returns></returns>
    public T[] InvApplyTo<T>(T[] arr)
    {
        return InverseOutput().Select(i => arr[i]).ToArray();
    }

}
