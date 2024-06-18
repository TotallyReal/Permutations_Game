using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLCM : MonoBehaviour
{

    [SerializeField] private Cog leftCog;
    [SerializeField] private Cog rightCog;
    [SerializeField] private Transform[] locks;
    [SerializeField] private float secPerLock = 0.3f;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private Transform gate;

    private Coroutine lockMechanismCoroutine;

    private int lockIndex = 0;


    private void Start()
    {
        StartLockMechanism();
    }

    private void OnEnable()
    {
        RaycastSelector2D.Instance.OnObjectPressed += Instance_OnObjectPressed;
        ResetLocks();
        StartLockMechanism();
    }

    private void OnDisable()
    {
        RaycastSelector2D.Instance.OnObjectPressed -= Instance_OnObjectPressed;
        StopLockMechanism();
    }

    private void Instance_OnObjectPressed(object sender, Transform e)
    {
        if (e.TryGetComponent<Cog>(out Cog cog))
        {
            if (cog == leftCog)
            {
                StopLockMechanism();
                leftCog.SetNumberOfTeeth(leftCog.GetNumerOfTeeth() + 1);
                StartLockMechanism();
            }
            if (cog == rightCog)
            {
                StopLockMechanism();
                rightCog.SetNumberOfTeeth(rightCog.GetNumerOfTeeth() + 1);
                StartLockMechanism();
            }
        }
    }

    private void ResetLocks()
    {
        leftCog.SetAngle(0);
        rightCog.SetAngle(0);
        foreach (Transform singleLock in locks)
        {
            singleLock.gameObject.SetActive(true);
        }
        lockIndex = 0;
    }

    private void StartLockMechanism()
    {
        if (lockMechanismCoroutine == null)
        {
            ResetLocks();
            lockMechanismCoroutine = StartCoroutine(WheelManager.Instance.AddPulse(UseLock, secPerLock, -1));
        }
    }

    private void StopLockMechanism()
    {
        if (lockMechanismCoroutine != null)
        {
            StopCoroutine(lockMechanismCoroutine);
            lockMechanismCoroutine = null;
            //ResetLocks();
        }
    }

    private bool UseLock(int count)
    {
        float discreteLeftAngle = Mathf.RoundToInt(leftCog.GetNumerOfTeeth() * (leftCog.GetAngle() / 360));
        float discreteRightAngle = Mathf.RoundToInt(rightCog.GetNumerOfTeeth() * (rightCog.GetAngle() / 360));
        if (discreteLeftAngle == 0 && discreteRightAngle == 0)
        {
            particles.Play();
            if (lockIndex == locks.Length)
            {
                StopLockMechanism();
                gate.transform.DOLocalMoveY(-2, 1);
                return false;
            }
            if (lockIndex > 0)
            {
                ResetLocks();
                return true;
            }
        }

        if (lockIndex >= locks.Length)
        {
            ResetLocks();
            return true;
        }

        locks[lockIndex].gameObject.SetActive(false);
        lockIndex++;

        DOTween.To(leftCog.GetAngle, leftCog.SetAngle, (discreteLeftAngle+1) * 360 / leftCog.GetNumerOfTeeth(), secPerLock * 0.75f);

        DOTween.To(rightCog.GetAngle, rightCog.SetAngle, (discreteRightAngle+1) * 360 / rightCog.GetNumerOfTeeth(), secPerLock * 0.75f);
        return true;
    }
}
