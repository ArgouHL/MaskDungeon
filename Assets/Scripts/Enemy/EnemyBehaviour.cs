using System.Collections;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("目標")]
    public Transform Player;

    [Header("攻擊類型")]
    [SerializeField] private int typeID = 0;

    [Header("攻擊設置")]
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private AttackPattern[] attackPatterns;
    [SerializeField] private GameObject aimEffectPrefab;

    private bool isAttacking = false;
    private GameObject currentAimEffect;

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, Player.position);

        if (distance <= attackRange && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        // 0~1秒：瞄準階段
        yield return new WaitForSeconds(1f);

        // 第1秒：生成瞄準特效
        if (aimEffectPrefab != null)
        {
            currentAimEffect = Instantiate(aimEffectPrefab, transform);
        }

        // 1~2秒：準備攻擊階段
        yield return new WaitForSeconds(1f);

        // 第2秒：生成攻擊
        if (typeID < attackPatterns.Length)
        {
            GameObject atk = Instantiate(attackPatterns[typeID].atkPrefab, transform);
            atk.transform.localPosition = attackPatterns[typeID].point;
        }

        // 銷毀瞄準特效
        if (currentAimEffect != null)
        {
            Destroy(currentAimEffect);
        }

        isAttacking = false;
    }
}
