using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingGate : Gate
{

    [SerializeField] private float yOpenState = 0f;
    [SerializeField] private float duration = 1f;

    private bool isOpen = false;

    public override bool TryOpen()
    {
        if (!isOpen)
        {
            isOpen = true;
            transform.DOLocalMoveY(yOpenState, duration).SetEase(Ease.OutSine);
            
        }
        return true;
    }

    public override void Close()
    {
        
    }

}
