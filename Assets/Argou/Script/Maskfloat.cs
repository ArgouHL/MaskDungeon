using UnityEngine;

public class Maskfloat : MonoBehaviour
{

    private float rotateSpeed = 75f;
    private float updownrange = 0.4f;
    private float updownspeed = 0.375f;
    private float originalY;


  

    // Update is called once per frame
    void Update()
    {
        float y = Mathf.Sin(2 * Mathf.PI * Time.time * updownspeed);
        Vector3 pos = transform.position;
        pos.y = originalY + updownrange * 0.5f * (y + 1f);
        transform.position = pos;

        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
       
    }
   
}
