using UnityEngine;

public class InputManager : MonoBehaviour
{
    internal static InputManager instance;

    internal PlayerInput input;

    private void Awake()
    {
        instance = this;
        input = new PlayerInput();
        input.Player.Enable();
    }

    

}
