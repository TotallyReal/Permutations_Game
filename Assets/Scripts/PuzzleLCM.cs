using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PuzzleLCM : MonoBehaviour
{

    [SerializeField] private Cog leftCog;
    [SerializeField] private Cog rightCog;
    [SerializeField] private Transform[] locks;
    [SerializeField] private float secPerLock = 0.3f;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private MovingGate gate;
    [SerializeField] private TextMeshProUGUI puzzleName;

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
                //Debug.Log("1. Before stopping");
                StopLockMechanism();
                //Debug.Log("2. Before restarting");
                leftCog.SetNumberOfTeeth(leftCog.GetNumerOfTeeth() + 1);
                StartLockMechanism();
                //Debug.Log("4. Restarted");
            }
            if (cog == rightCog)
            {
                StopLockMechanism();
                rightCog.SetNumberOfTeeth(rightCog.GetNumerOfTeeth() + 1);
                StartLockMechanism();
            }
        }
    }

    private bool resetLockWait = true;

    private void ResetLocks()
    {
        if (rightTween != null && rightTween.IsActive())
        {
            rightTween.Kill();
        }
        if (leftTween != null && leftTween.IsActive())
        {
            leftTween.Kill();
        }
        leftCog.SetAngle(0);
        rightCog.SetAngle(0);
        resetLockWait = true;
        foreach (Transform singleLock in locks)
        {
            singleLock.gameObject.SetActive(true);
        }
        lockIndex = 0;
    }

    private int coroutineID = 0;

    private void StartLockMechanism()
    {
        if (lockMechanismCoroutine == null)
        {
            ResetLocks();
            //Debug.Log("3. Reseted locks");
            lockMechanismCoroutine = StartCoroutine(CogManager.Instance.AddPulse2(UseLock, secPerLock, -1, coroutineID++));
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

    private DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> leftTween = null, rightTween = null;

    private bool UseLock(int count, int id)
    {
        //Debug.Log($"In Use Lock... id = {id}");
        float discreteLeftAngle = Mathf.RoundToInt(leftCog.GetNumerOfTeeth() * (leftCog.GetAngle() / 360));
        float discreteRightAngle = Mathf.RoundToInt(rightCog.GetNumerOfTeeth() * (rightCog.GetAngle() / 360));
        //Debug.Log($"discreteLeftAngle = {discreteLeftAngle}");
        if (discreteLeftAngle == 0 && discreteRightAngle == 0)
        {
            particles.Play();
            if (resetLockWait)
            {
                resetLockWait = false;
                return true;
            }
            if (lockIndex == locks.Length)
            {
                StopLockMechanism();
                gate.TryOpen();
                puzzleName.gameObject.SetActive(true);
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

        leftTween = DOTween.To(leftCog.GetAngle, leftCog.SetAngle, (discreteLeftAngle+1) * 360 / leftCog.GetNumerOfTeeth(), secPerLock * 0.75f);

        

        rightTween = DOTween.To(rightCog.GetAngle, rightCog.SetAngle, (discreteRightAngle+1) * 360 / rightCog.GetNumerOfTeeth(), secPerLock * 0.75f);
        return true;
    }
}
