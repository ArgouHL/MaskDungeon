using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Range within which spawns can appear")]
    public float range = 5f;

    [System.Serializable]
    public class SpawnEntry
    {
        public string kind;
        public int number = 1;
    }

    [Tooltip("List of kinds to spawn and their counts")]
    public List<SpawnEntry> spawnList = new List<SpawnEntry>();

    // Ensure values stay valid when edited in inspector
    void OnValidate()
    {
        range = Mathf.Max(0f, range);
        if (spawnList != null)
        {
            for (int i = 0; i < spawnList.Count; i++)
            {
                if (spawnList[i] != null)
                    spawnList[i].number = Mathf.Max(0, spawnList[i].number);
            }
        }
    }

    void Start()
    {
    }

    void Update()
    {
    }

    // Draw range in the Scene view when selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0f, range));

        #if UNITY_EDITOR
        if (spawnList != null && spawnList.Count > 0)
        {
            Vector3 labelPos = transform.position + Vector3.up * 0.25f;
            string label = string.Join(", ", spawnList.Where(s => s != null).Select(s => string.Format("{0}:{1}", s.kind, s.number)));
            if (!string.IsNullOrEmpty(label))
                Handles.Label(labelPos, label);
        }
        #endif
    }
}
