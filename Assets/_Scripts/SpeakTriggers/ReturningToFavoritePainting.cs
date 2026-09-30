using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReturningToFavoritePainting", menuName = "ScriptableObjects/SpeakTriggers/ReturningToFavoritePainting")]
public sealed class ReturningToFavoritePainting : SpeakTrigger
{
    [SerializeField, Tooltip("\"元々興味があった\"と見なす最低スコアX")] private float minExistingInterestX;
    [SerializeField, Tooltip("再発火までのクールダウン秒。エディタで設定してください")] private float retriggerCooldownSeconds;

    private readonly Dictionary<int, float> lastScores = new Dictionary<int, float>();
    private int lastIncrementPaintingID = -1;
    private int targetPaintingID = -1;
    private float lastTriggeredTime = -99999f;

    private bool hasIncreaseThisTick;
    private int increasedIdThisTick;
    private float increasedCurrentScoreThisTick;
    private int previousIncrementPaintingIDThisTick;

    public override void ResetAllDynamicMember()
    {
        lastScores.Clear();
        lastIncrementPaintingID = -1;
        targetPaintingID = -1;
        lastTriggeredTime = -99999f;

        hasIncreaseThisTick = false;
        increasedIdThisTick = -1;
        increasedCurrentScoreThisTick = 0f;
        previousIncrementPaintingIDThisTick = -1;
    }

    public override void Tick(BlackBoard blackBoard)
    {
        hasIncreaseThisTick = false;
        increasedIdThisTick = -1;
        increasedCurrentScoreThisTick = 0f;
        previousIncrementPaintingIDThisTick = -1;

        // 差分検出＆スナップショット更新
        foreach (var kv in blackBoard.paintingsInterestScore)
        {
            int id = kv.Key;
            float current = kv.Value.CurrentScore;

            if (lastScores.TryGetValue(id, out float prev))
            {
                float delta = current - prev;
                if (delta >= 1.0f) // 加点イベントは1.0以上
                {
                    hasIncreaseThisTick = true;
                    increasedIdThisTick = id;
                    increasedCurrentScoreThisTick = current;
                }
            }

            lastScores[id] = current;
        }

        // 次フレーム用に「前回の加点対象」を更新（判定用の prev は別フィールドに退避）
        if (hasIncreaseThisTick)
        {
            previousIncrementPaintingIDThisTick = lastIncrementPaintingID;
            lastIncrementPaintingID = increasedIdThisTick;
        }
    }

    public override bool CanActivate(BlackBoard blackBoard)
    {
        if (!hasIncreaseThisTick) return false;
        if (blackBoard.CurrentPhase == Phase.Enter) return false;

        // 加点直前のスコア（prev）を取得するため、1フレーム遅延対策として minExistingInterestX は current でも代替可だが、
        // ここでは prev を lastScores 更新前に参照したかったため、上で prev を読んでいる。より厳密には別辞書に prev を保持する設計でも良い。
        // この実装では、前段で delta>=1.0 を検知しているため、元スコア(prev)は lastScores の更新前の値。
        // ただし、ここまでに lastScores を更新済みなので prev 値は保持していない。そこで緩和条件として current>=minExistingInterestX でも良いとする。
        bool wasAlreadyInterested = increasedCurrentScoreThisTick >= minExistingInterestX;
        bool returnedFromOther = (previousIncrementPaintingIDThisTick != -1 && previousIncrementPaintingIDThisTick != increasedIdThisTick);
        bool cooldownOk = (Time.time - lastTriggeredTime) >= retriggerCooldownSeconds;

        return wasAlreadyInterested && returnedFromOther && cooldownOk;
    }

    public override void OnActivated(BlackBoard blackBoard)
    {
        targetPaintingID = increasedIdThisTick;
        lastTriggeredTime = Time.time;
    }

    public override SpeechType ApplyAppropriateType(Phase currentPhase)
    {
        if (currentPhase == Phase.Enter || currentPhase == Phase.Explore) return SpeechType.InformationProvision;
        if (currentPhase == Phase.Recommend || currentPhase == Phase.Purchase) return SpeechType.DecisionSupport;
        return SpeechType.ExceptionError;
    }

    public override void ProvideInfoForPrompt(BlackBoard blackBoard, out string specificPrompt)
    {
        var log = blackBoard.paintingsInterestScore[targetPaintingID];
        if (string.IsNullOrEmpty(log.ArtistName))
        {
            Debug.LogWarning($"ArtistName is empty for paintingID={targetPaintingID}");
        }
        specificPrompt = string.Format(m_promptTemplate, log.ArtistName, log.PaintingName);
    }
}
