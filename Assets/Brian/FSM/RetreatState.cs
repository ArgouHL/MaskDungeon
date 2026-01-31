using UnityEngine;

public class RetreatState : IState
{
    public void Enter(AIController controller)
    {
        controller.anim.SetBool("isRetreating", true);
        // controller.agent.isStopped = false;
        controller.agent.isStopped = true;
        controller.agent.velocity = Vector3.zero;
    }

    public void Update(AIController controller)
    {
        Vector3 back = -controller.transform.forward;
        controller.agent.Move(back * controller.retreatSpeed * Time.deltaTime);
        Vector3 dir = controller.transform.position - controller.player.position; 
        dir.y = 0;
        if(dir.magnitude <= controller.retreatDistance * 1.5f)
        {
            controller.ChangeState(new ChaseState());
        }
    }

    public void Exit(AIController controller)
    {
        controller.anim.SetBool("isRetreating", false);
    }
}