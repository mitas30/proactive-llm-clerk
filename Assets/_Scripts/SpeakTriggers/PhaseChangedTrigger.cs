using UnityEngine;

[CreateAssetMenu(fileName = "PhaseChangedTrigger", menuName = "ScriptableObjects/SpeakTriggers/PhaseChangedTrigger")]
public sealed class PhaseChangedTrigger : SpeakTrigger
{
    [SerializeField, Tooltip("再発火までのクールダウン秒。エディタで設定してください")] private float retriggerCooldownSeconds;

    private Phase? lastPhase;
    private float lastTriggeredTime = -99999f;
    private bool phaseChangedThisTick;

    public override void ResetAllDynamicMember()
    {
        lastPhase = null;
        lastTriggeredTime = -99999f;
        phaseChangedThisTick = false;
    }

    public override void Tick(BlackBoard blackBoard)
    {
        phaseChangedThisTick = false;

        var current = blackBoard.CurrentPhase;
        if (lastPhase == null)
        {
            lastPhase = current;
            return;
        }

        // 仕様維持：Enter は発火対象外。ただし lastPhase は更新して追跡を継続。
        bool changed = current != lastPhase.Value;
        lastPhase = current;

        if (blackBoard.CurrentPhase == Phase.Enter) return;

        phaseChangedThisTick = changed;
    }

    public override bool CanActivate(BlackBoard blackBoard)
    {
        if (!phaseChangedThisTick) return false;
        bool cooldownOk = (Time.time - lastTriggeredTime) >= retriggerCooldownSeconds;
        return cooldownOk;
    }

    public override void OnActivated(BlackBoard blackBoard)
    {
        lastTriggeredTime = Time.time;
    }

    public override SpeechType ApplyAppropriateType(Phase currentPhase)
    {
        if (currentPhase == Phase.Explore) return SpeechType.InformationProvision;
        if (currentPhase == Phase.Recommend || currentPhase == Phase.Purchase) return SpeechType.DecisionSupport;
        return SpeechType.ExceptionError;
    }

    public override void ProvideInfoForPrompt(BlackBoard blackBoard, out string specificPrompt)
    {
        string phaseStr = "";
        // SpeakTrigger のヘルパを使うと二重表現になるが、ユーザの意図通り維持。
        switch (blackBoard.CurrentPhase)
        {
            case Phase.Explore: phaseStr = "鑑賞フェーズ"; break;
            case Phase.Recommend: phaseStr = "推薦フェーズ"; break;
            case Phase.Purchase: phaseStr = "購入フェーズ"; break;
            default: phaseStr = "不明なフェーズ"; break;
        }
        specificPrompt = string.Format(m_promptTemplate, phaseStr);
    }
}
