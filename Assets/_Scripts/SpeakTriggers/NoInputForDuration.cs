using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "NoInputForDuration", menuName = "ScriptableObjects/SpeakTriggers/NoInputForDuration")]
public sealed class NoInputForDuration : SpeakTrigger
{
    [SerializeField, Tooltip("無入力継続時間 t（秒）")] private float inactiveSeconds;
    [SerializeField, Tooltip("再発火までのクールダウン秒。エディタで設定してください")] private float retriggerCooldownSeconds;

    private float lastInputTime = 0f;
    private float lastTriggeredTime = -99999f;

    public override void ResetAllDynamicMember()
    {
        lastInputTime = Time.unscaledTime;
        lastTriggeredTime = -99999f;
    }

    public override void Tick(BlackBoard blackBoard)
    {
        // 入力検知（Keyboard + Mouse のみ）
        bool hasInput = false;
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.anyKey.wasPressedThisFrame) hasInput = true;

        var mouse = Mouse.current;
        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
                hasInput = true;
            if (!hasInput && mouse.delta.ReadValue() != Vector2.zero) hasInput = true;
            if (!hasInput && mouse.scroll.ReadValue() != Vector2.zero) hasInput = true;
        }

        if (hasInput)
        {
            lastInputTime = Time.unscaledTime;
        }
    }

    public override bool CanActivate(BlackBoard blackBoard)
    {
        bool timeExceeded = (Time.unscaledTime - lastInputTime) >= inactiveSeconds;
        bool cooldownOk = (Time.unscaledTime - lastTriggeredTime) >= retriggerCooldownSeconds;

        return timeExceeded && cooldownOk;
    }

    public override void OnActivated(BlackBoard blackBoard)
    {
        lastTriggeredTime = Time.unscaledTime;
    }

    public override SpeechType ApplyAppropriateType(Phase currentPhase)
    {
        return SpeechType.EmpathicPresentation;
    }

    public override void ProvideInfoForPrompt(BlackBoard blackBoard, out string specificPrompt)
    {
        specificPrompt = m_promptTemplate; // 固定文をインスペクタに設定
    }
}
