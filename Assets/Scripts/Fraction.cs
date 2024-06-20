using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// TODO: consider making this class into a struct
[Serializable]
public class Fraction
{

    #region ----------------- static methods -----------------

    // Assumption: gcd(a,n) = 1
    public static int InverseMod(int a, int n)
    {
        if (n == 1)
        {
            return 1;
        }
        // TODO: Use gcd for faster computation
        for (int i = 0; i < n; i++)
        {
            int d = (a * i) % n;
            if (d == 1)
                return i;
            if (d == -1)
                return -i;
        }
        throw new Exception($"gcd({a},{n})!=1");
    }

    /// <summary>
    /// Returns (a,b,d) such that d=gcd(n,m) and a*n + b*m = d
    /// </summary>
    /// <param name="n"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    public static (int, int, int) GCDCoef(int n, int m)
    {
        // assume n >= m >= 0:
        if (m == 0)
        {
            return (1, 0, n);
        }
        
        int q = n / m;
        int r = n - q * m;
        int a, b, d;
        (a, b, d) = GCDCoef(m, r);
        return (b, a - q, d);

    }

    public static int GCD(int n, int m)
    {
        n = Mathf.Abs(n);
        m = Mathf.Abs(m);
        return (m < n) ? InnerGCD(m, n) : InnerGCD(n, m);
    }

    // assume that n<m
    private static int InnerGCD(int n, int m)
    {
        if (n == 0) return m;
        return InnerGCD(m % n, n);
    }

    #endregion

    public int numerator;
    public int denominator;

    public Fraction(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
        }
        this.numerator = numerator;
        this.denominator = denominator;
    }

    public void Reduce()
    {
        int gcdValue = GCD(numerator, denominator);
        if (gcdValue > 0)
        {
            numerator /= gcdValue;
            denominator /= gcdValue;
        }
    }

    #region ----------------- unary actions -----------------

    public static Fraction operator +(Fraction a) => a;

    public static Fraction operator -(Fraction a) => new Fraction(-a.numerator, a.denominator);

    #endregion

    #region ----------------- binary actions -----------------

    // Addition operator

    public static Fraction operator +(Fraction a, Fraction b)
        => new Fraction(a.numerator * b.denominator + b.numerator * a.denominator, a.denominator * b.denominator);


    public static Fraction operator +(Fraction a, int b)
        => new Fraction(a.numerator + b * a.denominator, a.denominator);


    public static Fraction operator +(int a, Fraction b)
        => new Fraction(a * b.denominator + b.numerator, b.denominator);

    // Subtraction operator

    public static Fraction operator -(Fraction a, Fraction b)
        => a + (-b);

    public static Fraction operator -(Fraction a, int b)
        => a + (-b);

    public static Fraction operator -(int a, Fraction b)
        => a + (-b);

    // Multiplication operator

    public static Fraction operator *(Fraction a, Fraction b)
        => new Fraction(a.numerator * b.numerator, a.denominator * b.denominator);

    public static Fraction operator *(Fraction fraction, int value)
    {
        return new Fraction(fraction.numerator * value, fraction.denominator);
    }

    public static Fraction operator *(int value, Fraction fraction)
    {
        return new Fraction(fraction.numerator * value, fraction.denominator);
    }

    // Division operator

    public static Fraction operator /(Fraction a, Fraction b)
    {
        if (b.numerator == 0)
        {
            throw new DivideByZeroException();
        }
        return new Fraction(a.numerator * b.denominator, a.denominator * b.numerator);
    }

    public static Fraction operator /(Fraction fraction, int value)
    {
        return new Fraction(fraction.numerator, fraction.denominator * value);
    }

    public static Fraction operator /(int value, Fraction fraction)
    {
        return new Fraction(fraction.denominator * value, fraction.numerator); // reciprocal multiplication
    }


    #endregion

    #region ----------------- comparison -----------------

    public static bool operator ==(Fraction a, Fraction b)
    {
        return a.numerator * b.denominator - b.numerator * a.denominator == 0;
    }

    public static bool operator ==(Fraction a, int b)
    {
        return a.numerator - b * a.denominator == 0;
    }

    public static bool operator ==(int b, Fraction a)
    {
        return a.numerator - b * a.denominator == 0;
    }

    public static bool operator !=(Fraction a, Fraction b)
    {
        return a.numerator * b.denominator - b.numerator * a.denominator != 0;
    }

    public static bool operator !=(Fraction a, int b)
    {
        return a.numerator - b * a.denominator != 0;
    }

    public static bool operator !=(int b, Fraction a)
    {
        return a.numerator - b * a.denominator != 0;
    }

    #endregion

    #region ----------------- conversion -----------------

    public float Value()
    {
        // TODO: Do not divide by zero.
        return numerator/(float)denominator;
    }

    public static explicit operator float(Fraction fraction)
    {
        return fraction.Value();
    }

    public static implicit operator Fraction(int value)
    {
        return new Fraction(value, 1);
    }

    public bool TryGetInt(out int value)
    {
        if (numerator % denominator == 0)
        {
            value = numerator / denominator;
            return true;
        }
        value = 0;
        return false;
    }

    public override string ToString() => $"{numerator} / {denominator}";

    #endregion
}
