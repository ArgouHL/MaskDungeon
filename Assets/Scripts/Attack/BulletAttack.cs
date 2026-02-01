using UnityEngine;

public class BulletAttack : MonoBehaviour
{
    public float speed = 10f;
    public float destroyTime = 0.5f;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }
}
