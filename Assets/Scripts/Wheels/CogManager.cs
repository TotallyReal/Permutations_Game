using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof(Timer))]
public class CogManager : MonoBehaviour
{

    public static CogManager Instance;
    private float t = 0;

    private Timer timer;


    private void Awake()
    {
        Instance = this;
        timer = GetComponent<Timer>();
    }

    private void FixedUpdate()
    {
        t += UnityEngine.Time.deltaTime;
    }

    public static float Time()
    {
        return Instance.timer.Time();
    }

    // TODO: The pulse is aligned to the beginning of the program. 
    public IEnumerator AddPulse(Timer.Pulse pulse, float secPerPulse, int totalNumPulses)
    {
        int pulseCount = Mathf.FloorToInt(Time() / secPerPulse);
        while (totalNumPulses < 0 || pulseCount < totalNumPulses)
        {
            if (Time() > pulseCount * secPerPulse)
            {
                pulseCount += 1;
                if (!pulse(pulseCount))
                    yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Sends a pulse at times 
    ///             phase + n * secPerPulse, 
    /// with the parameter n%period.
    /// </summary>
    public IEnumerator AddPeriodicPulse(Timer.Pulse pulse, int period, float secPerPulse, float phase)
    {
        return timer.AddPeriodicPulse(pulse, period, secPerPulse, phase);
    }

    // Perform the pulse. Return true to continue;
    public delegate bool Pulse2(int pulseCount, int id);

    // TODO: The pulse is aligned to the beginning of the program. 
    public IEnumerator AddPulse2(Pulse2 pulse2, float secPerPulse, int totalNumPulses, int id)
    {
        int pulseCount = Mathf.FloorToInt(Time() / secPerPulse);
        while (totalNumPulses < 0 || pulseCount < totalNumPulses)
        {
            if (Time() > pulseCount * secPerPulse)
            {
                pulseCount += 1;
                if (!pulse2(pulseCount, id))
                    yield break;
            }
            yield return null;
        }
    }
}
