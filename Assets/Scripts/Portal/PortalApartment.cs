using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalApartment : MonoBehaviour
{
    [Header("Pipe rooms")]
    [SerializeField] private PipesRoom leftPipes;
    [SerializeField] private PipesRoom middlePipes;
    [SerializeField] private PipesRoom rightPipes;

    [Header("Gem rooms")]
    [SerializeField] private GemRoom leftRoom;
    [SerializeField] private GemRoom middleRoom;
    [SerializeField] private GemRoom rightRoom;

    [Header("Portals")]
    [SerializeField] private Portal portalToLeft;
    [SerializeField] private Portal portalToRight;

    private int order;
    private int roomIndex;

    private void Start()
    {
        Permutation permutation = middlePipes.GetPermutation();
        order = permutation.Order();
        SetRoomIndex(0);
        OnApartmentMorphed?.Invoke(this, EventArgs.Empty);
    }

    private void AdjustPipeColors()
    {
        leftPipes.SetRightColors(middlePipes.GetLeftColors());
        rightPipes.SetLeftColors(middlePipes.GetRightColors());
    }

    private void AdjustGemRooms()
    {
        if (roomIndex == 0)
        {
            middleRoom.ShowGems(1);
            leftRoom.ShowGems(0);
            rightRoom.ShowGems((order == 1) ? 1 : 2);
        } else
        {
            leftRoom.ShowGems((roomIndex - 1) % order + 1);
            middleRoom.ShowGems(roomIndex % order + 1);
            rightRoom.ShowGems((roomIndex + 1) % order + 1);
        }
    }

    private void OnEnable()
    {
        portalToLeft.Portaled += OnPortal;
        portalToRight.Portaled += OnPortal;
        middlePipes.OnPermutationChanged += OnPermutationChanged;
    }

    private void OnDisable()
    {
        portalToLeft.Portaled -= OnPortal;
        portalToRight.Portaled -= OnPortal;
        middlePipes.OnPermutationChanged -= OnPermutationChanged;
    }

    public event EventHandler<int> OnRoomIndexChanged;

    private void SetRoomIndex(int newRoomIndex)
    {
        roomIndex = Mathf.Max(newRoomIndex, 0); // should not be negative
        portalToRight.isActive = (roomIndex > 0);

        if (roomIndex > order)
            roomIndex -= order;

        portalToLeft.isActive = !((order == 6) && (roomIndex == 5));

        AdjustGemRooms();
        AdjustPipeColors();
        if ((order == 6) && (roomIndex == 5))
        {
            rightRoom.ShowGems(6);
            rightPipes.UpdatePermutation(new Permutation(5));
        }

        Debug.Log($"In room {roomIndex}");
        OnRoomIndexChanged?.Invoke(this, roomIndex);
    }

    public event EventHandler OnApartmentMorphed;

    private void OnPermutationChanged(object sender, Permutation permutation)
    {
        middleRoom.JoinGems(1, () =>
        {
            order = permutation.Order();
            middleRoom.ShowGems(1);
            leftPipes.UpdatePermutation(permutation);
            rightPipes.UpdatePermutation(permutation);

            SetRoomIndex(0);
            OnApartmentMorphed?.Invoke(this, EventArgs.Empty);
        });
    }

    private void OnPortal(object sender, Vector3 v)
    {
        if (sender == (object)portalToLeft)
        {
            middlePipes.CopyRoom(rightPipes);
            middleRoom.CopyRoom(rightRoom);
            // TODO: right now the left and right rooms are determined by the middle one.
            // maybe later will make it more general.
            SetRoomIndex(roomIndex + 1);

            return;
        }
        if (sender == (object)portalToRight)
        {
            middlePipes.CopyRoom(leftPipes);
            middleRoom.CopyRoom(leftRoom);

            SetRoomIndex(roomIndex - 1);
            return;
        }
    }

    internal PipesRoom GetPipesRoom(int index)
    {
        if (index == 0)
            return middlePipes;
        if (index > 0)
            return rightPipes;
        else return leftPipes;
    }
}
