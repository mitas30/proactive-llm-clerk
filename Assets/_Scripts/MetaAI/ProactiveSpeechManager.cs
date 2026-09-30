using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

/// <summary>
/// ゲーム内イベントを監視し、SpeakTriggerを評価して発話リクエストを送信する。
/// </summary>
[RequireComponent(typeof(BlackBoard))]
public class ProactiveSpeechManager : MonoBehaviour
{
    [SerializeField] private Behavior m_aiCharacterBehavior;
    private BlackBoard m_blackBoard;

    [Header("能動的な発話イベントたち")]
    [SerializeField, Tooltip("注意 : リストの順で優先順位が決まる")] private List<SpeakTrigger> m_speakTriggers;

    [Header("プロンプト整形")]
    [SerializeField] private PromptComposer m_promptComposer;

    [Header("ログ出力")]
    [SerializeField] private BehaviorLog m_behaviorLog;

    private void Awake()
    {
        m_blackBoard = GetComponent<BlackBoard>();
        foreach (var trigger in m_speakTriggers)
        {
            trigger.ResetAllDynamicMember();
        }
    }

    void Update()
    {
        CheckTriggerableSpeech();
    }

    private void CheckTriggerableSpeech()
    {
        // 状態更新は常に行う（ban 中でも内部状態を進める）
        foreach (var trigger in m_speakTriggers)
        {
            trigger.Tick(m_blackBoard);
        }

        // ban 中は発火判定そのものを行わない
        if (m_blackBoard.BanProactivelySpeech)
        {
            return;
        }

        bool isAnyTriggered = false;
        foreach (var trigger in m_speakTriggers)
        {
            if (trigger.CanActivate(m_blackBoard))
            {
                isAnyTriggered = true;

                // 発火確定時の更新（クールダウン更新・ターゲット確定など）
                trigger.OnActivated(m_blackBoard);

                SpeechType speechType = trigger.ApplyAppropriateType(m_blackBoard.CurrentPhase);
                Phase currentPhase = m_blackBoard.CurrentPhase;
                trigger.ProvideSpecificContextForPrompt(m_blackBoard, currentPhase, speechType, out string specificPrompt);

                // ログに重要コンテキストのみ記録
                if (m_behaviorLog != null)
                {
                    string trigName = trigger != null ? trigger.GetType().Name : "UnknownTrigger";
                    string promptSnippet = specificPrompt;
                    if (!string.IsNullOrEmpty(promptSnippet)) promptSnippet = System.Text.RegularExpressions.Regex.Replace(promptSnippet, "\\s+", " ");
                    m_behaviorLog.AddLog(BehaviorLog.BehaviorEventKind.ProactiveTrigger,
                        $"{trigName} prompt=\"{promptSnippet}\" phase={currentPhase} type={speechType}");
                }
                m_blackBoard.WantToSpeakProactively = true;

                // プロンプト合成
                string proactivePrompt = m_promptComposer.Compose(
                    m_blackBoard,
                    m_behaviorLog,
                    m_blackBoard.CurrentPhase,
                    speechType,
                    specificPrompt
                );

                // BTへ設定
                m_aiCharacterBehavior.SetVariableValue("proactivePrompt", proactivePrompt);
            }
            if (isAnyTriggered) break;
        }
    }

    /// <summary>
    /// specificPrompt と現在のログだけでプロンプトを構築して返す。 チュートリアルのイベント用
    /// </summary>
    public string ComposeProactivePromptFromSpecific(string specificPrompt)
    {
        if (m_promptComposer == null)
        {
            Debug.LogWarning("ProactiveSpeechManager: PromptComposer が未設定です");
            return specificPrompt ?? string.Empty;
        }
        return m_promptComposer.ComposeOnlySpecificAndLogs(m_behaviorLog, specificPrompt ?? string.Empty);
    }
}