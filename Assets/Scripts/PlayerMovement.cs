using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    private Vector2 moveInput;
    private Vector3 moveDirection;

    AttackManager attackManager;

    private void Awake()
    {
        attackManager = GetComponent<AttackManager>();
    }

    void Update()
    {
        Move();
        Attack();
    }

    private void Move ()
    {
        if (attackManager.IsAttacking) return;

        moveInput = new Vector2(
            Keyboard.current.dKey.isPressed ? 1 : (Keyboard.current.aKey.isPressed ? -1 : 0),
            Keyboard.current.wKey.isPressed ? 1 : (Keyboard.current.sKey.isPressed ? -1 : 0)
        );

        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void Attack ()
    {
        if (Input.GetKeyDown (KeyCode.Space))
            attackManager.Attack();
    }
}
