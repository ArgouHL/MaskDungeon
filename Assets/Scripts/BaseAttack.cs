using UnityEngine;

public class BaseAttack : MonoBehaviour
{
    private SpriteRenderer sprite;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();

        Destroy(gameObject , 0.5f);
    }
    private void FixedUpdate()
    {
        var tempColor = sprite.color;
        tempColor.a -= 2f * Time.fixedDeltaTime;
        if (tempColor.a < 0)
            tempColor.a = 0;
        sprite.color = tempColor;
    }
}
