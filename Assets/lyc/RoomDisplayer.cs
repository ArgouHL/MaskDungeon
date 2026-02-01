using UnityEngine;
using Unity.AI.Navigation;
public class RoomDisplayer : MonoBehaviour
{
    Transform player;
    bool isDisplaying = true;
    private float maxDistance = 35f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dis = player.position - transform.position;
        dis.y = 0;
        if(!isDisplaying && dis.magnitude <= maxDistance)
        {
            isDisplaying = true;
            SetChildrenLightActive(isDisplaying);
        }
        if(isDisplaying && dis.magnitude > maxDistance)
        {
            isDisplaying = false;
            SetChildrenLightActive(isDisplaying);
        }
    }
    public void SetChildrenLightActive(bool active)
    {
        Light[] lights = GetComponentsInChildren<Light>(true);

        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].gameObject.SetActive(active);
        }
    }


}
