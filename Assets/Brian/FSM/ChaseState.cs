using UnityEngine;

public class ChaseState : IState
{
    public void Enter(AIController controller)
    {
        controller.anim.SetBool("isChasing", true);
        controller.agent.isStopped = false;
        controller.SetNewPatrolDestination();
    }

    public void Update(AIController controller)
    {
        if (!controller.agent.pathPending && controller.agent.remainingDistance <= controller.reachedDistance)
        {
            controller.ChangeState(new IdleState());
            controller.agent.ResetPath();
        }
    }

    public void Exit(AIController controller)
    {
        controller.anim.SetBool("isChasing", false);
    }
}