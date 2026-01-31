using System;
using Unity.VisualScripting;
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

    bool isBattle = false;

    private void Awake()
    {
        // Subscribe a handler that will notify the SpawnManager when this room starts.
        // We compute the room index at invocation time because the RoomGenerator
        // may add this RoomControl to its list after Awake runs.
        roomStart += HandleRoomStart;
    }

    void Update()
    {
        if(isBattle)
        {
            var spawnMgr = FindObjectOfType<SpawnManager>();
            if (spawnMgr == null)
            {
                Debug.LogWarning($"Update: SpawnManager not found for room '{gameObject.name}'");
                return;
            }

            if (spawnMgr.enemyCount <= 0)
            {
                isBattle = false;
                RoomEnd();
            }
        }  
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

    private void OnDestroy()
    {
        // Unsubscribe to avoid leaking delegates
        roomStart -= HandleRoomStart;
    }

    private void HandleRoomStart(int level)
    {
        if(isBattle) return;

        isBattle = true;
        // Find required managers
        var spawnMgr = FindObjectOfType<SpawnManager>();
        if (spawnMgr == null)
        {
            Debug.LogWarning($"HandleRoomStart: SpawnManager not found for room '{gameObject.name}'");
            return;
        }

        spawnMgr.SpawnEnemy(transform.position, level);
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

    // Expose the internal room step so other systems can react to room distance/step
    public int RoomStep => roomStep;

    internal void SetDoorStatus(bool[] status)
    {
        if (status[0]) SetConnectDoor(Way.up);
        if (status[1]) SetConnectDoor(Way.down);
        if (status[2]) SetConnectDoor(Way.left);
        if (status[3]) SetConnectDoor(Way.right);
    }

}
