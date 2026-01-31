using System;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class RoomControl : MonoBehaviour
{

    bool upDoorBlock = true;
    bool downDoorBlock = true;
    bool leftDoorBlock = true;
    bool rightDoorBlock = true;

    int roomStep =0;

    [SerializeField] DoorControl upDoor;
    [SerializeField] DoorControl downDoor;
    [SerializeField] DoorControl leftDoor;
    [SerializeField] DoorControl rightDoor;

    internal delegate void RoomAction(int roomStep);
    internal RoomAction roomStart;

    private void Awake()
    {
        
    }
    internal void RoomStart()
    {
        DoorClose();
        roomStart?.Invoke(roomStep);
    }

    internal void SetConnectDoor(Way way)
    {
        switch (way)
        {
            case Way.up:
                upDoorBlock = false;
                break;
            case Way.down:
                downDoorBlock = false;
                break;
            case Way.left:
                leftDoorBlock = false;
                break;
            case Way.right:
                rightDoorBlock = false;
                break;
            default:
                break;
        }
        DoorOpen();
    }

    internal void RoomEnd()
    {
        DoorOpen();
    }
    private void DoorClose()
    {
       if(!upDoorBlock)
        {
            upDoor.Close();
        }
        if (!downDoorBlock)
        {
            downDoor.Close();
        }
        if (!leftDoorBlock)
        {
            leftDoor.Close();
        }

        if (!rightDoorBlock)
        {
            rightDoor.Close();
        }
    }

    private void DoorOpen()
    {
        if (!upDoorBlock)
        {
            upDoor.Open();
        }
        if (!downDoorBlock)
        {
            downDoor.Open();
        }
        if (!leftDoorBlock)
        {
            leftDoor.Open();
        }

        if (!rightDoorBlock)
        {
            rightDoor.Open();
        }
    }

    internal bool[] GetDoorStatus()
    {
      
        return new bool[] { !upDoorBlock, !downDoorBlock, !leftDoorBlock, !rightDoorBlock };
    }

    internal Vector3 GetRoomCentre()
    {
        return transform.position;
    }

    internal void SetDoorStatus(bool[] status)
    {
        if (status[0]) SetConnectDoor(Way.up);
        if (status[1]) SetConnectDoor(Way.down);
        if (status[2]) SetConnectDoor(Way.left);
        if (status[3]) SetConnectDoor(Way.right);
    }

}
