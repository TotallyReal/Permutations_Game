using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemRoom : MonoBehaviour
{

    [SerializeField] private Transform gemPrefab;
    [SerializeField] private float widthDiff = 0.5f;
    private Transform[] gems;
    public int numOfGems { get; private set; }
    // Start is called before the first frame update
    void Awake()
    {
        gems = new Transform[6];
        for (int i = 0; i < 6; i++)
        {
            gems[i] = Instantiate(gemPrefab, transform);
        }
    }


    public void ShowGems(int amount)
    {
        amount = Mathf.Clamp(amount, 0, 6);
        numOfGems = amount;
        float x0 = -(amount - 1) * widthDiff / 2;
        for (int i = 0; i < amount; i++)
        {
            gems[i].transform.localPosition = new Vector3(x0 + i*widthDiff, 0, 0);
            gems[i].gameObject.SetActive(true);
        }
        for (int i = amount; i < gems.Length; i++) {
            gems[i].gameObject.SetActive(false);
        }
    }

    public void CopyRoom(GemRoom toCopy)
    {
        ShowGems(toCopy.numOfGems);
    }

    public void JoinGems(float duration, TweenCallback onComplete)
    {
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < numOfGems; i++)
        {
            sequence.Join(gems[i].DOLocalMoveX(0, duration));
            sequence.Join(gems[i].DOScale(15,duration/2).SetLoops(2,LoopType.Yoyo).SetEase(Ease.InOutQuad));
        }
        sequence.OnComplete(onComplete);
    }

    internal void CreateFrom(PortalApartment.RoomInfo info, PortalApartment.PortalSide side)
    {
        ShowGems((info.roomIndex + (int)side) % info.order + 1);
        if (side == PortalApartment.PortalSide.RIGHT && info.finishRoom)
        {
            ShowGems(6);                
        }
    }
}
