using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingGate : MonoBehaviour
{

    [SerializeField] private float yOpenState = 0f;
    [SerializeField] private float duration = 1f;

    private bool isOpen = false;

    public void OpenGate()
    {
        if (!isOpen)
        {
            isOpen = true;
            transform.DOLocalMoveY(yOpenState, duration).SetEase(Ease.OutSine);
        }
        
    }

}
