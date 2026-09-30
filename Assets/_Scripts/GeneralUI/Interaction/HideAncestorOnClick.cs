using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class HideAncestorOnClick : MonoBehaviour
{
    [SerializeField, Tooltip("何階層上の祖先を非表示にするか（1＝親、2＝祖父など）")]
    private int m_ancestorLevel = 1;

    private Button m_button;

    void Awake()
    {
        m_button = GetComponent<Button>();
        m_button.onClick.AddListener(OnClicked);
    }

    /// <summary>
    /// ボタン押下時に、指定階層上の祖先パネルを CanvasGroup で非表示にする
    /// </summary>
    public void OnClicked()
    {
        // 祖先トランスフォームを取得
        Transform target = transform;
        for (int i = 0; i < m_ancestorLevel; i++)
        {
            if (target.parent != null)
                target = target.parent;
            else
            {
                Debug.LogWarning($"[{nameof(HideAncestorOnClick)}] {m_ancestorLevel} 階層上の祖先が存在しません。");
                return;
            }
        }

        // CanvasGroup を取得 or 追加
        CanvasGroup cg = target.GetComponent<CanvasGroup>();

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    void OnDestroy()
    {
        m_button.onClick.RemoveListener(OnClicked);
    }
}
