using UnityEngine;

public class AIController : MonoBehaviour
{
    public Animator anim;
    public UnityEngine.AI.NavMeshAgent agent;
    public Transform player;
    public Transform eyePoint;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Patrol")]
    public float patrolRadius = 8f;
    Vector3 patrolDestination;

    [Header("Attack")]
    public float attackDistance = 2.5f;
    public float attackCD = 1.5f;
    public float attackRecover = 2f;
    public float attackTimer;

    [Header("Chase")]
    public float chaseDistance = 16f;
    public float chaseDegree = 105f;
    public float chaseTimer;
    public float chaseTime = 3f;


    [Header("Retreat")]
    public float retreatDistance = 1.8f;
    public float retreatSpeed = 3f;
    public float reachedDistance = 0.6f;
    [Header("Idle")]
    public float idleMin = 2f;
    public float idleMax = 5f;
    public float idleTimer;
    private IState currentState;

    [Header("Mob Basic Setting")]
    public int health = 10;
    public int damage = 1;
    public GameObject dropMask;
    public EnemyBehaviour enemyBehaviour;
    public Transform midpoint;


    public void SetLevel(int lvl)
    {
        float min = 0.8f + lvl * 0.01f;
        float max = 1f + lvl * 0.2f;

        float factor = Random.Range(min, max);

        health = Mathf.RoundToInt(health * factor);
        transform.localScale *= factor;
    }
    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        enemyBehaviour = GetComponent<EnemyBehaviour>();
        ChangeState(new IdleState());
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        RaycastHit hit;

        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 5f))
        {
            midpoint = hit.collider.transform;
        }
    }

    void Update()
    {
        if(currentState is DeathState)
        {
            return;
        }

        if(attackTimer >= 0)
        {
            attackTimer -= Time.deltaTime;
        }
        if (player == null)
        {
            if(currentState is not WalkState && currentState is not IdleState)
            {
                ChangeState(new IdleState());
            }
        }
        else
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0;
            float distance = direction.magnitude;
            bool inSight = distance <= chaseDistance && Vector3.Angle(direction, transform.forward) <= chaseDegree && HasLineOfSightToPlayer(direction, distance);
            if( currentState is WalkState || currentState is IdleState)
            {
                if(inSight)
                {
                    ChangeState(new ChaseState());
                    chaseTimer = 0;
                }
            }
            else
            {
                if(inSight){
                    chaseTimer = 0;
                    if(distance < attackDistance && attackTimer <= 0f && Vector3.Angle(direction, transform.forward) < 15f)
                    {
                        attackTimer = attackCD + attackRecover;
                        ChangeState(new AttackState());
                    }
                }
                else
                {
                    if (currentState is not AttackState)
                    {
                        chaseTimer += Time.deltaTime;
                        
                    }
                    if(chaseTimer >= chaseTime)
                    {
                        ChangeState(new IdleState());
                    }
                }
            }
        }

        currentState?.Update(this);
    }

    public void ChangeState(IState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    //====main AI functions====

    public void SetNewPatrolDestination()
    {
        float largerRadius = 10f;
        Vector3 randomPoint;

        if (RandomPointOnNavMeshInTwoRanges(
            transform.position,
            patrolRadius,
            midpoint.position,
            largerRadius,
            out randomPoint))
        {
            patrolDestination = randomPoint;

            if (agent != null)
            {
                agent.SetDestination(patrolDestination);
                Debug.DrawRay(patrolDestination, Vector3.up, Color.blue, 1.0f);
            }
        }
        else
        {
            patrolDestination = transform.position;
            if (agent != null) agent.ResetPath();
        }
    }

    bool RandomPointOnNavMeshInTwoRanges( Vector3 centerA, float radiusA, Vector3 centerB, float radiusB, out Vector3 result)
    {
        const int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 rnd = Random.insideUnitCircle * radiusB;
            Vector3 candidate = centerB + new Vector3(rnd.x, 0f, rnd.y);

            if ((candidate - centerA).sqrMagnitude > radiusA * radiusA)
                continue;

            if (RandomPointOnNavMesh(candidate, 0.5f, out result))
                return true;
        }

        result = Vector3.zero;
        return false;
    }


    bool RandomPointOnNavMesh(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * radius;
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 1.5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }

    bool HasLineOfSightToPlayer(Vector3 direction, float distance)
    {
        Vector3 origin = eyePoint != null ? eyePoint.position : transform.position + Vector3.up;

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance, obstacleMask))
        {
            return false;
        }

        return true;
    }

    //====Health System====

    public void Hurt(int damage)
    {
        if(currentState is DeathState)
        {
            return;
        }
        health -= damage;
        if(health <= 0)
        {
            ChangeState(new DeathState());
            agent.enabled = false;
            GetComponent<Collider>().enabled = false;
        }
    }

    public void GenerateMask()
    {
        if(dropMask != null)
        {
            FindObjectOfType<SpawnManager>().enemyCount--;
            enemyBehaviour.Death();
            Instantiate(dropMask, transform.position + Vector3.up * 0.5f, transform.rotation).GetComponent<MaskBehaviour>().SetType(enemyBehaviour.GetTypeID());
        }
    }

    //====Gizmos====

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}