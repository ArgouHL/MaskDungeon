using UnityEngine;
using DG.Tweening;

public class BaseAttack : MonoBehaviour
{
    private SpriteRenderer sprite;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();

        sprite.DOFade(0, 0.5f).OnComplete( () => Destroy(gameObject));
    }
}
