using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プロンプトの追加コンテキストを構成するための設定。
/// ・フェーズごとの前置きテンプレート
/// ・スピーチタイプごとの前置きテンプレート
/// ・システムプロンプト（人格や話し方のガイド）
/// ・ログの取り込み有無や範囲
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/MetaAI/Prompt Context Config", fileName = "PromptContextConfig")]
public class PromptContextConfig : ScriptableObject
{
    [Serializable]
    public class PhasePromptEntry
    {
        public Phase phase;
        [TextArea(4, 6)] public string template;
    }

    [Serializable]
    public class SpeechTypePromptEntry
    {
        public SpeechType speechType;
        [TextArea(4, 6)] public string template;
    }

    [Header("フェーズ別テンプレート")]
    public List<PhasePromptEntry> phasePrompts = new List<PhasePromptEntry>();

    [Header("スピーチタイプ別テンプレート")]
    public List<SpeechTypePromptEntry> speechTypePrompts = new List<SpeechTypePromptEntry>();

    [Header("ログ取り込み設定")]
    public bool includeLogs;
    public List<BehaviorLog.BehaviorEventKind> includedLogKinds = new List<BehaviorLog.BehaviorEventKind>
    {
        BehaviorLog.BehaviorEventKind.PaintingInteraction,
        BehaviorLog.BehaviorEventKind.ProactiveTrigger,
        BehaviorLog.BehaviorEventKind.FlowTransition,
        BehaviorLog.BehaviorEventKind.UserRespondSpeech,
        BehaviorLog.BehaviorEventKind.ProactiveSpeech
    };
    [Tooltip("直近のログ行数（0で制限なし）")] public int maxLogLines;

    [Header("長さ制限（0で無制限）")]
    [Tooltip("プロンプト全体の最大文字数。超える場合はログ部分から優先的にカットします。")]
    public int maxPromptChars = 0;
    [Tooltip("ログ部の最大文字数（0で無制限）。maxPromptCharsより先に適用されます。")]
    public int maxLogChars = 0;

    [Header("区切り")]
    public string sectionSeparator;

    public string GetPhaseTemplate(Phase phase)
    {
        foreach (var e in phasePrompts)
        {
            if (e != null && e.phase.Equals(phase)) return e.template;
        }
        return string.Empty;
    }

    public string GetSpeechTypeTemplate(SpeechType type)
    {
        foreach (var e in speechTypePrompts)
        {
            if (e != null && e.speechType.Equals(type)) return e.template;
        }
        return string.Empty;
    }
}
