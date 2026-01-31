using UnityEngine;
using DG.Tweening;

public class SwordAttack : MonoBehaviour
{
    private void Start()
    {
        transform.DOLocalRotate(new Vector3(0, -50f, 0), 0.25f).OnComplete(() => Destroy(gameObject));
    }
}
