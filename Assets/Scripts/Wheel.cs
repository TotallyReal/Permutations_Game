using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    [SerializeField] private float secPerStep = 0.5f;
	[SerializeField] private int steps = 3;
	[SerializeField] private WheelPulse pulse;
    [SerializeField] bool everyStep = false;

    Coroutine pulseCoroutine = null;

    private void Start()
    {
        StartPulse();
    }

    public void StartPulse()
    {
        if (pulseCoroutine == null)
        {
            pulseCoroutine = StartCoroutine(WheelManager.Instance.AddPulse((count) => { pulse.Pulse(); return true; }, secPerStep * (everyStep?1:steps), -1));
        }
    }
    public void OnEnable()
    {
        StartPulse();
    }

    public void OnDisable()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
    }

    private void FixedUpdate(){
		
		float t = WheelManager.Time() / (secPerStep * steps);
        transform.localRotation = Quaternion.Euler(0, 0, - t * 360 );
	}    

}
