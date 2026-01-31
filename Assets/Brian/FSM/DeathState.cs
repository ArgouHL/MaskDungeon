using UnityEngine;

public class DeathState : IState
{
    public void Enter(AIController controller)
    {
        controller.anim.SetBool("isDead", true);
        controller.agent.isStopped = true;
        controller.agent.ResetPath();
        controller.agent.velocity = Vector3.zero;
        controller.GenerateMask();
    }

    public void Update(AIController controller)
    {

    }

    public void Exit(AIController controller)
    {
        controller.anim.SetBool("isDead", false);
    }
}