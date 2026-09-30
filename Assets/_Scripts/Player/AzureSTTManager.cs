using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using UnityEngine;
using BehaviorDesigner.Runtime;

public class AzureSTTManager : MonoBehaviour
{
    [SerializeField] string SubscriptionKey;
    [SerializeField] string Region;

    [SerializeField] BlackBoard blackBoard;

    [Header("ログ")]
    [SerializeField] private BehaviorLog m_behaviorLog;

    [Header("Behavior Designer")]
    [SerializeField] Behavior aiCharacterBehavior;

    [Header("GameEvent")]
    [SerializeField] GameEvent startTalkEvent;
    [SerializeField] GameEvent cancelTalkEvent;
    [SerializeField] GameEvent completeTalkEvent;

    [SerializeField] TMPro.TMP_Text recognizedUIText;

    [Header("プロンプト整形")]
    [SerializeField] private PromptComposer m_promptComposer;

    SpeechRecognizer recognizer;
    readonly StringBuilder transcript = new();   // ★ 取得した全文を保持
    bool isListening = false;                    // ★ 二重スタート防止
    bool isFirstRecognize = true;
    ConcurrentQueue<string> resultQueue = new();

    private readonly object _threadLock = new object();
    private bool _isDisposing = false;

    // ――――――――― イベント登録 ――――――――― //
    void OnEnable()
    {
        startTalkEvent.OnPublished.AddListener(StartTalking);
        cancelTalkEvent.OnPublished.AddListener(CancelTalkingRecognition);
        completeTalkEvent.OnPublished.AddListener(CompleteTalkingRecognition);
    }
    void OnDisable()
    {
        startTalkEvent.OnPublished.RemoveListener(StartTalking);
        cancelTalkEvent.OnPublished.RemoveListener(CancelTalkingRecognition);
        completeTalkEvent.OnPublished.RemoveListener(CompleteTalkingRecognition);
        _ = SafeStopAndDisposeAsync();
    }

    void Update()
    {
        while (resultQueue.TryDequeue(out var text))
        {
            if (isFirstRecognize)
            {
                recognizedUIText.text = "";
                isFirstRecognize = false;
            }
            recognizedUIText.text += text + "\n";
        }
    }

    /// <summary>
    /// 録音を開始する
    /// </summary>
    public async void StartTalking()
    {
        if (isListening) return;
        isListening = true;
        isFirstRecognize = true;
        transcript.Clear();

        Debug.Log("ユーザの音声認識を開始します。");

        var config = SpeechConfig.FromSubscription(SubscriptionKey, Region);
        config.SpeechRecognitionLanguage = "ja-JP";
        var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        recognizer = new SpeechRecognizer(config, audioConfig);

        // まとまった文章を認識したときのイベント
        recognizer.Recognized += (_, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                transcript.Append(e.Result.Text).Append(' ');
                resultQueue.Enqueue(e.Result.Text);
            }
        };

        recognizer.Canceled += (_, e) =>
        {
            Debug.LogWarning($"Azure STT Canceled: {e.Reason} / {e.ErrorDetails}");
            // ネットワークや権限エラー時は自動で終了しないため手動解放
            _ = SafeStopAndDisposeAsync();
        };

        await recognizer.StartContinuousRecognitionAsync();
    }

    //音声入力の確定
    public async void CompleteTalkingRecognition()
    {
        string recogResult = transcript.ToString().Trim();
        await SafeStopAndDisposeAsync();

        if (m_behaviorLog != null && !string.IsNullOrWhiteSpace(recogResult))
        {
            m_behaviorLog.AddLog(BehaviorLog.BehaviorEventKind.UserRespondSpeech, recogResult);
        }

        var phase = blackBoard != null ? blackBoard.CurrentPhase : Phase.Enter;
        string passivePrompt = m_promptComposer != null
            ? m_promptComposer.PassiveCompose(recogResult, blackBoard, m_behaviorLog, phase)
            : recogResult;

        aiCharacterBehavior.SetVariableValue("userSpeechText", recogResult);
        aiCharacterBehavior.SetVariableValue("passivePrompt", passivePrompt);

        if (!string.IsNullOrEmpty(recogResult))
        {
            Debug.Log($"認識結果: {recogResult}");
            blackBoard.ShouldResponse = true;
        }
        else
        {
            Debug.Log("発話が認識されませんでした。");
        }

        // ? AIの発話禁止フラグを解除
        blackBoard.IsShutUp = false;
    }

    // ――――――――― 録音停止（破棄） ――――――――― //
    public void CancelTalkingRecognition()
    {
        if (!isListening) return;
        Debug.Log("発話をキャンセルし、内容を破棄しました。");
        _ = SafeStopAndDisposeAsync();
    }

    private async Task SafeStopAndDisposeAsync()
    {
        SpeechRecognizer recognizerToDispose = null;

        // lockブロック内で共有リソースの状態を安全に変更する
        lock (_threadLock)
        {
            // 既に他のスレッドが処理中、またはrecognizerが存在しない場合は何もしない
            if (_isDisposing || recognizer == null)
            {
                return;
            }

            _isDisposing = true; // これから処理を開始することをマーク

            // ローカル変数にインスタンスを退避させる
            recognizerToDispose = recognizer;

            // 共有変数は即座にnullにして、他のスレッドがアクセスできないようにする
            recognizer = null;
            isListening = false;
        }

        // lockブロックの外で、時間のかかる非同期処理を実行する
        // これにより、他のスレッドを長期間ブロックすることを防ぐ
        Debug.Log("Safely stopping speech recognition...");
        await recognizerToDispose.StopContinuousRecognitionAsync();
        recognizerToDispose.Dispose();
        Debug.Log("Speech recognition stopped and disposed.");

        // すべての処理が完了したら、フラグをリセットする
        _isDisposing = false;
        recognizedUIText.text = "";
    }
}