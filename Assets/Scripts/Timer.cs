using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CogManager;

/// <summary>
/// Used to keep synchronized time for many objects, and emit pulses according to this timer.
/// </summary>
public class Timer : MonoBehaviour
{

    [SerializeField] private float t = 0;
    [Range(0, 2)]
    [Tooltip("This timer advances at 'rate'xstandard time")]
    [SerializeField] private float rate = 1f;
    [SerializeField] private bool pause;

    private void FixedUpdate()
    {
        if (!pause)
        {
            t += UnityEngine.Time.deltaTime * rate;
        }        
    }

    public float Time()
    {
        return t;
    }

    // Perform the pulse. Return true to continue;
    public delegate bool Pulse(int pulseCount);

    /// <summary>
    /// Sends a pulse at times 
    ///             phase + n * secPerPulse, 
    /// with the parameter n%period.
    /// </summary>
    public IEnumerator AddPeriodicPulse(Pulse pulse, int period, float secPerPulse, float phase)
    {
        int pulseCount = Mathf.FloorToInt((Time() - phase) / secPerPulse);
        int periodicPulseCount = pulseCount % period;
        if (periodicPulseCount < 0)
            periodicPulseCount += period;

        // While(true) is scary, I know. However you can stop this method both by stopping the coroutine,
        // and by returtning false in the pulse method.
        while (true)
        {
            //float t = Time();
            if (Time() > pulseCount * secPerPulse + phase)
            {
                //Debug.Log($"Passing the pulse {pulseCount}");
                if (periodicPulseCount == period)
                    periodicPulseCount = 0;
                if (!pulse(periodicPulseCount))
                    yield break;
                pulseCount += 1;
                periodicPulseCount += 1;
            }
            yield return null;
        }
    }
}
