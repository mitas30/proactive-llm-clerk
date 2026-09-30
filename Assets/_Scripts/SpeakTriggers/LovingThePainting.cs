using UnityEngine;

[CreateAssetMenu(fileName = "KeepLookingAtSamePainting", menuName = "ScriptableObjects/SpeakTriggers/LovingThePainting")]
public sealed class LovingThePainting : SpeakTrigger
{
    [SerializeField, Tooltip("連続して見ている時好きとみなすスコアの閾値")] float loveThreshold;
    [SerializeField, Tooltip("再発火までのクールダウン秒。エディタで設定してください")] float retriggerCooldownSeconds;
    private int currentFocusedPaintingID;
    private float beginingLoveScore;
    private float lastTriggeredTime;
    private bool thresholdExceededThisTick;

    public override void ResetAllDynamicMember()
    {
        currentFocusedPaintingID = -1;
        beginingLoveScore = 0f;
        lastTriggeredTime = -99999f;
        thresholdExceededThisTick = false;
    }

    public override void Tick(BlackBoard blackBoard)
    {
        thresholdExceededThisTick = false;

        // 注視中の絵なし
        if (blackBoard.CurrentFocusedPaintingID == -1)
        {
            currentFocusedPaintingID = -1;
            beginingLoveScore = 0f;
            return;
        }

        if (blackBoard.CurrentFocusedPaintingID != -1 && currentFocusedPaintingID != blackBoard.CurrentFocusedPaintingID)
        {
            currentFocusedPaintingID = blackBoard.CurrentFocusedPaintingID;
            beginingLoveScore = blackBoard.paintingsInterestScore[currentFocusedPaintingID].CurrentScore;
        }

        float currentScore = blackBoard.paintingsInterestScore[currentFocusedPaintingID].CurrentScore;
        if (currentScore - beginingLoveScore > loveThreshold)
        {
            thresholdExceededThisTick = true;
        }
    }

    public override bool CanActivate(BlackBoard blackBoard)
    {
        if (!thresholdExceededThisTick) return false;
        bool cooldownOk = (Time.time - lastTriggeredTime) >= retriggerCooldownSeconds;
        return cooldownOk;
    }

    public override void OnActivated(BlackBoard blackBoard)
    {
        lastTriggeredTime = Time.time;
        beginingLoveScore = blackBoard.paintingsInterestScore[currentFocusedPaintingID].CurrentScore;
    }

    public override SpeechType ApplyAppropriateType(Phase currentPhase)
    {
        if (currentPhase == Phase.Enter) return SpeechType.EmpathicPresentation;
        else if (currentPhase == Phase.Explore || currentPhase == Phase.Recommend) return SpeechType.InformationProvision;
        else if (currentPhase == Phase.Purchase) return SpeechType.DecisionSupport;
        else
        {
            Debug.LogError("LovingThePainting trigger is applied in inappropriate phase.");
            return SpeechType.ExceptionError;
        }
    }

    public override void ProvideInfoForPrompt(BlackBoard blackBoard, out string specificPrompt)
    {
        string artist = blackBoard.paintingsInterestScore[currentFocusedPaintingID].ArtistName;
        string paintingName = blackBoard.paintingsInterestScore[currentFocusedPaintingID].PaintingName;
        specificPrompt = string.Format(m_promptTemplate, artist, paintingName);
    }
}
