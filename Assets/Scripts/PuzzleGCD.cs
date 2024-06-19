using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class PuzzleGCD : MonoBehaviour
{

    [SerializeField] private Cog leftCog;
    [SerializeField] private Cog rightCog;
    [SerializeField] private float rotationPerSec = 1f;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private Transform[] locks;
    [SerializeField] private MovingGate gate;
    [SerializeField] private TextMeshProUGUI puzzleName;

    private Coroutine sparkCoroutine;
    private float initTime = 0;
    private int gcdValue = 0;

    private int GCD(int n, int m)
    {
        return (m < n) ? InnerGCD(m, n) : InnerGCD(n, m);
    }

    // assume that n<m
    private int InnerGCD(int n, int m)
    {
        if (n == 0) return m;
        return InnerGCD(m % n, n);
    }

    // Start is called before the first frame update
    void Start()
    {
        leftCog.SetAngle(-90);
        rightCog.SetAngle(90);
    }

    private void StartSpark()
    {
        if (sparkCoroutine == null)
        {
            gcdValue = GCD(rightCog.GetNumerOfTeeth(), leftCog.GetNumerOfTeeth());
            float angle = WheelManager.Time() * 360 * rotationPerSec;
            angle %= 360;
            angle /= 360;

            counter = Mathf.FloorToInt(angle * gcdValue); // TODO : fix counter 
            RelockAll();
            sparkCoroutine = StartCoroutine(WheelManager.Instance.AddPulse(Spark, 1 / (gcdValue * rotationPerSec), -1));
        }
    }

    private void StopSpark()
    {
        if (sparkCoroutine != null)
        {
            StopCoroutine(sparkCoroutine);
            sparkCoroutine = null;
        }
    }

    private void OnEnable()
    {
        RaycastSelector2D.Instance.OnObjectPressed += Instance_OnObjectPressed;
        lockIndex = 0;
        StartSpark();
    }

    private void OnDisable()
    {
        RaycastSelector2D.Instance.OnObjectPressed -= Instance_OnObjectPressed;
        StopSpark();
    }

    private void Instance_OnObjectPressed(object sender, Transform e)
    {
        if (e.TryGetComponent<Cog>(out Cog cog))
        {
            if (cog == leftCog)
            {
                StopSpark();
                leftCog.SetNumberOfTeeth(leftCog.GetNumerOfTeeth() + 1);
                StartSpark();
            }
            if (cog == rightCog)
            {
                StopSpark();
                rightCog.SetNumberOfTeeth(rightCog.GetNumerOfTeeth() + 1);
                StartSpark();
            }
        }
    }

    int counter = 0;
    int lockIndex = 0;

    private void RelockAll()
    {
        foreach (Transform singleLock in locks){ 
            singleLock.gameObject.SetActive(true);
        }
        lockIndex = 0;
    }

    private bool Spark(int sparkCount)
    {
        ParticleSystem.MainModule main = particles.main;
        if (counter == gcdValue)
        {
            counter = 1;
            main.startColor = Color.yellow;
            if (lockIndex == locks.Length - 1)
            {
                locks[lockIndex].gameObject.SetActive(false);
                StopSpark();
                gate.OpenGate();
                puzzleName.gameObject.SetActive(true);
            } else
            {
                RelockAll();
            }
        }
        else
        {
            main.startColor = Color.green;
            if (lockIndex == counter - 1 && lockIndex<locks.Length - 1)
            {
                locks[lockIndex].gameObject.SetActive(false);
                lockIndex++;
            }
            else
            {
                RelockAll();
            }
            counter++;
        }
        particles.Play();
        return true;
    }



    // Update is called once per frame
    void Update()
    {
        leftCog.SetAngle(- WheelManager.Time() * 360 * rotationPerSec - 90);
        rightCog.SetAngle(WheelManager.Time() * 360 * rotationPerSec + 90);
    }
}
