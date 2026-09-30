using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PreferringAnArtist", menuName = "ScriptableObjects/SpeakTriggers/PreferringAnArtist")]
public sealed class PreferringAnArtist : SpeakTrigger
{
    [SerializeField, Tooltip("画家Aの合計興味度の最低値X")] private float minTotalInterestX;
    [SerializeField, Tooltip("2番手の画家に対する優位倍率Y（second==0でもX超なら成立）")] private float dominanceMultiplierY;
    [SerializeField, Tooltip("再発火までのクールダウン秒。エディタで設定してください")] private float retriggerCooldownSeconds;

    private string lastDominantArtist = null;
    private float lastTriggeredTime = -99999f;
    private string candidateDominantArtist = null;

    public override void ResetAllDynamicMember()
    {
        lastDominantArtist = null;
        lastTriggeredTime = -99999f;
        candidateDominantArtist = null;
    }

    public override bool CanActivate(BlackBoard blackBoard)
    {
        if (blackBoard.CurrentPhase == Phase.Enter) return false;

        // 集計
        var sumByArtist = new Dictionary<string, float>();
        foreach (var kv in blackBoard.paintingsInterestScore)
        {
            var log = kv.Value;
            string artist = log.ArtistName;
            if (string.IsNullOrEmpty(artist))
            {
                Debug.LogWarning($"ArtistName is empty for paintingID={kv.Key}");
                continue; // 集計対象外
            }
            if (!sumByArtist.ContainsKey(artist)) sumByArtist[artist] = 0f;
            sumByArtist[artist] += log.CurrentScore;
        }

        if (sumByArtist.Count == 0) return false;

        // トップと2位を求める
        string topArtist = null; float topSum = -1f;
        string secondArtist = null; float secondSum = -1f;
        foreach (var kv in sumByArtist)
        {
            if (kv.Value > topSum)
            {
                secondArtist = topArtist; secondSum = topSum;
                topArtist = kv.Key; topSum = kv.Value;
            }
            else if (kv.Value > secondSum)
            {
                secondArtist = kv.Key; secondSum = kv.Value;
            }
        }

        if (topArtist == null) return false;

        // CanActivate 内では候補として保持するだけ（確定は OnActivated で）
        candidateDominantArtist = topArtist;

        bool meetsX = topSum >= minTotalInterestX;
        bool dominance;
        if (secondSum <= 0f)
        {
            dominance = meetsX; // 2位が0でもX超なら成立
        }
        else
        {
            dominance = topSum >= dominanceMultiplierY * secondSum;
        }

        bool cooldownOk = (Time.time - lastTriggeredTime) >= retriggerCooldownSeconds;

        // 判定フレームで注視している絵が、トップアーティストの作品であることを要求する
        bool focusedIsTopArtist = false;
        int focusedPaintingId = blackBoard.CurrentFocusedPaintingID;
        if (blackBoard.paintingsInterestScore.TryGetValue(focusedPaintingId, out var focusedLog))
        {
            string focusedArtist = focusedLog.ArtistName;
            focusedIsTopArtist = !string.IsNullOrEmpty(focusedArtist) && focusedArtist == topArtist;
        }

        return meetsX && dominance && cooldownOk && focusedIsTopArtist;
    }

    public override void OnActivated(BlackBoard blackBoard)
    {
        lastDominantArtist = candidateDominantArtist;
        lastTriggeredTime = Time.time;
    }

    public override SpeechType ApplyAppropriateType(Phase currentPhase)
    {
        if (currentPhase == Phase.Explore) return SpeechType.InformationProvision;
        if (currentPhase == Phase.Recommend) return SpeechType.DecisionSupport;
        if (currentPhase == Phase.Purchase) return SpeechType.EmpathicPresentation;
        return SpeechType.ExceptionError;
    }

    public override void ProvideInfoForPrompt(BlackBoard blackBoard, out string specificPrompt)
    {
        string artist = lastDominantArtist ?? "";
        if (string.IsNullOrEmpty(artist))
        {
            Debug.LogWarning("PreferringAnArtist: lastDominantArtist is empty at ProvideInfoForPrompt");
        }
        specificPrompt = string.Format(m_promptTemplate, artist);
    }
}
