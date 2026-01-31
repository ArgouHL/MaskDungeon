using UnityEngine;

public class AIController : MonoBehaviour
{
    public Animator anim;
    public UnityEngine.AI.NavMeshAgent agent;
    public Transform player;
    public float patrolRadius = 8f;
    public float attackDistance = 2.5f;
    public float reachedDistance = 0.6f;
    public float idleMin = 2f;
    public float idleMax = 5f;

    public float idleTimer;
    private IState currentState;

    Vector3 patrolDestination;

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
        ChangeState(new WalkState());
    }

    void Update()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            //if not already in AttackState, switch to it
            if (!(currentState is AttackState))
            {
                ChangeState(new AttackState());
            }
        }
        else
        {
            //if in AttackState, switch to WalkState
            if (currentState is AttackState)
            {
                ChangeState(new WalkState());
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
        Vector3 randomPoint;
        if (RandomPointOnNavMesh(transform.position, patrolRadius, out randomPoint))
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
            // fallback: stay in place
            patrolDestination = transform.position;
            if (agent != null) agent.ResetPath();
        }
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

    //====Gizmos====

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}