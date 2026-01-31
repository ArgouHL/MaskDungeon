using UnityEngine;

public class ChaseState : IState
{
    private float detectTimer;
    public void Enter(AIController controller)
    {
        controller.agent.isStopped = false;
        detectTimer = 0f;
        controller.anim.SetBool("isChasing", true);
    }

    public void Update(AIController controller)
    {
        if(detectTimer > 0)
        {
            detectTimer -= Time.deltaTime;
        }
        Vector3 dir = controller.transform.position - controller.player.position; 
        dir.y = 0;
        if(dir.magnitude <= controller.retreatDistance)
        {
            controller.agent.updateRotation = true;
            controller.ChangeState(new RetreatState());
        }
        else if(dir.magnitude > controller.attackDistance)
        {
            controller.anim.SetBool("isWalking", true);

            controller.agent.isStopped = false;
            controller.agent.updateRotation = true;
            if(detectTimer <= 0)
            {
                controller.agent.SetDestination(controller.player.position);
                detectTimer = 0.5f;
            }
        }
        else
        {
            controller.anim.SetBool("isWalking", false);

            dir.Normalize();
            controller.agent.isStopped = true;
            if(Vector3.Angle( -dir,controller.transform.forward) > 5f)
            {
                Quaternion targetRot = Quaternion.LookRotation(-dir);
                controller.transform.rotation = Quaternion.RotateTowards(
                    controller.transform.rotation,
                    targetRot,
                    360f * Time.deltaTime
                );
            }
        }
        
    }

    public void Exit(AIController controller)
    {
        controller.agent.updateRotation = true;
        controller.anim.SetBool("isChasing", false);
        controller.anim.SetBool("isWalking", false);

    }
}