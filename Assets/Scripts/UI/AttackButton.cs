using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 攻擊按鈕 - 點擊時觸發玩家攻擊
/// 根據平台自動顯示/隱藏：Android 顯示，Windows 隱藏
/// </summary>
[RequireComponent(typeof(Image))]
public class AttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("玩家參考")]
    [Tooltip("玩家物件，如果不指定會自動尋找")]
    [SerializeField] private PlayerBehavior player;

    [Header("平台設置")]
    [Tooltip("是否根據平台自動顯示/隱藏按鈕（Android 顯示，Windows 隱藏）")]
    [SerializeField] private bool autoHideByPlatform = true;

    [Header("視覺回饋（選用）")]
    [Tooltip("按下時的透明度")]
    [SerializeField] private float pressedAlpha = 0.7f;

    private Image buttonImage;
    private float originalAlpha;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        originalAlpha = buttonImage.color.a;

        // 如果沒有指定玩家，自動尋找
        if (player == null)
        {
            player = FindObjectOfType<PlayerBehavior>();
            if (player == null)
            {
                Debug.LogError("找不到 PlayerBehavior！請確保場景中有玩家物件。");
            }
            else
            {
                Debug.Log($"成功連接到玩家: {player.gameObject.name}");
            }
        }
    }

    private void Start()
    {
        // 根據平台自動顯示/隱藏按鈕
        if (autoHideByPlatform && player != null)
        {
            CheckPlatformAndToggleButton();
        }
    }

    /// <summary>
    /// 檢查平台並切換按鈕顯示狀態
    /// </summary>
    private void CheckPlatformAndToggleButton()
    {
        PlatformType currentPlatform = player.CurrentPlatform;

        switch (currentPlatform)
        {
            case PlatformType.Windows:
                // Windows 使用鍵盤，隱藏攻擊按鈕
                gameObject.SetActive(false);
                Debug.Log("偵測到 Windows 平台，隱藏攻擊按鈕");
                break;

            case PlatformType.Android:
                // Android 使用觸控，顯示攻擊按鈕
                gameObject.SetActive(true);
                Debug.Log("偵測到 Android 平台，顯示攻擊按鈕");
                break;
        }
    }

    /// <summary>
    /// 當按下按鈕時
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // 視覺回饋 - 改變透明度
        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = pressedAlpha;
            buttonImage.color = color;
        }

        // 觸發攻擊
        if (player != null)
        {
            player.TriggerAttack();
        }
    }

    /// <summary>
    /// 當放開按鈕時
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        // 恢復原本的透明度
        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = originalAlpha;
            buttonImage.color = color;
        }
    }
}
