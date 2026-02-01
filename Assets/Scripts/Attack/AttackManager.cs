using System.Collections;
using UnityEngine;

[System.Serializable]
public class AttackPattern
{
    public GameObject atkPrefab;
    public GameObject aimPrefab;
    public Vector3 point;
    public float atkTime;
    public float CD;
}

public class AttackManager : MonoBehaviour
{
    public AttackPattern [] attackPattern;

    private bool isAttacking = false;
    public bool IsAttacking => isAttacking;

    public void Attack(int index)
    {
        if (isAttacking) return;

        StartCoroutine(AttackCoroutine(index));
    }

    private IEnumerator AttackCoroutine(int index)
    {
        isAttacking = true;

        GameObject atk = Instantiate(attackPattern[index].atkPrefab , transform);
        atk.transform.localPosition = attackPattern[index].point;
        yield return new WaitForSeconds(attackPattern[index].atkTime);

        isAttacking = false;
    }
}
