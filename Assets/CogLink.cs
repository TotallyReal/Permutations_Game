using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CogLink : MonoBehaviour
{

    [Tooltip("Left or Up")]
    [SerializeField] private Cog cog1;
    [Tooltip("Right or Down")]
    [SerializeField] private Cog cog2;
    [SerializeField] private CogOrientation orientation;

    [SerializeField] private bool isOn = true;

    public enum CogOrientation
    {
        LEFT_RIGHT,
        UP_DOWN
    }

    private int gcdValue = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartLink();
    }

    public void ChangeCog1(Cog cog)
    {
        StopLink();
        cog1 = cog;
        StartLink();
    }

    public void ChangeCog2(Cog cog)
    {
        StopLink();
        cog2 = cog;
        StartLink();
    }


    #region ------------------- Link Event -------------------

    public UnityEngine.Events.UnityEvent<(int, int)> OnClick;
    public event Action MoreOnLinked;
    public event Action<int, bool> MoreOnLinked1;
    public event EventHandler<(int, int)> OnLinked;

    private Coroutine linkCoroutine = null;

    public bool IsActive()
    {
        return linkCoroutine != null;
    }

    public void RestartLink()
    {
        StopLink();
        StartLink();
    }

    public void StartLink()
    {
        if (linkCoroutine != null || cog1 == null || cog2 == null)
            return;

        if (cog1.GetNumerOfTeeth() == 0 || cog2.GetNumerOfTeeth() == 0)
            return;

        if (cog1.GetRotationsPerSec() == 0 || cog2.GetRotationsPerSec() == 0)
        {
            // no rotation, possibly one such k1 (resp. k2) solution
            // TODO: ignore this case for now, maybe add this static case later
            return;
        }
        if (cog1.GetRotationsPerSec() != -cog2.GetRotationsPerSec())
        {
            // a much more complicated case (both programmatically, and as a puzzle piece).
            // Maybe implement in the future
            return;
        }

        if (orientation == CogOrientation.LEFT_RIGHT)
        {
            // Check if the phases are reflections through the Y-axis, namely
            //        phase1-up = -(phase2-up)   (mod 360),
            // <=>    phase1+phase2 = 2*up (mod 360)
            int phaseCheck = (int)cog1.GetPhase() + (int)cog2.GetPhase() -2 * (int)Cog.Phase.UP;
            if ( phaseCheck % 360 != 0)
            {
                // TODO: ignoring this case for now, but possibly implement it in the future.
                return;
            }
            /**
            Let rps = rotationPerSec1 = -rotationPerSec1
            The following computation is not that hard once looking on the visualization, however,
            for completeness I add it here.
                
            looking for solution with t for the following equations:
                    phase1F + k1 / numTeeth1 + t * rps = PhaseF.Right + N
                    phase2F + k2 / numTeeth2 - t * rps = PhaseF.Left + M
                
            Taking the sum yields:
                    k1 / numTeeth1 + k2 / numTeeth2 = (PhaseF.Right+PhaseF.Left) - (phase1F+phase2F) + N + M
                                                    = (2*Phase.Up + K1) - (2*Phase.Up + K2) + N + M  
                                                    in integers
            If d = gcd(numTeeth1, numTeeth2), and we set L1 = numTeeth1/d, L2 = numTeeth2/d, then we want
                    k1 * L2 + k2 * L1 = N * d * L1 * L2
                
            Since L1 divides the right side, then it divides the left, and because gcd( L1 , L2 ) = 1, 
            we must have that L1 | k1. Similarly L2 | k2. 
            Writing them as k1 = a1 * L1, and k2 = a2 * L2 we have
                    (a1 + a2) * L1 * L2 = N * d * L1 * L2 
                
            Since we want k1 in [0,1,...,numTeeth1 - 1], then a1 in [0,1,...,d-1].
            The same holds for a2, so the solutions are exactly:
                    a1 = a2 = 0, and a2 = d - a1 for a1 in [1,2,...,d-1]

            Going back to the original problem, the solution for t is :
                    phase1F + a1 * L1 / (d * L1) + t * rps = PhaseF.Right + N
            So that
                    t = (PhaseF.Right - phase1F)/rps - (a1 - d*N)) / (d*rps)

            We need to have a pulse every 1/(d*rps) second, starting with (PhaseF.Right - phase1F)/rps.
            The k-th pulse correspond to (a1, d-a1) pair for a1 = k % d.
            **/

            int gcdValue = Fraction.GCD(cog1.GetNumerOfTeeth(), cog2.GetNumerOfTeeth());
            float rps = (float)cog1.GetRotationsPerSec();
            float phase = (float)(Cog.Phase.RIGHT.GetFraction() - cog1.GetPhase().GetFraction()) / rps;
            int sign = rps>0 ? 1 : -1;
            linkCoroutine = StartCoroutine(WheelManager.Instance.AddPeriodicPulse(
                Linked, gcdValue, sign / (gcdValue * rps), sign * phase));
        }
    }

    public void StopLink()
    {
        if (linkCoroutine != null)
        {
            StopCoroutine(linkCoroutine);
            linkCoroutine = null;
        }
    }

    private bool Linked(int count)
    {
        if (count == 0)
            OnLinked?.Invoke(this, (0, 0));
        else
            OnLinked?.Invoke(this, (count, gcdValue - count));
        return true;
    }

    #endregion


    #region ------------------- Maybe for the future ... -------------------

    private void StartLink2()
    {
        if (linkCoroutine == null)
        {
            if (orientation == CogOrientation.LEFT_RIGHT)
            {
                // phase1F + k1 / numTeeth1 + t * rotationPerSec1 = -1/4 + N
                // phase2F + k2 / numTeeth2 + t * rotationPerSec2 = +1/4 + M

                if (cog1.GetRotationsPerSec() == 0 || cog2.GetRotationsPerSec() == 0)
                {
                    // no rotation, possibly one such k1 (resp. k2) solution
                    // TODO: ignore this case for now, maybe add this static case later
                    return;
                }
                // t = -(phase1F + k1 / numTeeth1 + 1/4 - N)/rotationPerSec1  
                // t = -(phase2F + k2 / numTeeth2 - 1/4 - M)/rotationPerSec2  

                // (phase1F + k1/numTeeth1 + 1/4 - N)/rotationPerSec1 = (phase2F + k2/numTeeth2 - 1/4 - M)/rotationPerSec2
                // (phase1F + k1/numTeeth1 + 1/4)/rotationPerSec1 - (phase2F + k2/numTeeth2 - 1/4)/rotationPerSec2 = N/rotationPerSec1 - M/rotationPerSec2 
                // (phase1F + k1/numTeeth1 + 1/4) * rps2.num * rps1.denom - (phase2F + k2/numTeeth2 - 1/4) * rps2.denom * rps1.num =
                //                  N * rps2.num * rps1.denom - M * rps1.num * rps2.denom    in  gcd * Z
                Fraction rotPerSec1 = cog1.GetRotationsPerSec();
                Fraction rotPerSec2 = cog2.GetRotationsPerSec();
                int r1 = rotPerSec1.numerator * rotPerSec2.denominator;
                int r2 = rotPerSec2.numerator * rotPerSec1.denominator;
                //int gcdReduce = Fraction.GCD(d, rotPerSec1.denominator * rotPerSec2.denominator);

                var leftFraction = (cog1.GetPhase().GetFraction() + new Fraction(1, 4)) * r2;
                var rightFraction = (cog2.GetPhase().GetFraction() - new Fraction(1, 4)) * r1;
                var constantPart = leftFraction - rightFraction;

                int n1 = cog1.GetNumerOfTeeth();
                int n2 = cog2.GetNumerOfTeeth();
                var coef1 = r2 / n1;
                var coef2 = r1 / n2;

                int a, b, d;
                (a, b, d) = Fraction.GCDCoef(r2, r1);
                // when is
                //   constantPart + k1 * coef1 - k2 * coef2 = N * r2 - M * r1


                for (int k1 = 0; k1 < n1; k1++)
                {
                    for (int k2 = 0; k2 < n2; k1++)
                    {
                        Fraction fraction = constantPart + k1 * coef1 - k2 * coef2;
                        if (fraction.TryGetInt(out int value) && (value % d == 0))
                        {
                            int q = value / d;
                            int N = +q * r2; // + Z * r1/d
                            int M = -q * r1; // + Z * r2/d
                            // t = -(phase1F + k1 / numTeeth1 + 1/4 - (q * r2))*rps1.denom/rps1.num + Z * (r1/d) *rps1.denom/rps1.num
                            //   = -(phase1F + k1 / numTeeth1 + 1/4 - (q * r2))*rps1.denom/rps1.num + Z * (rps1.denom * rps2.denom) / d
                            // .... I can do this, but I don't think that I will ever use this.
                            // I will only implement for not the rotations at opposite directions.
                        }
                    }
                }
            }
        }
    }

    #endregion
}
