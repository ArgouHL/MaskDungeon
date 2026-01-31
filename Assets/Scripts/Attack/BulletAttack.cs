using UnityEngine;

public class BulletAttack : MonoBehaviour
{
    private float speed = 10f;

    private void Start()
    {
        Destroy(gameObject, 0.5f);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }
}
