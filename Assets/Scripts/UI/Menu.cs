using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Menu : MonoBehaviour
{
    public static Menu instance { get; private set; }
    [SerializeField] GameObject winCV;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static bool gameStartBool = false;

    public GameObject MenuObj;
    public GameObject GameOverObj;

    private void OnEnable()
    {
        InputManager.instance.input.Player.Attack.performed += GameStart;
    }

    private void OnDisable()
    {
        InputManager.instance.input.Player.Attack.performed -= GameStart;

    }
    private void GameStart(InputAction.CallbackContext context)
    {
        if (MenuObj.activeSelf)
        {
            gameStartBool = true;
            MenuObj.SetActive(false);
        }
        else if (GameOverObj.activeSelf)
        {
            SceneManager.LoadScene(0);
        }
    }

    public void GameWin()
    {
        winCV.SetActive(true);
    }
}
