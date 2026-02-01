using UnityEngine;
using System.Collections;


public class Bomb : MonoBehaviour
{
    private float radius = 3f;
    public GameObject prefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SetBoom());
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private IEnumerator SetBoom()
    {
        yield return new WaitForSeconds(3.5f);
        Visualize();
        yield return new WaitForSeconds(1.5f);
        float duration = 0.75f;
        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;
            t = 1f - Mathf.Pow(1f - t, 2f);
            float size = Mathf.Lerp(0.3f, 4.5f, t);
            transform.localScale = Vector3.one * size;
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one * 4.5f;

        yield return new WaitForSeconds(0.05f);
        Destroy(gameObject);
    }
    void Visualize()
    {
        
    }

}
