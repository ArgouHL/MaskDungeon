using UnityEngine;

/// <summary>
/// 平台 UI 管理器 - 根據平台自動顯示/隱藏 UI 元素
/// Android 顯示觸控 UI，Windows 隱藏
/// </summary>
public class PlatformUIManager : MonoBehaviour
{
    [Header("玩家參考")]
    [Tooltip("玩家物件，如果不指定會自動尋找")]
    [SerializeField] private PlayerBehavior player;

    [Header("平台設置")]
    [Tooltip("是否根據平台自動顯示/隱藏此 UI（Android 顯示，Windows 隱藏）")]
    [SerializeField] private bool autoHideByPlatform = true;

    [Tooltip("要控制的 UI 物件（如果不指定，就控制此腳本所在的物件）")]
    [SerializeField] private GameObject targetUI;

    private void Awake()
    {
        // 如果沒有指定 UI 物件，就使用此腳本所在的物件
        if (targetUI == null)
        {
            targetUI = gameObject;
        }

        // 如果沒有指定玩家，自動尋找
        if (player == null)
        {
            player = FindObjectOfType<PlayerBehavior>();
            if (player == null)
            {
                Debug.LogWarning($"[PlatformUIManager] 找不到 PlayerBehavior！{targetUI.name} 將保持預設狀態。");
            }
        }
    }

    private void Start()
    {
        // 根據平台自動顯示/隱藏 UI
        if (autoHideByPlatform && player != null)
        {
            CheckPlatformAndToggleUI();
        }
    }

    /// <summary>
    /// 檢查平台並切換 UI 顯示狀態
    /// </summary>
    private void CheckPlatformAndToggleUI()
    {
        PlatformType currentPlatform = player.CurrentPlatform;

        switch (currentPlatform)
        {
            case PlatformType.Windows:
                // Windows 使用鍵盤，隱藏觸控 UI
                targetUI.SetActive(false);
                Debug.Log($"[PlatformUIManager] 偵測到 Windows 平台，隱藏 {targetUI.name}");
                break;

            case PlatformType.Android:
                // Android 使用觸控，顯示觸控 UI
                targetUI.SetActive(true);
                Debug.Log($"[PlatformUIManager] 偵測到 Android 平台，顯示 {targetUI.name}");
                break;
        }
    }

    /// <summary>
    /// 手動設置 UI 顯示狀態（可從外部呼叫）
    /// </summary>
    public void SetUIVisible(bool visible)
    {
        if (targetUI != null)
        {
            targetUI.SetActive(visible);
        }
    }

    /// <summary>
    /// 手動重新檢查平台並更新 UI（可在運行時切換平台後呼叫）
    /// </summary>
    public void RefreshPlatformUI()
    {
        if (player != null)
        {
            CheckPlatformAndToggleUI();
        }
    }
}
