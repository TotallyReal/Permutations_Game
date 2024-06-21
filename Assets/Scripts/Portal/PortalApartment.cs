using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalApartment : MonoBehaviour
{

    [SerializeField] private PermutationPipes leftPipes;
    [SerializeField] private PermutationPipes middlePipes;
    [SerializeField] private PermutationPipes rightPipes;

    [SerializeField] private GemRoom leftRoom;
    [SerializeField] private GemRoom middleRoom;
    [SerializeField] private GemRoom rightRoom;

    [SerializeField] private Portal portalToLeft;
    [SerializeField] private Portal portalToRight;

    private int order;
    private int roomIndex;

    private void Start()
    {
        Permutation permutation = middlePipes.GetPermutation();
        roomIndex = 0;
        order = permutation.Order();
        middleRoom.ShowGems(1);
        AdjustGemRooms();
        AdjustPipeColors();
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
            if (order == 1)
            {
                leftRoom.ShowGems(1);
                rightRoom.ShowGems(1);
            } else
            {
                leftRoom.ShowGems(2);
                rightRoom.ShowGems(2);
            }
        } else
        {
            if (roomIndex > 0)
            {
                middleRoom.ShowGems(roomIndex % order + 1);
                rightRoom.ShowGems((roomIndex + 1) % order + 1);
                leftRoom.ShowGems((roomIndex - 1) % order + 1);
            }
            else
            {
                int leftRoomIndex = -roomIndex;
                middleRoom.ShowGems(leftRoomIndex % order + 1);
                rightRoom.ShowGems((leftRoomIndex - 1) % order + 1);
                leftRoom.ShowGems((leftRoomIndex + 1) % order + 1);
            }
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

    private void OnPortal(object sender, Vector3 v)
    {
        if (sender == (object)portalToLeft)
        {
            middlePipes.SetLeftColors(rightPipes.GetLeftColors());
            AdjustPipeColors();
            middleRoom.ShowGems(rightRoom.numOfGems);
            roomIndex = (roomIndex + 1);
            if (roomIndex > order)
                roomIndex -= order;
            AdjustGemRooms();
            Debug.Log($"In room {roomIndex}");

            if (order == 6 && roomIndex == 5)
            {
                portalToLeft.Deactivate();
            }

            return;
        }
        if (sender == (object)portalToRight)
        {
            middlePipes.SetLeftColors(leftPipes.GetLeftColors());
            AdjustPipeColors();
            middleRoom.ShowGems(leftRoom.numOfGems);
            roomIndex = (roomIndex - 1);
            if (roomIndex < -order)
                roomIndex += order;
            AdjustGemRooms();
            Debug.Log($"In room {roomIndex}");
            return;
        }
    }

    private void OnPermutationChanged(object sender, Permutation permutation)
    {
        middleRoom.JoinGems(1, () =>
        {
            roomIndex = 0;
            order = permutation.Order();
            middleRoom.ShowGems(1);
            leftPipes.UpdatePermutation(permutation);
            rightPipes.UpdatePermutation(permutation);
            AdjustGemRooms();
            AdjustPipeColors();
        });
    }
}
