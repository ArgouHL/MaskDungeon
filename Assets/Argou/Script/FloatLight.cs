using UnityEngine;

public class FloatLight : MonoBehaviour
{
    Light light;
     float minIntense =6;
     float maxIntense=8;
    float time = 0;
    float speed = 3;
    private void Awake()
    {
        light = GetComponent<Light>();
        time = Random.Range(-1f, 1f);
    }

    private void Update()
    {
        time += Time.deltaTime* speed;
        light.intensity = (Mathf.Cos(time)+ Mathf.Cos(time*3.5f) + Mathf.Cos(time * 2.6f))/3*(maxIntense-minIntense)+ minIntense;
    }
}
