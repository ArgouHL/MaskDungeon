using DG.Tweening;
using UnityEngine;

public class Type3Aim : MonoBehaviour
{
    public Transform AimObj;

    private void Start()
    {
        AimObj.DOScale(1, 1f).SetEase(Ease.Linear).OnComplete(() => Destroy(gameObject));
    }
}
