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

    internal int roomStep = 0;
    int roomLevel = 0;
    [SerializeField] DoorControl upDoor;
    [SerializeField] DoorControl downDoor;
    [SerializeField] DoorControl leftDoor;
    [SerializeField] DoorControl rightDoor;

    internal delegate void RoomAction(int level);
    internal RoomAction roomStart;

    private void Awake()
    {
        
    }
    internal void RoomStart()
    {
        DoorClose();
        roomStart?.Invoke(roomStep);
    }

    internal void SetRoomStep(int step)
    {
        roomStep = step;
    }
    internal void SetRoomLevel(int maxStep)
    {
       // roomStep = step;
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
        DoorOpen(true);
    }

    internal void RoomEnd()
    {
        DoorOpen(true);
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

    private void DoorOpen(bool showWay)
    {
        if (!upDoorBlock)
        {
            upDoor.Open(showWay);
        }
        if (!downDoorBlock)
        {
            downDoor.Open(showWay);
        }
        if (!leftDoorBlock)
        {
            leftDoor.Open(showWay);
        }

        if (!rightDoorBlock)
        {
            rightDoor.Open(showWay);
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
