using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("�ж��]�w")]
    [SerializeField] Vector2Int roomSize = new Vector2Int(10, 10);
    [SerializeField] int targetMaxLength = 9;
    [SerializeField] int maxDeadLength = 3;

    [Header("�w�s��")]
    [SerializeField] GameObject startRoom;
    [SerializeField] GameObject gameRoom;
    [SerializeField] GameObject endRoom;

    private List<Vector2Int> roomSet = new List<Vector2Int>();
    private Dictionary<Vector2Int, Vector2Int> parentMap = new Dictionary<Vector2Int, Vector2Int>();

    // �x�s�C�Ӯy�й������Z��(�B��)
    private Dictionary<Vector2Int, int> distanceMap = new Dictionary<Vector2Int, int>();
    // �x�s�ͦ�������A��K�̫�������I
    private Dictionary<Vector2Int, GameObject> spawnedRoomObjects = new Dictionary<Vector2Int, GameObject>();
    private List<RoomControl> roomControls = new();
    private object step;

    private void Start()
    {
        GenRoom();
    }

    private void GenRoom()
    {
        // 1. ��l�ư_�I
        Vector2Int startPos = new Vector2Int(0, 0);
        roomSet.Add(startPos);
        distanceMap[startPos] = 0;
        spawnedRoomObjects[startPos] = CreateRoomObject(startRoom, startPos);
       int currentMaxDistance = 0;
        // 2. �ͦ��Ҧ��ж� (�]�t����)
        int attempts = 0;
        while (currentMaxDistance < targetMaxLength && attempts < 2000)
        {
            attempts++;
            Vector2Int branchParent = roomSet[Random.Range(0, roomSet.Count)];

            if (branchParent == startPos && GetNeighborCount(startPos) >= 1) continue;

            int deadLength = Random.Range(1, maxDeadLength + 1);
            Vector2Int currentHead = branchParent;

            for (int i = 0; i < deadLength; i++)
            {
                // �p�G�b�����L�{���w�g�F��ؼЪ��סA�i�H��ܰ���
                if (currentMaxDistance >= targetMaxLength) 
                {
                    FindObjectOfType<SpawnManager>().SpawnEnv();
                    break;
                }

                Way way = RandomWay();
                Vector2Int nextPos = GetNextCoordinate(currentHead, way);

                if (!roomSet.Contains(nextPos))
                {
                    roomSet.Add(nextPos);
                    parentMap[nextPos] = currentHead;

                    int newDist = distanceMap[currentHead] + 1;
                    distanceMap[nextPos] = newDist;

                    // ��s�ثe���a�Ϫ��̪��Z��
                    if (newDist > currentMaxDistance)
                    {
                        currentMaxDistance = newDist;
                    }

                    // �]�w�����s�q
                    spawnedRoomObjects[currentHead].GetComponent<RoomControl>().SetConnectDoor(way);
                    spawnedRoomObjects[nextPos] = CreateRoomObject(gameRoom, nextPos, way, newDist);

                    currentHead = nextPos;
                }
                else 
                { 
                    FindObjectOfType<SpawnManager>().SpawnEnv();
                    break;
                }
            }
        }

        // 3. ��X�̻����ж��é�m���I
        ReplaceFarthestWithEnd();

        
    }

    private void ReplaceFarthestWithEnd()
    {
        Vector2Int farthestPos = new Vector2Int(0, 0);
        int maxDist = -1;

        foreach (var pair in distanceMap)
        {
            if (pair.Value > maxDist)
            {
                maxDist = pair.Value;
                farthestPos = pair.Key;
            }
        }

        // ���Ӫ���ô���
        if (spawnedRoomObjects.ContainsKey(farthestPos))
        {
            Vector3 pos = spawnedRoomObjects[farthestPos].transform.position;
          spawnedRoomObjects[farthestPos].SetActive(false);
            var state = spawnedRoomObjects[farthestPos].GetComponent<RoomControl>().GetDoorStatus();
          GameObject endRoomGo= Instantiate(endRoom, pos, Quaternion.identity, transform);
            endRoomGo.GetComponent<RoomControl>().SetDoorStatus(state);
        }
    }

    private int GetNeighborCount(Vector2Int pos)
    {
        int count = 0;
        if (roomSet.Contains(pos + Vector2Int.up)) count++;
        if (roomSet.Contains(pos + Vector2Int.down)) count++;
        if (roomSet.Contains(pos + Vector2Int.left)) count++;
        if (roomSet.Contains(pos + Vector2Int.right)) count++;
        return count;
    }

    private Vector2Int GetNextCoordinate(Vector2Int current, Way way)
    {
        switch (way)
        {
            case Way.up: return current + Vector2Int.up;
            case Way.down: return current + Vector2Int.down;
            case Way.left: return current + Vector2Int.left;
            case Way.right: return current + Vector2Int.right;
            default: return current;
        }
    }

    private GameObject CreateRoomObject(GameObject prefab, Vector2Int coord)
    {
        Vector3 worldPos = new Vector3(coord.x * roomSize.x, 0, coord.y * roomSize.y);
        GameObject go = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        go.name = $"Room_{coord.x}_{coord.y}_Dist_{distanceMap[coord]}";
        roomControls.Add(go.GetComponent<RoomControl>());
        return go;
    }
    private GameObject CreateRoomObject(GameObject prefab, Vector2Int coord,Way way,int step)
    {
        GameObject go = CreateRoomObject(prefab, coord);
       go.GetComponent<RoomControl>().SetConnectDoor(ReverseWay(way));
        go.GetComponent<RoomControl>().SetRoomStep(step);
        return go;
    }

    private Way RandomWay()
    {
        return (Way)Random.Range(0, 4);
    }
    private Way ReverseWay(Way way)
    {
        Way rWay = Way.up;
        switch (way)
        {
            case Way.up: rWay= Way.down;break;
            default: case Way.down: rWay= Way.up; break;
                case Way.left:rWay= Way.right;break;
                case Way.right:
                rWay= Way.left;break;
                
               

                    
        }
        return rWay;
    }
    // �b Scene �����ХܨB�ƻP���u
    internal List<RoomControl> GetRoomList()
    {
        return roomControls;
    }

    private void OnDrawGizmos()
    {
        if (distanceMap == null) return;

        foreach (var pair in distanceMap)
        {
            Vector3 worldPos = new Vector3(pair.Key.x * roomSize.x, 2f, pair.Key.y * roomSize.y);

            // �ХܨB�ƼƦr (�b Scene �������)
#if UNITY_EDITOR
            UnityEditor.Handles.Label(worldPos, "Step: " + pair.Value.ToString());
#endif

            // �e�X�s���u
            if (parentMap.ContainsKey(pair.Key))
            {
              //  Gizmos.color = Color.Lerp(Color.yellow, Color.red, pair.Value / (float)totalRoomCount);
                Vector3 parentPos = new Vector3(parentMap[pair.Key].x * roomSize.x, 0.5f, parentMap[pair.Key].y * roomSize.y);
                Vector3 currentPos = new Vector3(pair.Key.x * roomSize.x, 0.5f, pair.Key.y * roomSize.y);
                Gizmos.DrawLine(parentPos, currentPos);
                Gizmos.DrawSphere(currentPos, 0.2f);
            }
        }
    }
}

public enum Way { up, down, left, right }
