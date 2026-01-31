using UnityEngine;

public class MaskBehaviour : MonoBehaviour
{
    private float rotateSpeed = 75f;
    private float updownrange = 0.4f;
    private float updownspeed = 0.375f;
    private float originalY;
    private int type = 0; // 1: energy ball, 2: sector, 3:bomb, 4:rush

    void Start()
    {
        originalY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        float y = Mathf.Sin( 2 * Mathf.PI * Time.time * updownspeed);
        Vector3 pos = transform.position;
        pos.y = originalY + updownrange * 0.5f * (y + 1f);
        transform.position = pos;

        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }
    public void SetType(int newtype)
    {
        type = newtype;
    }

    public int GetType()
    {
        return type;
    }
}
