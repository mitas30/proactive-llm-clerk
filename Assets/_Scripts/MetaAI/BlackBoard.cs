using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// btで利用する共有データ
/// </summary>
// ! ブラックボードは計算を持たないようにする。
public class BlackBoard : MonoBehaviour
{
    [Header("Painting Weights Settings")]
    [SerializeField, Tooltip("クリックカウントに対する重み")] private float m_clickMultiplier;
    [SerializeField, Tooltip("認識カウントに対する重み")] private float m_recognizeMultiplier;
    [SerializeField, Tooltip("注視カウントに対する重み")] private float m_stareMultiplier;

    [Header("Proactive Speech Ban Settings")]
    [SerializeField, Tooltip("能動発話を禁止する秒数（Editorから設定）")] private float m_proactivelyBanSeconds;

    /// <summary>
    /// 能動/受動に関わらず、AIが発話を控えるべき状態かどうか。
    /// </summary>
    public bool IsShutUp { get; set; }
    /// <summary>
    /// 受動的な発話応答をすべきか。
    /// </summary>
    public bool ShouldResponse { get; set; }
    /// <summary>
    /// 能動的に発話することを望んでいるか。
    /// </summary>
    [field: SerializeField] public bool WantToSpeakProactively { get; set; }
    public Phase CurrentPhase { get; set; }
    public bool IsQuit { get; set; }
    public float ClickMultiplier => m_clickMultiplier;
    public float RecognizeMultiplier => m_recognizeMultiplier;
    public float StareMultiplier => m_stareMultiplier;
    public int CurrentFocusedPaintingID { get; set; }
    public bool BanProactivelySpeech { get; set; }
    private Coroutine m_proactivelyBanCoroutine;
    public bool HasRespondUserSpeakingRunOnce { get; set; }

    public readonly Dictionary<int, PaintingInteractionLog> paintingsInterestScore = new Dictionary<int, PaintingInteractionLog>();
    void Awake()
    {
        IsShutUp = false;
        ShouldResponse = false;
        WantToSpeakProactively = false;
        CurrentPhase = Phase.Enter;
        CurrentFocusedPaintingID = -1;
        BanProactivelySpeech = false;
        HasRespondUserSpeakingRunOnce = false;
        IsQuit = false;
    }

    /// <summary>
    /// 能動発話禁止を t 秒間有効化する。実行中ならリセット。
    /// secondsOverride が指定されていればそれを優先、null の場合は Inspector の値を使用。
    /// 秒数 <= 0 の場合は禁止を即解除（フラグを false にしてタイマーなし）。
    /// </summary>
    public void TriggerProactivelyBan(float? secondsOverride = null)
    {
        var seconds = secondsOverride ?? m_proactivelyBanSeconds;
        // 進行中であれば止める（リセット挙動）
        if (m_proactivelyBanCoroutine != null)
        {
            StopCoroutine(m_proactivelyBanCoroutine);
            m_proactivelyBanCoroutine = null;
        }

        if (seconds <= 0f)
        {
            BanProactivelySpeech = false;
            return;
        }

        m_proactivelyBanCoroutine = StartCoroutine(ProactivelyBanTimer(seconds));
    }

    private IEnumerator ProactivelyBanTimer(float seconds)
    {
        BanProactivelySpeech = true;
        WantToSpeakProactively = false;
        // timeScale に影響されないように Realtime を使用
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }
        BanProactivelySpeech = false;
        Debug.Log($"[BlackBoard] Proactively speech ban ended after {seconds} seconds. currentTime={Time.time}s");
        m_proactivelyBanCoroutine = null;
    }
}
