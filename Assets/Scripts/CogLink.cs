using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class CogLink : MonoBehaviour
{

    [Tooltip("Left or Up")]
    [SerializeField] private Cog cog1;
    [SerializeField] private Cog.Phase linkPhase1 = Cog.Phase.UP;
    [Tooltip("Right or Down")]
    [SerializeField] private Cog cog2;
    [SerializeField] private Cog.Phase linkPhase2 = Cog.Phase.UP;
    [SerializeField] private CogOrientation orientation;

    [SerializeField] private bool isOn = true;


    public enum CogOrientation
    {
        LEFT_RIGHT,
        UP_DOWN
    }


    // Start is called before the first frame update
    void Start()
    {
        StartSpecialLink();
    }

    private void OnEnable()
    {
        if (cog1 != null)
        {
            cog1.CogChanged += OnCogChanged;
        }
        if (cog2 != null)
        {
            cog2.CogChanged += OnCogChanged;
        }
    }

    private void OnDisable()
    {
        if (cog1 != null)
        {
            cog1.CogChanged -= OnCogChanged;
        }
        if (cog2 != null)
        {
            cog2.CogChanged -= OnCogChanged;
        }
    }

    private void OnCogChanged(object sender, EventArgs e)
    {
        RestartLink();
    }

    public void ChangeCog1(Cog cog)
    {
        StopLink();
        if (cog1 != null)
        {
            cog1.CogChanged -= OnCogChanged;
        }
        cog1 = cog;
        if (cog1 != null)
        {
            cog1.CogChanged += OnCogChanged;
        }
        StartSpecialLink();
    }

    public void ChangeCog2(Cog cog)
    {
        StopLink();
        if (cog2 != null)
        {
            cog2.CogChanged -= OnCogChanged;
        }
        cog2 = cog;
        if (cog2 != null)
        {
            cog2.CogChanged += OnCogChanged;
        }
        StartSpecialLink();
    }


    #region ------------------- Link Event -------------------

    //public UnityEngine.Events.UnityEvent<(int, int)> OnClick;
    //public event Action MoreOnLinked;
    //public event Action<int, bool> MoreOnLinked1;
    public event EventHandler<(int, int)> OnLinked;

    public event EventHandler OnLinkStarted;

    private Coroutine linkCoroutine = null;

    public bool IsActive()
    {
        return linkCoroutine != null;
    }

    [ContextMenu("Restart the link")]
    public void RestartLink()
    {
        StopLink();
        StartSpecialLink();
    }

    // both cogs (1) rotating clockwise
    //           (2) have the same phase difference
    //           (3) spark when both teeth point up
    private void StartSymmetricSpark(int teeth1, int teeth2, int phase_diff)
    {
        /**
        The following computation is not that hard once looking on the visualization, however,
        for completeness I add it here.
        
        The angles are considered in counterclockwise direction, namely rotating by positive angle
        means rotating counter clockwise. We also think of them mod 1 (instead of mod 360)
                   
        The teeth are arranged counter clock wise, so we ask if there are solutions to 
            phase + k1 / numTeeth1 - t = UP + N
            phase + k2 / numTeeth2 - t = UP + M
                
        Taking the difference yields:
            k1 / numTeeth1 - k2 / numTeeth2 = N - M = K   is an integer

        Given d = gcd(numTeeth1, numTeeth2), and setting L1 = numTeeth1/d, L2 = numTeeth2/d, we want
            k1 * L2 - k2 * L1 = K * d * L1 * L2
                
        Since L1 divides the right side, then it divides the left, and because gcd( L1 , L2 ) = 1, 
        we must have that L1 | k1. Similarly L2 | k2. 
        Writing them as k1 = a1 * L1, and k2 = a2 * L2 we have
            (a1 - a2) * L1 * L2 = K * d * L1 * L2 
         => (a1 - a2) = K * d
                
        Since we want k1 in [0,1,...,numTeeth1), then a1 in [0,1,...,d).
        The same holds for a2, so the solutions are exactly:
            a1 = a2 = 0, and a2 = d - a1 for a1 in [1,2,...,d-1]

        Going back to the original problem, the solution for t is :
                phase + a1 * L1 / (d * L1) - t = UP + N
        So that
                t = -(UP - phase) + (a1 - d*N)) / d

        We need to have a pulse every 1/d second, starting with -(UP - phase)=-phase_diff.
        The k-th pulse correspond to (a1, a1) pair for a1 = k % d.
        **/

        gcdValue = Fraction.GCD(teeth1, teeth2);
        L1 = teeth1 / gcdValue;
        L2 = teeth2 / gcdValue;
        float phaseDiff = phase_diff/(float)360;

        linkCoroutine = StartCoroutine(CogManager.Instance.AddPeriodicPulse(
            Linked, gcdValue, (float)1 / gcdValue, phaseDiff));
        OnLinkStarted?.Invoke(this, EventArgs.Empty);
    }

    private int gcdValue = 0;
    private int L1 = 0;
    private int L2 = 0;


    private int dir1 = 1;
    private int dir2 = 1;
    private int shift1 = 0;
    private int shift2 = 0;
    private int n1 = 0;
    private int n2 = 0;

    public void StartSpecialLink()
    {
        if (linkCoroutine != null || cog1 == null || cog2 == null)
            return;
        n1 = cog1.GetNumerOfTeeth();
        n2 = cog2.GetNumerOfTeeth();

        if (n1 == 0 || n2 == 0)
            return;

        dir1 = cog1.antiClockwiseDir;
        dir2 = cog2.antiClockwiseDir;

        Cog.Phase phase1 = cog1.GetPhase();
        Cog.Phase phase2 = cog2.GetPhase();

        int phase_diff1 = (int)linkPhase1 - (int)phase1;
        int phase_diff2 = (int)linkPhase2 - (int)phase2;

        if (dir2 == 1)
        {
            // reflect the second cog 
            phase_diff2 = -phase_diff2;
        }

        if (dir1 == 1)
        {
            // reflect the second cog 
            phase_diff1 = -phase_diff1;
        }

        if (phase_diff1 == phase_diff2)
        {
            StartSymmetricSpark(n1, n2, phase_diff1);
            return;
        }

        int diff = (phase_diff1 - phase_diff2 + 360) % 360;

        shift1 = 0;
        shift2 = 0;
        if (diff == 180)
        {
            if (n1 % 2 == 0)
            {
                StartSymmetricSpark(n1, n2, phase_diff2);
                shift1 = n1 / 2;
                return;
            }
            if (n2 % 2 == 0)
            {
                StartSymmetricSpark(n1, n2, phase_diff1);
                shift2 = n2 / 2;
                return;
            }

            return; // cogs cannot meet
        }

        // left with case (diff == 90,-90)
        if (n1 % 4 == 0)
        {
            shift1 = ((diff-90) % 360 == 0) ? n1 / 4 : -n1 / 4;
            StartSymmetricSpark(n1, n2, phase_diff2);
            return;
        }

        if (n2 % 4 == 0)
        {
            shift2 = ((diff+90) % 360 == 0) ? n2 / 4 : -n2 / 4;
            StartSymmetricSpark(n1, n2, phase_diff1);
            return;
        }

        // cogs will not meet

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
            The following computation is not that hard once looking on the visualization, however,
            for completeness I add it here.
                
            The dir1, dir2 are +-1 indicating whether the rotation is anti or clockwise.

            looking for solution with t for the following equations:
                    phase1F + k1 / numTeeth1 + t * dir1 = PhaseF.Right + N
                    phase2F + k2 / numTeeth2 - t * dir2 = PhaseF.Left + M
                
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
            /*float rps = (float)cog1.GetRotationsPerSec();
            float phase = (float)(Cog.Phase.RIGHT.GetFraction() - cog1.GetPhase().GetFraction()) / rps;
            int sign = rps>0 ? 1 : -1;
            linkCoroutine = StartCoroutine(CogManager.Instance.AddPeriodicPulse(
                Linked, gcdValue, sign / (gcdValue * rps), sign * phase));*/

            //float rps = (float)cog1.GetRotationsPerSec();
            int rotDir = cog1.antiClockwiseDir;
            float phase2 = (float)(Cog.Phase.RIGHT.GetFraction() - cog1.GetPhase().GetFraction()) / rotDir;
            linkCoroutine = StartCoroutine(CogManager.Instance.AddPeriodicPulse(
                Linked, gcdValue, (float)1 / gcdValue, rotDir * phase2));



            /**
            The following computation is not that hard once looking on the visualization, however,
            for completeness I add it here.
                
            The dir1, dir2 are +-1 indicating whether the rotation is anti or clockwise.

            looking for solution with t for the following equations:
                    phase1F + k1 / numTeeth1 + t * dir1 = PhaseF_L1 + N
                    phase2F + k2 / numTeeth2 + t * dir2 = PhaseF_L2 + M
            with k1 in [0,...,numTeeth1), k2 in [0,...,numTeeth2)

            Letting phase_diff_i = phase_iF-phase_iLF, we get that 
                    dir1 * phase_diff1 + dir1 * k1 / numTeeth1 + t = dir1 * N
                    dir2 * phase_diff2 + dir2 * k2 / numTeeth2 + t = dir2 * M
                
            Subtracting yields:
                    dir1 * phase_diff1 - dir2 * phase_diff2 + dir1 * k1 / numTeeth1 - dir2 * k2 / numTeeth2
                                    = dir1 * N - dir2 * M

            Let phase_arg = dir1 * phase_diff1 - dir2 * phase_diff2 = i/4 for some i.
            If d = gcd(numTeeth1, numTeeth2), and we set L1 = numTeeth1/d, L2 = numTeeth2/d, then we want
                    (i*d*L1*L2 + 4*dir1*k1*L2 - 4*dir2*k2*L1) = (dir1*N - dir2*M) * (4*L1*L2*d)

            Since L1 divides the right hand side, it divides the left one, and because gcd(L1,L2)=1 
            we must have that a1*L1 = 4*k1. Similarly a2*L2 = 4*k2.
            Using the gcd(L1,L2)=1 again, we see that 4 | a1 or 4 | a2 (possibly both, if L1,L2 are odd).
                    dir1*a1 - dir2*a2 = ((dir1*N - dir2*M) * 4 + i) * d

            We separate the problem to 3 cases according to i (which we assume is one of 0,1,2,3):
                1. If i = 0, we simply have
                        dir1*a1 - dir2*a2 = (dir1*N - dir2*M) * (4*d)
                   Since 4 | a1 or a2, it must divide both, and writing ai=4*bi we get
                        dir1*b1 - dir2*b2 = (dir1*N - dir2*M) * d   
                   and bi in [0,...,d).
                   It follows that mod d we have b2 = dir1*dir2*b1

                For the other two cases, since 4 divides the right hand side, it must divide the left hand side:
                2. If i = 2, we similarly get that 2 | a1,a2.
                      If both L1,L2 are odd, then both 4 | a1,a2, and we can write again ai=4*bi and get
                            dir1*2*b1 - dir2*2*b2 = ((dir1*N - dir2*M) * 2 + 1) * d
                      We must have that d = 2*dd and then
                            dir1*b1 - dir2*b2 = ((dir1*N - dir2*M) * 2 + 1) * dd
                      This time, for each choice of b1 in [0,d) we choose
                            b2 = dir1*dir2*b1 + {+-1, +-3} dd
                      So that b2 will remain in [0,d)
                      
                   and bi in [0,...,2d), and at least one of them is even.
                   If both L1,L2 are odd, then both 
                2. If i = 1,3, then 4 | d*L1*L2



            Going back to the original problem, the solution for t is :
                    dir1 * phase_diff1 + dir1 * a1 / 4 * d + t = dir1 * N

            So that
                    t = - dir1 * ( phase_diff1 + (a1 + 4*d*N) / (4*d) )



            We need to have a pulse every 1/(d*rps) second, starting with (PhaseF.Right - phase1F)/rps.
            The k-th pulse correspond to (a1, d-a1) pair for a1 = k % d.
            **/
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
        (int, int) teeth = (count == 0) ? (0, 0) : ((gcdValue - count) * L1, count * L2);
        teeth = (count * L1 + shift1, count * L2 + shift2);
        teeth = (teeth.Item1 * (-dir1), teeth.Item2 * (-dir2));
        teeth = ((teeth.Item1 % n1) + n1, (teeth.Item2 % n2) + n2);
        teeth = ((teeth.Item1 % n1), (teeth.Item2 % n2));

        //Debug.Log($"Teeth are {teeth}");
        OnLinked?.Invoke(this, teeth);
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
