using System.Collections;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public Transform attackPoint;
    public GameObject attackEffect;

    private bool isAttacking = false;
    public bool IsAttacking => isAttacking;

    public void Attack()
    {
        if (isAttacking) return;

        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        GameObject atk = Instantiate(attackEffect, attackPoint);

        yield return new WaitForSeconds(0.5f);

        Destroy(atk);
        isAttacking = false;
    }
}
