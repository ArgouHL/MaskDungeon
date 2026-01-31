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
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private AttackPattern[] attackPatterns;


    private bool isAttacking = false;
    private bool isAimming = false;
    private GameObject currentAimEffect;

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, Player.position);

        if (distance <= attackRange && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }

        // 瞄準階段平滑旋轉看向玩家
        if (isAttacking && !isAimming)
        {
            Vector3 direction = (Player.position - transform.position).normalized;
            direction.y = 0; // 保持在水平面上

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        // 0~1秒：瞄準階段
        yield return new WaitForSeconds(1f);

        isAimming = true;
        // 第1秒：生成瞄準特效
        currentAimEffect = Instantiate(attackPatterns[typeID].aimPrefab, transform);
        //currentAimEffect.transform.localPosition = attackPatterns[typeID].point;

        // 1~2秒：準備攻擊階段
        yield return new WaitForSeconds(1f);

        // 第2秒：生成攻擊
        if (typeID < attackPatterns.Length)
        {
            GameObject atk = Instantiate(attackPatterns[typeID].atkPrefab, transform);
            atk.transform.localPosition = attackPatterns[typeID].point;

            // 設定攻擊來源為敵人
            SetAttackSource(atk, "Enemy");
        }

        yield return new WaitForSeconds(attackPatterns[typeID].atkTime);

        isAttacking = false;
        isAimming = false;
    }

    private void SetAttackSource(GameObject attackObject, string source)
    {
        CheckAttack checkAttack = attackObject.GetComponent<CheckAttack>();
        if (checkAttack == null)
        {
            checkAttack = attackObject.transform.GetChild(0).GetComponent<CheckAttack>();
        }
        if (checkAttack != null)
            checkAttack.attackSource = source;
    }

    public int GetTypeID ()
    {
        return typeID;
    }
}
