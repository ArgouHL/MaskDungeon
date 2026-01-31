using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehavior : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    private Vector2 moveInput;
    private Vector3 moveDirection;
    private Quaternion targetRotation;
    private bool hasTargetRotation = false;

    AttackManager attackManager;
    CharacterController characterController;

    private void Awake()
    {
        attackManager = GetComponent<AttackManager>();
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Move();
        Attack();
    }

    private void Move()
    {
        // 攻擊時不能移動也不能旋轉
        if (attackManager.IsAttacking) return;

        // 檢測按鍵輸入（支持八方位）
        moveInput = new Vector2(
            Keyboard.current.dKey.isPressed ? 1 : (Keyboard.current.aKey.isPressed ? -1 : 0),
            Keyboard.current.wKey.isPressed ? 1 : (Keyboard.current.sKey.isPressed ? -1 : 0)
        );

        // 計算移動和旋轉方向（八方位）
        if (moveInput != Vector2.zero)
        {
            moveDirection = new Vector3(moveInput.x + moveInput.y, 0f, -moveInput.x + moveInput.y).normalized;

            // 檢測是否剛按下按鍵或沒有目標旋轉
            bool keyPressed = Keyboard.current.wKey.wasPressedThisFrame ||
                             Keyboard.current.sKey.wasPressedThisFrame ||
                             Keyboard.current.aKey.wasPressedThisFrame ||
                             Keyboard.current.dKey.wasPressedThisFrame;

            if (keyPressed || !hasTargetRotation)
            {
                targetRotation = Quaternion.LookRotation(moveDirection);
                hasTargetRotation = true;
            }
        }

        // 如果有目標旋轉，平滑旋轉到目標方向
        if (hasTargetRotation)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // 檢查是否已經到達目標旋轉
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                hasTargetRotation = false;
            }
        }

        // 如果有方向輸入，使用 CharacterController 進行移動
        if (moveInput != Vector2.zero)
        {
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
    }

    private void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            attackManager.Attack(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            attackManager.Attack(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            attackManager.Attack(2);
    }
}
