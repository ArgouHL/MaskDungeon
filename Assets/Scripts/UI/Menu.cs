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

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            GameStart();
    }

    private void OnEnable()
    {
        InputManager.instance.input.Player.Attack.performed += AttackEvent;
    }

    private void OnDisable()
    {
        InputManager.instance.input.Player.Attack.performed -= AttackEvent;

    }
    private void AttackEvent(InputAction.CallbackContext context)
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

    void GameStart ()
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
