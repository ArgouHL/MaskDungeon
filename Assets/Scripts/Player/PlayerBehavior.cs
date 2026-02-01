using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PinePie.SimpleJoystick;

/// <summary>
/// 控制平台類型
/// </summary>
public enum PlatformType
{
    Windows,  // 使用鍵盤輸入
    Android   // 使用蘑菇頭（Joystick）輸入
}

public class PlayerBehavior : MonoBehaviour
{
    // 攻擊類型陣列，最後一個元素是當前攻擊模式
    private List<int> attackTypes = new List<int> { 0 };

    // 當前攻擊類型（陣列最後一個元素）
    public int CurrentAttackType => attackTypes[attackTypes.Count - 1];
    [SerializeField] playerMask playerMask;

    [Header("平台設置")]
    [Tooltip("選擇當前平台：Windows 使用鍵盤，Android 使用蘑菇頭")]
    [SerializeField] private PlatformType platformType = PlatformType.Windows;

    [Header("蘑菇頭設置")]
    [Tooltip("Joystick 控制器，如果不指定會自動尋找")]
    [SerializeField] private JoystickController joystickController;
    [Tooltip("Joystick 物件的名稱（用於自動尋找）")]
    [SerializeField] private string joystickName = "PinePie Joystick";

    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float gravity = -9.81f;

    [Header("攻击设置")]
    [SerializeField] private AttackPattern[] attackPatterns;

    private Vector3 moveDirection;
    private Quaternion targetRotation;
    private bool hasTargetRotation = false;
    private bool isAttacking = false;
    private float yVelocity = 0f;

    CharacterController characterController;
    private bool isRushing = false;

    /// <summary>
    /// 根據平台類型獲取移動輸入
    /// </summary>
    private Vector2 moveInput
    {
        get
        {
            switch (platformType)
            {
                case PlatformType.Windows:
                    // 使用鍵盤輸入
                    return InputManager.instance.input.Player.Move.ReadValue<Vector2>();

                case PlatformType.Android:
                    // 使用蘑菇頭輸入
                    if (joystickController != null)
                    {
                        return joystickController.InputDirection;
                    }
                    else
                    {
                        Debug.LogWarning("找不到 Joystick 控制器！");
                        return Vector2.zero;
                    }

                default:
                    return Vector2.zero;
            }
        }
    }

    



    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        // 如果是 Android 平台且沒有手動指定 Joystick，自動尋找
        if (platformType == PlatformType.Android && joystickController == null)
        {
            FindJoystick();
        }
    }

    /// <summary>
    /// 自動尋找場景中的 Joystick
    /// </summary>
    private void FindJoystick()
    {
        JoystickController[] joysticks = FindObjectsOfType<JoystickController>();

        foreach (var joystick in joysticks)
        {
            if (joystick.name == joystickName || joystick.gameObject.name == joystickName)
            {
                joystickController = joystick;
                Debug.Log($"成功連接到 Joystick: {joystickController.gameObject.name}");
                return;
            }
        }

        // 如果找不到指定名稱的，就使用第一個找到的
        if (joystickController == null && joysticks.Length > 0)
        {
            joystickController = joysticks[0];
            Debug.LogWarning($"找不到名為 '{joystickName}' 的 Joystick，使用第一個找到的: {joysticks[0].gameObject.name}");
        }
        else if (joysticks.Length == 0)
        {
            Debug.LogError("場景中找不到任何 Joystick！請確保已添加 Joystick Prefab 到 Canvas 下。");
        }
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

    /// <summary>
    /// 公開的攻擊方法，可以從 UI 按鈕或其他腳本呼叫
    /// </summary>
    public void TriggerAttack()
    {
        if (!Menu.gameStartBool)
            return;

        Debug.Log($"[{Time.time:F2}] 攻擊按鈕被按下！");
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
        //delete first if more than 4 types
        if (attackTypes.Count == 6)
        {
            attackTypes.RemoveAt(1);
        }

        attackTypes.Add(newType);
        playerMask.ChangeMask(newType);
        Debug.Log($"獲得新攻擊類型: {newType}, 當前陣列: [{string.Join(", ", attackTypes)}]");


        FindObjectOfType<MaskManager>().UpdateSideMasks(attackTypes);
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
        FindObjectOfType<MaskManager>().UpdateSideMasks(attackTypes);
    }
}
