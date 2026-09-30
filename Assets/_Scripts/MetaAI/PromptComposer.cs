using System;
using System.Collections;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;

/// <summary>
/// LLMへ渡す最終プロンプトを合成する責務を持つ
/// </summary>
public class PromptComposer : MonoBehaviour
{
    [Header("静的プロンプト")]
    [SerializeField] private PromptContextConfig m_promptContextConfig;

    [Header("動的プロンプト関連")]
    [SerializeField, Tooltip("説明文の単一情報源。ここに登録された順で連結します。")] private TutorialSequenceAsset m_tutorialSequence;
    [SerializeField] private AllPaintingsDatabase m_allPaintingsDatabase;

    [Header("デバッグ")]
    [SerializeField, Tooltip("生成時にログセクション（<log>に入る最終形）をUnityコンソールへ出力します。")]
    private bool m_printLogsToConsole = false;

    [SerializeField, Tooltip("生成時に合成された最終プロンプト全文をUnityコンソールへ出力します（長文になる可能性があります）。")]
    private bool m_printComposedPromptToConsole = false;

    [SerializeField, Tooltip("コンソールへ出力する最大文字数（0以下は無制限）。")]
    private int m_consoleMaxChars = 4000;

    /// <summary>
    /// 受動（ユーザ発話への返答）用のプロンプトを生成。
    /// </summary>
    /// <remarks>
    /// ユーザ発話に加えて、現在フェーズの説明（テンプレート）・追加情報（動的コンテキスト）・行動ログを含める。
    /// </remarks>
    public string PassiveCompose(string recognizeResult, BlackBoard blackBoard, BehaviorLog log, Phase phase)
    {
        var sep = m_promptContextConfig != null ? m_promptContextConfig.sectionSeparator : "\n\n";
        var sb = new StringBuilder(512);

        // 0) 重要コンテキスト
        sb.Append("<重要コンテキスト>\n" +
                $"{phase}フェーズにおいて、客が「{recognizeResult}」という発話を行いました。\n" +
                "ユーザの発話とこれまでの会話の流れに対して適切で、かつ現在のフェーズの目的を達成するような返答を生成してください。\n" +
                "</重要コンテキスト>");
        sb.Append(sep);

        // 1) フェーズ情報（現在フェーズ、その説明、追加情報）
        var phaseT = m_promptContextConfig != null ? m_promptContextConfig.GetPhaseTemplate(phase) : string.Empty;
        var dynamicT = FetchDynamicContext(phase, blackBoard);

        var phaseBody = string.Join("\n", new[] { phaseT, dynamicT }.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (!string.IsNullOrWhiteSpace(phaseBody))
        {
            sb.Append($"<phase>\n{phaseBody}\n</phase>");
            sb.Append(sep);
        }

        // 2) 絵の情報
        sb.Append("[追加コンテキスト]\n");
        sb.Append(FetchGoodsContext());
        sb.Append(sep);

        // 3) 行動ログ
        if (m_promptContextConfig != null && m_promptContextConfig.includeLogs && log != null)
        {
            var text = log.GetLogsAsText(m_promptContextConfig.includedLogKinds?.ToArray());

            // 行数制限
            if (m_promptContextConfig.maxLogLines > 0)
            {
                var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                if (lines.Length > m_promptContextConfig.maxLogLines)
                {
                    lines = lines.Skip(Math.Max(0, lines.Length - m_promptContextConfig.maxLogLines)).ToArray();
                    // 直近ログを上に表示するため、末尾N行を取得後に反転
                    Array.Reverse(lines);
                    text = string.Join("\n", lines);
                }
                else
                {
                    // 直近ログを上に表示するため全体を反転
                    Array.Reverse(lines);
                    text = string.Join("\n", lines);
                }
            }

            // ログ全体の文字数制限
            if (m_promptContextConfig.maxLogChars > 0 && text.Length > m_promptContextConfig.maxLogChars)
            {
                // 直近ログを上（先頭）にしているため、先頭側を残す
                text = text.Substring(0, m_promptContextConfig.maxLogChars);
            }

            if (!string.IsNullOrEmpty(text))
            {
                PrintToConsoleIfEnabled($"PromptComposer PassiveCompose logs (phase={phase})", text, m_printLogsToConsole);
                sb.Append($"<log>\n{text}\n</log>");
                sb.Append(sep);
            }
        }

        var composed = sb.ToString().Trim();

        // プロンプト全体の文字数制限
        if (m_promptContextConfig != null && m_promptContextConfig.maxPromptChars > 0 && composed.Length > m_promptContextConfig.maxPromptChars)
        {
            composed = composed.Substring(0, m_promptContextConfig.maxPromptChars);
        }
        PrintToConsoleIfEnabled($"PromptComposer PassiveCompose composed (phase={phase})", composed, m_printComposedPromptToConsole);
        return composed;
    }

    /// <summary>
    /// 能動的な発話を生成するためのプロンプトを生成。
    /// </summary>
    /// <param name="blackBoard">共有状態（動的なコンテキスト用）</param>
    /// <param name="log">行動ログ（null可）</param>
    /// <param name="phase">現在フェーズ</param>
    /// <param name="speechType">発話タイプ</param>
    /// <param name="specificPrompt">トリガー個別のプロンプト片</param>
    public string Compose(
        BlackBoard blackBoard,
        BehaviorLog log,
        Phase phase,
        SpeechType speechType,
        string specificPrompt)
    {
        var sep = m_promptContextConfig != null ? m_promptContextConfig.sectionSeparator : "\n\n";
        var sb = new StringBuilder(512);

        // 1) 重要コンテキスト
        if (!string.IsNullOrWhiteSpace(specificPrompt))
        {
            sb.Append($"<重要コンテキスト>\n{specificPrompt}\n</重要コンテキスト>");
            sb.Append(sep);
        }

        sb.Append("[補足情報]\n");

        // 2) フェーズ関係
        var phaseT = m_promptContextConfig != null ? m_promptContextConfig.GetPhaseTemplate(phase) : string.Empty;
        phaseT += "\n" + FetchDynamicContext(phase, blackBoard);
        if (!string.IsNullOrWhiteSpace(phaseT))
        {
            sb.Append($"<phase>\n{phaseT}\n</phase>");
            sb.Append(sep);
        }

        // 3) 発話タイプ関係
        var typeT = m_promptContextConfig != null ? m_promptContextConfig.GetSpeechTypeTemplate(speechType) : string.Empty;
        if (!string.IsNullOrWhiteSpace(typeT))
        {
            sb.Append($"<speechType>\n{typeT}\n</speechType>");
            sb.Append(sep);
        }

        // 4) 絵の情報
        sb.Append($"<paintingInfo>\n{FetchGoodsContext()}\n</paintingInfo>");
        sb.Append(sep);

        // 5) ログ
        if (m_promptContextConfig != null && m_promptContextConfig.includeLogs && log != null)
        {
            var text = log.GetLogsAsText(m_promptContextConfig.includedLogKinds?.ToArray());

            // 行数制限
            if (m_promptContextConfig.maxLogLines > 0)
            {
                var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                if (lines.Length > m_promptContextConfig.maxLogLines)
                {
                    lines = lines.Skip(Math.Max(0, lines.Length - m_promptContextConfig.maxLogLines)).ToArray();
                    // 直近ログを上に表示するため、末尾N行を取得後に反転
                    Array.Reverse(lines);
                    text = string.Join("\n", lines);
                }
                else
                {
                    // 直近ログを上に表示するため全体を反転
                    Array.Reverse(lines);
                    text = string.Join("\n", lines);
                }
            }

            // ログ全体の文字数制限
            if (m_promptContextConfig.maxLogChars > 0 && text.Length > m_promptContextConfig.maxLogChars)
            {
                // 直近ログを上（先頭）にしているため、先頭側を残す
                text = text.Substring(0, m_promptContextConfig.maxLogChars);
            }

            if (!string.IsNullOrEmpty(text))
            {
                PrintToConsoleIfEnabled($"PromptComposer Compose logs (phase={phase}, speechType={speechType})", text, m_printLogsToConsole);
                sb.Append($"<log>\n{text}\n</log>");
                sb.Append(sep);
            }
        }

        var composed = sb.ToString().Trim();

        // プロンプト全体の文字数制限
        if (m_promptContextConfig != null && m_promptContextConfig.maxPromptChars > 0 && composed.Length > m_promptContextConfig.maxPromptChars)
        {
            //? 今回は末尾をカット
            composed = composed.Substring(0, m_promptContextConfig.maxPromptChars);
        }

        PrintToConsoleIfEnabled($"PromptComposer Compose composed (phase={phase}, speechType={speechType})", composed, m_printComposedPromptToConsole);
        return composed;
    }

    /// <summary>
    /// specificPrompt と Logs のみでプロンプトを構築する簡易版。
    /// </summary>
    public string ComposeOnlySpecificAndLogs(BehaviorLog log, string specificPrompt)
    {
        var sep = m_promptContextConfig != null ? m_promptContextConfig.sectionSeparator : "\n\n";
        var sb = new StringBuilder(256);

        // specificPrompt
        if (!string.IsNullOrWhiteSpace(specificPrompt))
        {
            sb.Append($"<重要コンテキスト>\n{specificPrompt}\n</重要コンテキスト>");
            sb.Append(sep);
        }

        // Logs（設定でログ非表示ならスキップ）
        if (m_promptContextConfig != null && m_promptContextConfig.includeLogs && log != null)
        {
            var text = log.GetLogsAsText(m_promptContextConfig.includedLogKinds?.ToArray());

            if (m_promptContextConfig.maxLogLines > 0 && !string.IsNullOrEmpty(text))
            {
                var lines = text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
                if (lines.Length > m_promptContextConfig.maxLogLines)
                {
                    lines = lines.Skip(Mathf.Max(0, lines.Length - m_promptContextConfig.maxLogLines)).ToArray();
                    text = string.Join("\n", lines);
                }
            }

            if (m_promptContextConfig.maxLogChars > 0 && !string.IsNullOrEmpty(text) && text.Length > m_promptContextConfig.maxLogChars)
            {
                text = text.Substring(text.Length - m_promptContextConfig.maxLogChars);
            }

            if (!string.IsNullOrEmpty(text))
            {
                PrintToConsoleIfEnabled("PromptComposer ComposeOnlySpecificAndLogs logs", text, m_printLogsToConsole);
                sb.Append($"<log>\n{text}\n</log>");
                sb.Append(sep);
            }
        }

        var composed = sb.ToString().Trim();
        if (m_promptContextConfig != null && m_promptContextConfig.maxPromptChars > 0 && composed.Length > m_promptContextConfig.maxPromptChars)
        {
            composed = composed.Substring(0, m_promptContextConfig.maxPromptChars);
        }
        PrintToConsoleIfEnabled("PromptComposer ComposeOnlySpecificAndLogs composed", composed, m_printComposedPromptToConsole);
        return composed;
    }

    private void PrintToConsoleIfEnabled(string title, string text, bool enabled)
    {
        if (!enabled || string.IsNullOrEmpty(text)) return;

        if (m_consoleMaxChars > 0 && text.Length > m_consoleMaxChars)
        {
            text = text.Substring(0, m_consoleMaxChars) + "\n...(truncated)";
        }

        Debug.Log($"[{title}]\n{text}");
    }

    /// <summary>
    /// Phaseに応じた追加情報を取得する。
    /// </summary>
    /// <param name="currentPhase">現在のフェーズ</param>
    private string FetchDynamicContext(Phase currentPhase, BlackBoard blackBoard)
    {
        switch (currentPhase)
        {
            case Phase.Enter:
                return FetchEnterPhaseContext();
            case Phase.Explore:
                return FetchExplorePhaseContext();
            case Phase.Recommend:
                return FetchRecommendPhaseContext(blackBoard);
            case Phase.Purchase:
                return FetchPurchasePhaseContext(blackBoard);
            default:
                break;
        }
        return string.Empty;
    }

    private string FetchEnterPhaseContext() { return String.Empty; }
    private string FetchExplorePhaseContext() { return m_allPaintingsDatabase.GetOverviewText(); }
    private string FetchRecommendPhaseContext(BlackBoard blackBoard) { return GetTopPaintingsByInterest(blackBoard, 5); }
    private string FetchPurchasePhaseContext(BlackBoard blackBoard) { return GetTopPaintingsByInterest(blackBoard, 3); }
    private string FetchGoodsContext() { return m_allPaintingsDatabase.GetDetailedText(); }

    /// <summary>
    /// TutorialSequenceAsset の説明を順番に結合した文字列を返す。
    /// </summary>
    /// <param name="separator">結合セパレータ（デフォルトは改行）</param>
    /// <param name="includeIndexAndTitle">先頭に番号とタイトルを付与するか</param>
    private string ComposeTutorialSummary(string separator = "\n", bool includeIndexAndTitle = false)
    {
        if (m_tutorialSequence == null) return string.Empty;
        return m_tutorialSequence.BuildSummary(separator, includeIndexAndTitle);
    }

    /// <summary>
    /// BlackBoard.paintingsInterestScore から興味度(CurrentScore)の高い上位N件を
    /// ランキング形式の文字列で返す。
    /// </summary>
    /// <param name="blackBoard">興味度データを保持する BlackBoard</param>
    /// <param name="topN">出力する上位件数（0以下なら空文字を返す）</param>
    /// <param name="lineSeparator">行区切り（デフォルト: \n）</param>
    /// <returns>ランキング文字列。対象が無い場合は空文字。</returns>
    private string GetTopPaintingsByInterest(BlackBoard blackBoard, int topN, string lineSeparator = "\n")
    {
        if (blackBoard == null || topN <= 0) return string.Empty;

        var dict = blackBoard.paintingsInterestScore;
        if (dict == null || dict.Count == 0) return string.Empty;

        // スコア降順、同点時は StareCount -> RecognizeCount -> ClickCount -> PaintingID
        var ordered = dict.Values
            .OrderByDescending(p => p.CurrentScore)
            .ThenByDescending(p => p.StareCount)
            .ThenByDescending(p => p.RecognizeCount)
            .ThenByDescending(p => p.ClickCount)
            .ThenBy(p => p.PaintingID)
            .Take(topN)
            .ToList();

        if (ordered.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("ユーザは以下の絵に興味があるようです。");
        sb.Append(lineSeparator);

        for (int i = 0; i < ordered.Count; i++)
        {
            var p = ordered[i];
            var artist = string.IsNullOrWhiteSpace(p.ArtistName) ? "[Unknown]" : p.ArtistName;
            var name = string.IsNullOrWhiteSpace(p.PaintingName) ? "[Unknown]" : p.PaintingName;

            // 両方空の場合も上の補完で "[Unknown]: [Unknown]" となる
            sb.AppendFormat("{0}位: {1}: {2}", i + 1, artist, name);
            if (i < ordered.Count - 1)
            {
                sb.Append(lineSeparator);
            }
        }

        return sb.ToString();
    }
}
