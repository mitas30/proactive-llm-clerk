using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 現段階では、発話条件を持っているクラス。
/// 条件、プロンプト、事後処理、優先度などを保持する。
/// </summary>
public abstract class SpeakTrigger : ScriptableObject
{
    [Header("プロンプト")]
    [SerializeField, TextArea(3, 5), Tooltip("この発話イベントのコンテキスト")] protected string m_promptTemplate;

    /// <summary>
    /// すべての動的メンバーを初期化する。
    /// </summary>
    public abstract void ResetAllDynamicMember();

    /// <summary>
    /// 内部状態の更新（観測・集計・スナップショット更新など）。
    /// 発火禁止中でも呼ばれる想定。
    /// </summary>
    public virtual void Tick(BlackBoard blackBoard) { }

    /// <summary>
    /// この発話イベントが現在発動可能か評価する。
    /// NOTE: 可能な限り副作用なし（内部状態の更新は Tick / OnActivated に分離）。
    /// </summary>
    public abstract bool CanActivate(BlackBoard blackBoard);

    /// <summary>
    /// 発火が確定した瞬間にのみ呼ばれるフック。
    /// クールダウン更新や、プロンプト用のターゲット確定などをここで行う。
    /// </summary>
    public virtual void OnActivated(BlackBoard blackBoard) { }

    /// <summary>
    /// 現在のフェーズに応じて、適切な発話タイプを返す。
    /// </summary>
    /// <param name="currentPhase">現在の購買フェーズ</param>
    /// <returns>購買フェーズに応じた発話タイプ</returns>
    public abstract SpeechType ApplyAppropriateType(Phase currentPhase);

    public void ProvideSpecificContextForPrompt(BlackBoard blackBoard, Phase currentPhase, SpeechType sppechType, out string specificPrompt)
    {
        ProvideInfoForPrompt(blackBoard, out string situation);
        string phaseStr = ConvertPhaseToJapanese(currentPhase);
        string speechTypeStr = ConvertSpeechTypeToJapanese(sppechType);
        specificPrompt = $"{phaseStr}において、{situation}\nあなたは客に対して{speechTypeStr}を行ってください。";
    }

    private string ConvertPhaseToJapanese(Phase phase)
    {
        return phase switch
        {
            Phase.Enter => "入店フェーズ",
            Phase.Explore => "鑑賞フェーズ",
            Phase.Recommend => "推薦フェーズ",
            Phase.Purchase => "購入フェーズ",
            _ => "不明なフェーズ",
        };
    }

    private string ConvertSpeechTypeToJapanese(SpeechType type)
    {
        return type switch
        {
            SpeechType.InformationProvision => "情報提供",
            SpeechType.EmpathicPresentation => "感情支援",
            SpeechType.DecisionSupport => "意思決定支援",
            SpeechType.ExceptionError => "エラー",
            _ => "不明な発話タイプ",
        };
    }

    /// <summary>    
    /// LLMに提供するプロンプトのうち、状況を作成する。
    /// </summary>
    /// <param name="specificPrompt">発話用のプロンプト</param>
    public abstract void ProvideInfoForPrompt(BlackBoard blackBoard, out string specificPrompt);
}