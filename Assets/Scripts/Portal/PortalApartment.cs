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
    Permutation middleInput;
    Permutation apartmentInput;

    private void Start()
    {
        InitializeApartment(new Permutation(5), new Permutation(5));
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

    public event EventHandler<RoomInfo> OnRoomChanged;

    private void SetRoomIndex(int newRoomIndex)
    {
        roomIndex = Mathf.Max(newRoomIndex, 0); // should not be negative
        portalToRight.isActive = (roomIndex > 0);

        if (roomIndex > order)
            roomIndex -= order;

        portalToLeft.isActive = !((order == 6) && (roomIndex == 5));

        Debug.Log($"In room {roomIndex}");
    }

    public event EventHandler<RoomInfo> OnApartmentMorphed;

    private void OnPermutationChanged(object sender, Permutation permutation)
    {
        middleRoom.JoinGems(1, () =>
        {
            this.permutation = permutation;
            InitializeApartment(middleInput, permutation);
        });
    }

    private void InitializeApartment(Permutation input, Permutation permutation)
    {
        this.permutation = permutation;
        this.middleInput = input;
        this.apartmentInput = input;
        order = permutation.Order();

        leftPipes.UpdatePermutation(permutation);
        rightPipes.UpdatePermutation(permutation);

        SetRoomIndex(0);

        RoomInfo roomInfo = CreateInfo();

        leftPipes.CreateFrom(roomInfo, PortalSide.LEFT);
        middlePipes.CreateFrom(roomInfo, PortalSide.MIDDLE);
        rightPipes.CreateFrom(roomInfo, PortalSide.RIGHT);

        leftRoom.CreateFrom(roomInfo, PortalSide.LEFT);
        middleRoom.CreateFrom(roomInfo, PortalSide.MIDDLE);
        rightRoom.CreateFrom(roomInfo, PortalSide.RIGHT);

        OnApartmentMorphed?.Invoke(this, roomInfo);
    }

    public struct RoomInfo
    {
        public Permutation permutation;
        public Permutation middleInput; 
        public Permutation apartmentInput;
        public int order;
        public int roomIndex;
        public bool finishRoom;
    }

    private RoomInfo CreateInfo()
    {
        return new RoomInfo()
        {
            permutation = permutation,
            order = order,
            roomIndex = roomIndex,
            finishRoom = (order == 6) && (roomIndex == 5),
            middleInput = middleInput,
            apartmentInput = apartmentInput
        };
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


            // TODO: right now the left and right rooms are determined by the middle one.
            // maybe later will make it more general.
            SetRoomIndex(roomIndex + 1);
            middleInput = permutation.Inverse() * middleInput;
            RoomInfo info = CreateInfo();

            leftPipes.CopyRoom(middlePipes);
            middlePipes.CopyRoom(rightPipes);
            rightPipes.CreateFrom(info, PortalSide.RIGHT);

            leftRoom.CopyRoom(middleRoom);
            middleRoom.CopyRoom(rightRoom);
            rightRoom.CreateFrom(info, PortalSide.RIGHT);

            OnRoomChanged?.Invoke(this, info);

            return;
        }
        if (sender == (object)portalToRight)
        {

            SetRoomIndex(roomIndex - 1);
            middleInput = permutation * middleInput;
            RoomInfo info = CreateInfo();

            rightPipes.CopyRoom(middlePipes);
            middlePipes.CopyRoom(leftPipes);
            leftPipes.CreateFrom(info, PortalSide.LEFT);

            rightRoom.CopyRoom(middleRoom);
            middleRoom.CopyRoom(leftRoom);
            leftRoom.CreateFrom(info, PortalSide.LEFT);

            OnRoomChanged?.Invoke(this, info);

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
