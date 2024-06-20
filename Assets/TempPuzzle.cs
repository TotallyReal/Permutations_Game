using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempPuzzle : MonoBehaviour
{

    [SerializeField] private CogLink cogLink;
    [SerializeField] private LockSteps lockSteps;
    [SerializeField] private Gate gate;


    private void OnEnable()
    {
        cogLink.OnLinked += CogLink_OnLinked;
        cogLink.OnLinkStarted += CogLink_OnLinkStarted;
        lockSteps.Unlocked += OnUnlocked;
    }

    private void CogLink_OnLinkStarted(object sender, System.EventArgs e)
    {
        lockSteps.ResetLocks();
    }

    private void CogLink_OnLinked(object sender, (int, int) e)
    {
        if (e.Item1 == 0 && e.Item2 == 0)
            lockSteps.TryUnlock(0);
        else
            lockSteps.TryUnlock(1);
    }

    private void OnUnlocked(object sender, System.EventArgs e)
    {
        Debug.Log("Unlocked!");
        gate.TryOpen();
    }

}
