using UnityEngine;

public class CheckAttack : MonoBehaviour
{
    private PlayerBehavior player;

    [Header("攻擊來源")]
    public string attackSource; // "Player" 或 "Enemy"

    [Header("碰撞設置")]
    [Tooltip("碰撞後是否立即銷毀物件")]
    public bool destroyOnHit = false;

    private bool hasHit = false;

    public void SetPlayer (PlayerBehavior player)
    {
        this.player = player;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // 玩家的攻擊打到敵人
        if (attackSource == "Player" && other.CompareTag("Enemy"))
        {
            Debug.Log("打到敵人");
            hasHit = true;

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }

            EnemyBehaviour enemy = other.GetComponent<EnemyBehaviour>();
            if (enemy != null)
            {
                int id = enemy.GetTypeID();
                player.attackType = id;
                Destroy(other.gameObject);
            }
        }
        // 敵人的攻擊打到玩家
        else if (attackSource == "Enemy" && other.CompareTag("Player"))
        {
            Debug.Log("打到玩家");
            hasHit = true;

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}
