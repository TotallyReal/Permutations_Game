using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLissajous : MonoBehaviour
{
    [SerializeField] private Cog leftCog;
    [SerializeField] private Cog rightCog;
    [SerializeField] private Transform gate;
    [SerializeField] private float rotPerSec = 1f;
    [SerializeField] private ParticleSystem particles;

    [SerializeField] private Transform bullet;
    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = bullet.GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        RaycastSelector2D.Instance.OnObjectPressed += Instance_OnObjectPressed;
    }

    private void OnDisable()
    {
        RaycastSelector2D.Instance.OnObjectPressed -= Instance_OnObjectPressed;
    }

    private bool bridgeRaised = false;

    private void Instance_OnObjectPressed(object sender, Transform e)
    {
        if (e.TryGetComponent<Cog>(out Cog cog))
        {
            if (cog == leftCog)
            {
                leftCog.SetNumberOfTeeth(leftCog.GetNumerOfTeeth() + 1);
            }
            if (cog == rightCog)
            {
                rightCog.SetNumberOfTeeth(rightCog.GetNumerOfTeeth() + 1);
            }
            if (!bridgeRaised && leftCog.GetNumerOfTeeth()==5 && rightCog.GetNumerOfTeeth() == 3)
            {
                bridgeRaised = true;
                particles.Play();
                gate.transform.DOLocalMoveY(-2, 2).SetEase(Ease.OutSine);
            }
        }
    }



    // Update is called once per frame
    void Update()
    {
        leftCog.SetAngle(CogManager.Time() * 360 * rotPerSec * leftCog.GetNumerOfTeeth());
        rightCog.SetAngle(CogManager.Time() * 360 * rotPerSec * rightCog.GetNumerOfTeeth());

        float rightAngle = rightCog.GetAngle() * Mathf.PI / 180;
        Vector3 rightCoords = rightCog.transform.position + new Vector3(-Mathf.Sin(rightAngle), Mathf.Cos(rightAngle), -5);
        float leftAngle = leftCog.GetAngle() * Mathf.PI / 180;
        Vector3 leftCoords = leftCog.transform.position + new Vector3(-Mathf.Sin(leftAngle), Mathf.Cos(leftAngle), -5);
        bullet.transform.position = new Vector3(rightCoords.x, leftCoords.y, -5);
        lineRenderer.SetPosition(0, leftCoords);
        lineRenderer.SetPosition(1, bullet.transform.position);
        lineRenderer.SetPosition(2, rightCoords);
    }
}
