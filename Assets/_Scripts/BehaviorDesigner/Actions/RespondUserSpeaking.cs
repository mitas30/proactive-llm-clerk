using UnityEngine;
using System.Collections;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TMPro;
using System;

public class RespondUserSpeaking : Action
{
    [Tooltip("ユーザーの発話内容を持つ変数")] public SharedString userSpeechText;
    [Tooltip("LLMに与えるプロンプトを持つ変数")] public SharedString promptText;
    [Tooltip("clerkAIのgameObjectの参照")] public SharedGameObject clerkAIGameObject;
    [Tooltip("ユーザのgameObjectの参照")] public SharedGameObject userGameObject;

    public DialogueUIView clerkAITextUI;
    [Tooltip("店員のAnimator")] public Animator m_animator;
    public TextMeshProUGUI[] m_candidateQuestionTexts;
    public BlackBoard blackBoard;
    [Tooltip("ログ出力（インスペクタで結合）")] public BehaviorLog behaviorLog;

    private bool isCompleted;

    // ? ノードに入った最初のフレームで実行される
    public override void OnStart()
    {
        isCompleted = false;
        Debug.Log($"ユーザーの発話を処理開始: 発話内容_{userSpeechText.Value}");
        // コルーチンを開始
        Owner.StartTaskCoroutine(this, "ProcessUserSpeech");
        blackBoard.ShouldResponse = false;
    }

    public override TaskStatus OnUpdate()
    {
        if (isCompleted)
        {
            return TaskStatus.Success;
        }

        // まだ処理中の場合はRunningを返す
        return TaskStatus.Running;
    }

    /// <summary>
    /// ユーザーの発話を処理するコルーチン
    /// </summary>
    /// <remarks>
    /// 参照は無いが、OnStartから呼び出されている
    /// </remarks>
    private IEnumerator ProcessUserSpeech()
    {
        var shopCustomer = userGameObject.Value;
        var clerkAI = clerkAIGameObject.Value;

        if (shopCustomer == null)
        {
            Debug.LogError("ShopCustomerが見つかりません");
            isCompleted = true;
            yield break;
        }

        var llmManager = clerkAI.GetComponent<LlmManager>();
        var ttsManager = clerkAI.GetComponent<AzureTTSManager>();

        if (llmManager == null || ttsManager == null)
        {
            Debug.LogError("必要なコンポーネントが見つかりません");
            isCompleted = true;
            yield break;
        }

        yield return StartCoroutine(GetGPTResponse(llmManager, userSpeechText.Value, promptText.Value));

        // この時点でgptReplyが設定されている
        if (!string.IsNullOrEmpty(gptReply))
        {
            m_animator.SetBool("IsSpeaking", true);
            clerkAITextUI.Show("店員", gptReply);

            behaviorLog?.AddLog(BehaviorLog.BehaviorEventKind.UserRespondSpeech,
                $"\"{gptReply}\"");
            yield return StartCoroutine(SpeakText(ttsManager, gptReply));
            behaviorLog?.AddLog(BehaviorLog.BehaviorEventKind.UserRespondSpeech, "done");

            m_animator.SetBool("IsSpeaking", false);
            clerkAITextUI.Hide();
        }
        blackBoard.TriggerProactivelyBan();
        blackBoard.HasRespondUserSpeakingRunOnce = true;
        isCompleted = true;
    }

    private string gptReply = string.Empty;

    /// <summary>
    /// GPTへのリクエストを行い、質問への応答文を取得するコルーチン
    /// </summary>
    /// <param name="llmManager"></param>
    /// <param name="prompt"></param>
    /// <returns></returns>
    private IEnumerator GetGPTResponse(LlmManager llmManager, string userSpeechText, string prompt)
    {
        var task = llmManager.ResponseGPTRequest(userSpeechText, prompt);

        // TaskがCompleteになるまで待つ
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.Exception != null)
        {
            Debug.LogError($"GPTリクエストエラー: {task.Exception}");
            gptReply = string.Empty;
        }
        else
        {
            gptReply = task.Result.response;
            var candidateQuestions = task.Result.candidateQuestions;
            for (int i = 0; i < Math.Min(candidateQuestions.Count, m_candidateQuestionTexts.Length); i++)
            {
                m_candidateQuestionTexts[i].text = candidateQuestions[i];
            }
        }
    }

    private IEnumerator SpeakText(AzureTTSManager ttsManager, string text)
    {
        var task = ttsManager.TextToSpeech(text);

        // TaskがCompleteになるまで待つ
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.Exception != null)
        {
            Debug.LogError($"TTSエラー: {task.Exception}");
            behaviorLog?.AddLog(BehaviorLog.BehaviorEventKind.UserRespondSpeech, "error TTS");
        }
    }

    public override void OnEnd()
    {
        // 必要に応じてクリーンアップ
        Owner.StopTaskCoroutine("ProcessUserSpeech");
    }
}

