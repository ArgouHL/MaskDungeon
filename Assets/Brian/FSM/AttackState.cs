using UnityEngine;

public class AttackState : IState
{
    public void Enter(AIController controller)
    {
        controller.anim.SetBool("isAttacking", true);
        controller.agent.isStopped = true;
        controller.agent.ResetPath();
        controller.agent.velocity = Vector3.zero;
        controller.enemyBehaviour.Attack();
        // TODO: ATTACK
    }

    public void Update(AIController controller)
    {
        if(controller.attackTimer < controller.attackCD)
        {
            controller.ChangeState(new ChaseState());
        }
    }

    public void Exit(AIController controller)
    {
        controller.anim.SetBool("isAttacking", false);
    }
}