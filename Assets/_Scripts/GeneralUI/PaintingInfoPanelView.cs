using UnityEngine;
using TMPro;

/// <summary>
/// PaintingInfoPanelとその配下のUI要素への参照を保持し、
/// UIの表示・非表示や内容の更新といった「見た目」に関する責務を持つクラス。
/// </summary>
public class PaintingInfoPanelView : MonoBehaviour
{
    [Header("UI要素への参照")]
    [SerializeField] private TMP_Text m_titleText;
    [SerializeField] private TMP_Text m_artistText;
    [SerializeField] private TMP_Text m_yearText;
    [SerializeField] private TMP_Text m_descriptionText;

    // CanvasGroupもここで管理する
    [SerializeField] private CanvasGroup m_canvasGroup;


    // 外部からテキストにアクセスするためのプロパティ（読み取り専用）
    public TMP_Text TitleText => m_titleText;
    public TMP_Text DescriptionText => m_descriptionText;


    private void Awake()
    {
        // 参照が設定されているかチェック
        if (m_titleText == null) Debug.LogError("TitleTextが設定されていません。", this);
        if (m_artistText == null) Debug.LogError("ArtistTextが設定されていません。", this);
        if (m_yearText == null) Debug.LogError("YearTextが設定されていません。", this);
        if (m_descriptionText == null) Debug.LogError("DescriptionTextが設定されていません。", this);
        if (m_canvasGroup == null)
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
            if (m_canvasGroup == null) Debug.LogError("CanvasGroupが見つかりません。", this);
        }
    }

    /// <summary>
    /// パネルを表示する
    /// </summary>
    public void Show()
    {
        m_canvasGroup.alpha = 1f;
        m_canvasGroup.interactable = true;
        m_canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// パネルを非表示にする
    /// </summary>
    public void Hide()
    {
        m_canvasGroup.alpha = 0f;
        m_canvasGroup.interactable = false;
        m_canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// パネルの情報を更新する
    /// </summary>
    /// <param name="title">表示するタイトル</param>
    /// <param name="artist">表示する作家名</param>
    /// <param name="year">表示する制作年</param>
    /// <param name="description">表示する説明文</param>
    public void UpdateInfo(string title, string artist, string year, string description)
    {
        m_titleText.text = title;
        m_artistText.text = artist;
        m_yearText.text = year;
        m_descriptionText.text = description;
    }
}