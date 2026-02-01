using UnityEngine;
using DG.Tweening;

public class SwordAttack : MonoBehaviour
{
    public GameObject hitEffectPrefab;

    private void Start()
    {
        GameObject hitEffect = Instantiate(hitEffectPrefab);
        hitEffect.transform.position = transform.position;
        hitEffect.transform.rotation = transform.rotation* Quaternion.Euler(0, -45f, 0);
        Destroy(hitEffect, 1f);
        transform.DOLocalRotate(transform.rotation.eulerAngles + new Vector3(0, -100f, 0), 0.25f).OnComplete(() => Destroy(gameObject));
    }
}
