// DialogueUIView.cs
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class DialogueUIView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_speakerNameText;
    [SerializeField] private TextMeshProUGUI m_contentText;
    [SerializeField] private CanvasGroup m_canvasGroup;

    // 末尾の句点（。や.）の直後に改行が無ければ改行を付与するための正規表現
    // 日本語の句点：後続の空白は取り除き、既に改行・文字列終端なら無視
    private static readonly Regex s_jpSentenceEnd = new Regex("(。)\\s*(?!\\n|$)", RegexOptions.Compiled);
    // ピリオド：小数点や連続ドット（...）は対象外。後続の空白は取り除き、既に改行・文字列終端なら無視
    private static readonly Regex s_enSentenceEnd = new Regex("(?<![.\\d])\\.(?![.\\d])\\s*(?!\\n|$)", RegexOptions.Compiled);
    // 感嘆符・疑問符（英/日）。連続する !?！? などもまとめて一塊として扱い、束の直後に改行を追加（空白は吸収）
    private static readonly Regex s_exclaimQuestionEnd = new Regex("([!?！？]+)\\s*(?!\\n|$)", RegexOptions.Compiled);

    private void Awake()
    {
        Hide();
    }

    // UIを表示し、内容を更新するための唯一の窓口
    public void Show(string speakerName, string content)
    {
        m_speakerNameText.text = speakerName;
        m_contentText.text = InsertLineBreaks(content);

        // CanvasGroupを使って表示
        m_canvasGroup.alpha = 1f;
        m_canvasGroup.interactable = true;
        m_canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        m_canvasGroup.alpha = 0f;
        m_canvasGroup.blocksRaycasts = false;
        m_canvasGroup.interactable = false;
    }

    /// <summary>
    /// 文末の「。」または「.」の直後に改行が無ければ改行を追加して見やすく整形します。
    /// ただし、以下は改行しません：
    /// - 連続ドット（... など）の一部
    /// - 小数点（3.14 など）のピリオド
    /// 既に改行がある場合や文字列終端の場合はそのままにします。
    /// </summary>
    private static string InsertLineBreaks(string text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;

        // 正規化（Windows/Unix差異の吸収）
        text = text.Replace("\r\n", "\n");

        // 日本語の文末句点の後に改行を追加（空白は吸収）
        text = s_jpSentenceEnd.Replace(text, "$1\n");

        // 英語のピリオド（小数点・連続ドット除外）の後に改行を追加（空白は吸収）
        text = s_enSentenceEnd.Replace(text, ".\n");

        // 感嘆符・疑問符（連続も含む）の後に改行を追加（空白は吸収）
        text = s_exclaimQuestionEnd.Replace(text, "$1\n");

        return text;
    }
}