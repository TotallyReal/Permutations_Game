using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class WheelManager : MonoBehaviour
{

    public static WheelManager Instance;
    private float t = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void FixedUpdate()
    {
        t += UnityEngine.Time.deltaTime;
    }

    public float Time()
    {
        return t; 
    }

    // Perform the pulse. Return true to continue;
    public delegate bool Pulse(int pulseCount);

    public IEnumerator AddPulse(Pulse pulse, float secPerPulse, int totalNumPulses)
    {
        int pulseCount = 0;
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
}
