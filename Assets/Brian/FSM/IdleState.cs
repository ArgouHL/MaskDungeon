using UnityEngine;

public class IdleState : IState
{
    public void Enter(AIController controller)
    {
        controller.idleTimer = Random.Range(controller.idleMin, controller.idleMax);
        controller.anim.SetBool("isIdle", true);
        controller.agent.isStopped = true;
    }

    public void Update(AIController controller)
    {
        // Logic for idle state
        controller.idleTimer -= Time.deltaTime;
        if (controller.idleTimer <= 0f)
        {
            controller.ChangeState(new WalkState());
        }
    }

    public void Exit(AIController controller)
    {
        controller.anim.SetBool("isIdle", false);
    }
}