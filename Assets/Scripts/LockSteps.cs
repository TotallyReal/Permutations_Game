using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockSteps : MonoBehaviour
{

    [SerializeField] private LockFrag[] locks;
    private int lockIndex = 0;
    private bool isLocked = true;

    private void Start()
    {
        ResetLocks();
    }

    public event EventHandler Unlocked;
    public event EventHandler LocksReset;

    public void ResetLocks()
    {
        foreach (LockFrag singleLock in locks)
        {
            singleLock.gameObject.SetActive(true);
        }
        lockIndex = 0;
        isLocked = true;
        LocksReset?.Invoke(this, EventArgs.Empty);
    }

    public bool TryUnlock(int lockType)
    {
        if (!isLocked)
        {
            return false; 
        }
        
        if (lockIndex >= locks.Length || locks[lockIndex].lockType != lockType)
        {
            ResetLocks();
            return false;
        }

        locks[lockIndex].gameObject.SetActive(false);
        lockIndex++;
        if (lockIndex == locks.Length)
        {
            Unlocked?.Invoke(this, EventArgs.Empty);
            isLocked = false;
        }
        return true;         
    }


}
