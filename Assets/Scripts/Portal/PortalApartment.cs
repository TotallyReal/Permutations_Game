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

    // the rooms information
    private int order;
    private int roomIndex;
    Permutation permutation; // controlls order
    // Permutation input

    private void Start()
    {
        InitializeFromMiddleRoom(middlePipes.GetPermutation());
    }

    private void AdjustPipeColors()
    {
        leftPipes.SetRightColors(middlePipes.GetLeftColors());
        rightPipes.SetLeftColors(middlePipes.GetRightColors());
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

        //AdjustGemRooms();
        AdjustPipeColors();
        if ((order == 6) && (roomIndex == 5))
        {
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
            this.permutation = permutation;
            InitializeFromMiddleRoom(permutation);
        });
    }

    private void InitializeFromMiddleRoom(Permutation permutation)
    {
        order = permutation.Order();

        leftPipes.UpdatePermutation(permutation);
        rightPipes.UpdatePermutation(permutation);

        SetRoomIndex(0);
        leftRoom.ShowGems(0);
        middleRoom.ShowGems(1);
        rightRoom.ShowGems((order == 1) ? 1 : 2);
        OnApartmentMorphed?.Invoke(this, EventArgs.Empty);
    }

    public struct RoomInfo
    {
        public Permutation permutation;
        public int order;
        public int roomIndex;
        public bool finishRoom;
    }

    public enum PortalSide
    {
        LEFT = -1, 
        MIDDLE = 0, 
        RIGHT = 1
    }

    private void OnPortal(object sender, Vector3 v)
    {
        if (sender == (object)portalToLeft)
        {
            middlePipes.CopyRoom(rightPipes);


            // TODO: right now the left and right rooms are determined by the middle one.
            // maybe later will make it more general.
            SetRoomIndex(roomIndex + 1);
            RoomInfo info = new RoomInfo() { 
                permutation = permutation, order = order, roomIndex = roomIndex,
                finishRoom = (order == 6) && (roomIndex == 5)
            };

            leftRoom.CopyRoom(middleRoom);
            middleRoom.CopyRoom(rightRoom);
            rightRoom.CreateFrom(info, PortalSide.RIGHT);

            return;
        }
        if (sender == (object)portalToRight)
        {
            middlePipes.CopyRoom(leftPipes);

            SetRoomIndex(roomIndex - 1);
            RoomInfo info = new RoomInfo()
            {
                permutation = permutation, order = order, roomIndex = roomIndex,
                finishRoom = (order == 6) && (roomIndex == 5)
            };

            rightRoom.CopyRoom(middleRoom);
            middleRoom.CopyRoom(leftRoom);
            leftRoom.CreateFrom(info, PortalSide.LEFT);

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
