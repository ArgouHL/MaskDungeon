using UnityEngine;

public class AttackState : IState
{
    public void Enter(AIController controller)
    {
        controller.anim.SetBool("isAttacking", true);
        controller.agent.isStopped = true;
    }

    public void Update(AIController controller)
    {
        
    }

    public void Exit(AIController controller)
    {
        controller.anim.SetBool("isAttacking", false);
    }
}