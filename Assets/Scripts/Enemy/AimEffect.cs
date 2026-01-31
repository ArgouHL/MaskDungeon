using UnityEngine;
using DG.Tweening;

public class AimEffect : MonoBehaviour
{
    public Transform AimObj;

    private void Start()
    {
        AimObj.DOScaleY(1, 1f).OnComplete(() => Destroy(gameObject));
    }
}
