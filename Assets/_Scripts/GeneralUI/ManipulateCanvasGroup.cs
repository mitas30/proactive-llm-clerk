using UnityEngine;

/// <summary>
/// このコンポーネントと同じ階層にあるCanvasGroupコンポーネントを操作するクラス
/// </summary>
/// <remarks>
/// GameEventクラスから呼び出される想定
/// </remarks>
public class ManipulateCanvasGroup : MonoBehaviour
{
    [System.Serializable]
    public class GameEventVisibilityPair
    {
        [SerializeField] public GameEvent gameEvent;
        [SerializeField] public bool isEnableUI;
    }

    [SerializeField] private GameEventVisibilityPair[] eventVisibilityPairs;
    private CanvasGroup canvasGroup;

    private void OnEnable()
    {
        foreach (var pair in eventVisibilityPairs)
        {
            if (pair.gameEvent != null)
            {
                pair.gameEvent.AddListener(() => SetVisibility(pair.isEnableUI));
            }
        }
    }

    private void OnDisable()
    {
        foreach (var pair in eventVisibilityPairs)
        {
            if (pair.gameEvent != null)
            {
                pair.gameEvent.RemoveListener(() => SetVisibility(pair.isEnableUI));
            }
        }
    }
    void Start()
    {
        // CanvasGroupコンポーネントを取得
        canvasGroup = GetComponent<CanvasGroup>();

        // CanvasGroupが見つからない場合は警告を出力
        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup component not found on " + gameObject.name);
        }
    }

    /// <summary>
    /// CanvasGroupの表示状態を指定された状態に設定します。
    /// </summary>
    /// <param name="isEnable">trueの場合、表示・操作可能に。falseの場合、非表示・操作不可に。</param>
    public void SetVisibility(bool isEnable)
    {
        if (isEnable)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
