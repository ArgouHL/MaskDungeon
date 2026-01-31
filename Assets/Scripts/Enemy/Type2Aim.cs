using UnityEngine;
using DG.Tweening;

public class Type2Aim : MonoBehaviour
{
    public Transform AimObj;

    private void Start()
    {
        AimObj.DOScaleY(1, 1f).SetEase(Ease.Linear).OnComplete(() => Destroy(gameObject));
    }
}
