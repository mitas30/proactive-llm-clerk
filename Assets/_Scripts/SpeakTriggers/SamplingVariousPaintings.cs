using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SamplingVariousPaintings", menuName = "ScriptableObjects/SpeakTriggers/SamplingVariousPaintings")]
public sealed class SamplingVariousPaintings : SpeakTrigger
{
    [SerializeField, Tooltip("時間窓t（秒）")]
    private float timeWindowSeconds;
    [SerializeField, Tooltip("時間窓t内で加点された異なる絵の数X（distinct）")]
    private int minDistinctPaintingsX;
    [SerializeField, Tooltip("興味度がこの値以上増えたら「加点」とみなす。0の場合は増加があれば全てカウント")]
    private float minScoreIncreaseDelta;
    [SerializeField, Tooltip("再発火までのクールダウン秒。エディタで設定してください")]
    private float retriggerCooldownSeconds;

    private readonly Dictionary<int, float> lastScores = new Dictionary<int, float>();
    private readonly Queue<(int paintingID, float time)> incrementsQueue = new Queue<(int, float)>();
    private readonly Dictionary<int, int> countsInWindowByPainting = new Dictionary<int, int>();
    private int distinctPaintingsInWindow;
    private float lastTriggeredTime = -99999f;

    public override void ResetAllDynamicMember()
    {
        lastScores.Clear();
        incrementsQueue.Clear();
        countsInWindowByPainting.Clear();
        distinctPaintingsInWindow = 0;
        lastTriggeredTime = -99999f;
    }

    public override void Tick(BlackBoard blackBoard)
    {
        float now = Time.time;
        // 差分検出
        foreach (var kv in blackBoard.paintingsInterestScore)
        {
            int id = kv.Key;
            float current = kv.Value.CurrentScore;
            float prev = lastScores.TryGetValue(id, out float prevValue) ? prevValue : 0f;
            float delta = current - prev;
            // NOTE: CurrentScore は (Count * Multiplier) の合計。
            // Multiplier が 1 未満の場合、1回のイベント増分が 1.0 未満になりうるため、閾値は設定可能にしている。
            // また、lastScores 未登録のタイミングで初回加点が起きると取りこぼすため、prev=0 とみなして検出する。
            if (delta > 0f && (minScoreIncreaseDelta <= 0f || delta >= minScoreIncreaseDelta))
            {
                incrementsQueue.Enqueue((id, now));
                if (countsInWindowByPainting.TryGetValue(id, out int count))
                {
                    countsInWindowByPainting[id] = count + 1;
                }
                else
                {
                    countsInWindowByPainting[id] = 1;
                    distinctPaintingsInWindow++;
                }
            }
            lastScores[id] = current; // 常に更新（ban 中も）
        }

        // 古いイベントを捨てる
        while (incrementsQueue.Count > 0 && (now - incrementsQueue.Peek().time) > timeWindowSeconds)
        {
            var old = incrementsQueue.Dequeue();
            if (countsInWindowByPainting.TryGetValue(old.paintingID, out int count))
            {
                count--;
                if (count <= 0)
                {
                    countsInWindowByPainting.Remove(old.paintingID);
                    distinctPaintingsInWindow--;
                }
                else
                {
                    countsInWindowByPainting[old.paintingID] = count;
                }
            }
        }
    }

    public override bool CanActivate(BlackBoard blackBoard)
    {
        if (incrementsQueue.Count == 0) return false;

        float now = Time.time;
        bool reached = distinctPaintingsInWindow >= minDistinctPaintingsX;
        bool cooldownOk = (now - lastTriggeredTime) >= retriggerCooldownSeconds;
        return reached && cooldownOk;
    }

    public override void OnActivated(BlackBoard blackBoard)
    {
        lastTriggeredTime = Time.time;
    }

    public override SpeechType ApplyAppropriateType(Phase currentPhase)
    {
        if (currentPhase == Phase.Enter) return SpeechType.EmpathicPresentation;
        if (currentPhase == Phase.Explore) return SpeechType.InformationProvision;
        if (currentPhase == Phase.Recommend || currentPhase == Phase.Purchase) return SpeechType.DecisionSupport;
        return SpeechType.ExceptionError;
    }

    public override void ProvideInfoForPrompt(BlackBoard blackBoard, out string specificPrompt)
    {
        specificPrompt = m_promptTemplate; // 固定文をインスペクタに設定
    }
}
