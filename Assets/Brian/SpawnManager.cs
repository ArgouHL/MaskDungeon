using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Tooltip("Environment prefabs to use based on room step groups")]
    public GameObject[] roomEnv, roomEnemy;
    [Tooltip("Enemy prefabs (1-based kinds as strings in SpawnPoint.kind)")]
    public GameObject[] EnemyList;

    // Spawn environment prefabs for every generated room according to the RoomControl.RoomStep
    // roomStep == 0 -> skip
    // roomStep == 1 or 2 -> use roomEnv[0]
    // roomStep == 3 or 4 -> use roomEnv[1]
    // otherwise -> use roomEnv[2]
    public void SpawnEnv()
    {
        var roomGen = FindObjectOfType<RoomGenerator>();
        if (roomGen == null)
        {
            Debug.LogWarning("SpawnEnv: RoomGenerator not found in scene.");
            return;
        }

        var rooms = roomGen.GetRoomList();
        if (roomEnv == null || roomEnv.Length == 0)
        {
            Debug.LogWarning("SpawnEnv: roomEnv array is empty.");
            return;
        }

        foreach (var rc in rooms)
        {
            if (rc == null) continue;
            int step = rc.RoomStep;
            if (step == 0) continue;

            int idx = 2; // default
            if (step == 1 || step == 2) idx = 0;
            else if (step == 3 || step == 4) idx = 1;

            if (idx < 0 || idx >= roomEnv.Length) continue;

            Vector3 spawnPos = rc.GetRoomCentre();
            // Parent to the room GameObject for clarity
            Instantiate(roomEnv[idx], spawnPos, Quaternion.identity, rc.transform);
        }
    }

    // Spawn enemy-room prefabs and then spawn enemies inside them using attached SpawnPoint
    public void SpawnEnemy(Vector3 position, int number)
    {
        var roomGen = FindObjectOfType<RoomGenerator>();
        if (roomGen == null)
        {
            Debug.LogWarning("SpawnEnemy: RoomGenerator not found in scene.");
            return;
        }

        var rooms = roomGen.GetRoomList();
        if (roomEnemy == null || roomEnemy.Length == 0)
        {
            Debug.LogWarning("SpawnEnemy: roomEnemy array is empty.");
            return;
        }

        // Use the specific room at index `number` instead of iterating all rooms
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("SpawnEnemy: room list is empty.");
            return;
        }

        if (number < 0 || number >= rooms.Count)
        {
            Debug.LogWarning($"SpawnEnemy: requested room index {number} is out of range (0..{rooms.Count - 1}).");
            return;
        }

        var rc = rooms[number];
        if (rc == null)
        {
            Debug.LogWarning($"SpawnEnemy: room at index {number} is null.");
            return;
        }

        int step = rc.RoomStep;
        if (step == 0) return;

        int idx = 2; // default
        if (step == 1 || step == 2) idx = 0;
        else if (step == 3 || step == 4) idx = 1;

        if (idx < 0 || idx >= roomEnemy.Length) return;

        Vector3 spawnPos = rc.GetRoomCentre();
        GameObject spawnedRoomEnemy = Instantiate(roomEnemy[idx], spawnPos, Quaternion.identity, rc.transform);

        // Look for a SpawnPoint component in the spawned prefab's children
        SpawnPoint sp = spawnedRoomEnemy.GetComponentInChildren<SpawnPoint>();
        if (sp == null)
        {
            Debug.LogWarning($"SpawnEnemy: no SpawnPoint found in spawned prefab '{spawnedRoomEnemy.name}'");
            return;
        }

        // Use the spawnList on the SpawnPoint to decide which EnemyList prefab(s) to spawn and how many
        if (sp.spawnList == null) return;

        foreach (var entry in sp.spawnList)
        {
            if (entry == null) continue;
            if (string.IsNullOrWhiteSpace(entry.kind)) continue;

            // The user expects kind to be a 1-based index in string form. Try parse.
            if (!int.TryParse(entry.kind, out int kind1Based))
            {
                Debug.LogWarning($"SpawnEnemy: could not parse SpawnEntry.kind='{entry.kind}' to int. Skipping.");
                continue;
            }

            int enemyIndex = kind1Based - 1;
            if (enemyIndex < 0 || enemyIndex >= EnemyList.Length)
            {
                Debug.LogWarning($"SpawnEnemy: computed enemy index {enemyIndex} out of range for EnemyList.");
                continue;
            }

            GameObject enemyPrefab = EnemyList[enemyIndex];
            int spawnCount = Mathf.Max(0, entry.number);

            for (int i = 0; i < spawnCount; i++)
            {
                // Random position within the spawn point's range on XZ plane
                Vector2 r = Random.insideUnitCircle * Mathf.Max(0f, sp.range);
                Vector3 pos = sp.transform.position + new Vector3(r.x, 0f, r.y);
                Instantiate(enemyPrefab, pos, Quaternion.identity, spawnedRoomEnemy.transform);
            }
        }
    }
}
