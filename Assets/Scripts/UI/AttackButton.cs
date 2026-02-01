using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 攻擊按鈕 - 點擊時觸發玩家攻擊
/// </summary>
[RequireComponent(typeof(Image))]
public class AttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("玩家參考")]
    [Tooltip("玩家物件，如果不指定會自動尋找")]
    [SerializeField] private PlayerBehavior player;

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
