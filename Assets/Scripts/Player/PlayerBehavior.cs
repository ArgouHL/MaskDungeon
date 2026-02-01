using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehavior : MonoBehaviour
{
    // 攻擊類型陣列，最後一個元素是當前攻擊模式
    private List<int> attackTypes = new List<int> { 0 };

    // 當前攻擊類型（陣列最後一個元素）
    public int CurrentAttackType => attackTypes[attackTypes.Count - 1];
    [SerializeField] playerMask playerMask;
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float gravity = -9.81f;

    [Header("攻击设置")]
    [SerializeField] private AttackPattern[] attackPatterns;

    private Vector2 moveInput => InputManager.instance.input.Player.Move.ReadValue<Vector2>();
    private Vector3 moveDirection;
    private Quaternion targetRotation;
    private bool hasTargetRotation = false;
    private bool isAttacking = false;
    private float yVelocity = 0f;

    CharacterController characterController;
    private bool isRushing = false;

    



    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        InputManager.instance.input.Player.Attack.performed += Attack;

    }

    private void OnDisable()
    {
        InputManager.instance.input.Player.Attack.performed -= Attack;

    }

    void Update()
    {
        if (!Menu.gameStartBool)
            return;

        Move();
    }

    private void Move()
    {
        // 處理重力（不受攻擊狀態影響）
        if (characterController.isGrounded && yVelocity < 0)
        {
            yVelocity = -2f; // 保持一點向下的力，確保持續接觸地面
        }
        else
        {
            yVelocity += gravity * Time.deltaTime; // 應用重力
        }
        if (isRushing)
        {
            Vector3 move = transform.forward * moveSpeed * Time.deltaTime * 3f;
            characterController.Move(move);
            return;
        }

        // 攻擊時不能移動也不能旋轉
        // if (isAttacking)
        // {
        //     // 只應用重力
        //     Vector3 gravityMove = new Vector3(0, yVelocity, 0) * Time.deltaTime;
        //     characterController.Move(gravityMove);
        //     return;
        // }

        // 檢測按鍵輸入（支持八方位）
        //moveInput = new Vector2(
        //    Keyboard.current.dKey.isPressed ? 1 : (Keyboard.current.aKey.isPressed ? -1 : 0),
        //    Keyboard.current.wKey.isPressed ? 1 : (Keyboard.current.sKey.isPressed ? -1 : 0)
        //);

        // 計算移動和旋轉方向（八方位）
        if (moveInput != Vector2.zero)
        {
            moveDirection = new Vector3(moveInput.x + moveInput.y, 0f, -moveInput.x + moveInput.y).normalized;

            // 檢測是否剛按下按鍵或沒有目標旋轉
            //bool keyPressed = Keyboard.current.wKey.wasPressedThisFrame ||
            //                 Keyboard.current.sKey.wasPressedThisFrame ||
            //                 Keyboard.current.aKey.wasPressedThisFrame ||
            //                 Keyboard.current.dKey.wasPressedThisFrame;

            if (!hasTargetRotation)
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

        // 計算最終移動（水平移動 + 重力）
        Vector3 finalMove = Vector3.zero;
        if (moveInput != Vector2.zero)
        {
            finalMove = moveDirection * moveSpeed * Time.deltaTime;
        }
        finalMove.y = yVelocity * Time.deltaTime;

        characterController.Move(finalMove);
    }

    private void Attack(InputAction.CallbackContext context)
    {
        if (!Menu.gameStartBool)
            return;

        PerformAttack(CurrentAttackType);
    }

    private void PerformAttack(int index)
    {
        if (isAttacking || index >= attackPatterns.Length) return;

        StartCoroutine(AttackCoroutine(index));
    }

    private IEnumerator AttackCoroutine(int index)
    {
        isAttacking = true;

        GameObject atk = Instantiate(attackPatterns[index].atkPrefab, transform);
        atk.transform.localPosition = attackPatterns[index].point;
        if (index != 4 && index != 2) // 衝刺
        {
            atk.transform.parent = null;
        }


        // 設定攻擊來源為玩家
        SetAttackSource(atk, "Player");
        if (index == 4) // 衝刺
        {
            isRushing = true;
        }


        yield return new WaitForSeconds(attackPatterns[index].atkTime);
        isRushing = false;

        isAttacking = false;
    }

    private void SetAttackSource(GameObject attackObject, string source)
    {
        CheckAttack checkAttack = attackObject.GetComponent<CheckAttack>();
        if (checkAttack == null)
        {
            checkAttack = attackObject.transform.GetChild(0).GetComponent<CheckAttack>();
        }
        if (checkAttack != null)
        {
            checkAttack.SetPlayer(this);
            checkAttack.attackSource = source;
        }
    }

    // 打到敵人時，獲得新的攻擊類型
    public void AddAttackType(int newType)
    {
        attackTypes.Add(newType);
        playerMask.ChangeMask(newType);
        Debug.Log($"獲得新攻擊類型: {newType}, 當前陣列: [{string.Join(", ", attackTypes)}]");
    }

    // 被敵人打到時，移除最後一個攻擊類型
    public void RemoveLastAttackType()
    {
        if(isRushing) return;
        if (attackTypes.Count > 1) // 至少保留一個攻擊類型
        {
            int removedType = attackTypes[attackTypes.Count - 1];
            attackTypes.RemoveAt(attackTypes.Count - 1);
            Debug.Log($"失去攻擊類型: {removedType}, 當前陣列: [{string.Join(", ", attackTypes)}]");
            playerMask.ChangeMask(attackTypes[attackTypes.Count - 1]);
        }
        else
        {
            Menu.instance.GameOverObj.SetActive(true);
            Debug.Log("你死了");
        }
    }
}
