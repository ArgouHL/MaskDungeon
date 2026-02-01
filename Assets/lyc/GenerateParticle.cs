using UnityEngine;

public class GenerateParticle : MonoBehaviour
{
    public GameObject particleSystem;
    public float timer = 0f;
    void Start()
    {
        Destroy(gameObject, 0.5f);
    }
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 0.05f)
        {
            Destroy(Instantiate(particleSystem, transform.position, transform.rotation), 0.36f);
            timer -= 0.05f;
        }
    }
}
