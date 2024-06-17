using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WheelPulse : MonoBehaviour
{

    [SerializeField] private Ease easeType = Ease.OutSine;

    // Start is called before the first frame update
    public void Pulse()
    {
        transform.DOScale(transform.localScale.x*1.1f, 0.1f).SetLoops(2, LoopType.Yoyo).SetEase(easeType);
    }
}
