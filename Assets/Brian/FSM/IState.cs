using UnityEngine;

public interface IState
{
    void Enter(AIController controller);
    void Update(AIController controller);
    void Exit(AIController controller);
}