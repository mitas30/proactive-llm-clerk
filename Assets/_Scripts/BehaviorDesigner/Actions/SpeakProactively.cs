using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using TMPro;
using System;

using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using Action = BehaviorDesigner.Runtime.Tasks.Action;


public class SpeakProactively : Action
{
    [Tooltip("能動発話用のプロンプト（エディタから入力）")]
    public SharedString proactivePrompt;

    [Tooltip("clerkAI の GameObject 参照（Llm/TTS を持つ想定）")]
    public SharedGameObject clerkAIGameObject;

    public DialogueUIView clerkAITextUI;

    [Tooltip("店員の Animator")]
    public Animator m_animator;

    [Tooltip("次の質問候補を表示する TextMeshProUGUI 群（最大3つ想定）")]
    public TextMeshProUGUI[] m_candidateQuestionTexts;

    public BlackBoard blackBoard;

    [Tooltip("ログ出力（インスペクタで結合）")]
    public BehaviorLog behaviorLog;

    private bool isCompleted;
    private string aiResponse = string.Empty;

    // ノードに入った最初のフレームで実行
    public override void OnStart()
    {
        isCompleted = false;
        Owner.StartTaskCoroutine(this, "ProcessProactiveSpeech");
        blackBoard.WantToSpeakProactively = false;
    }

    public override TaskStatus OnUpdate()
    {
        return isCompleted ? TaskStatus.Success : TaskStatus.Running;
    }

    /// <summary>
    /// 店員が能動発話を生成して話すコルーチン
    /// </summary>
    private IEnumerator ProcessProactiveSpeech()
    {
        var clerkAI = clerkAIGameObject != null ? clerkAIGameObject.Value : null;
        if (clerkAI == null)
        {
            Debug.LogError("clerkAIGameObject が設定されていません");
            isCompleted = true;
            yield break;
        }

        var llmManager = clerkAI.GetComponent<LlmManager>();
        var ttsManager = clerkAI.GetComponent<AzureTTSManager>();

        if (llmManager == null || ttsManager == null)
        {
            Debug.LogError("必要なコンポーネントが見つかりません (LlmManager / AzureTTSManager)");
            isCompleted = true;
            yield break;
        }

        yield return StartCoroutine(GetProactiveGPTResponse(llmManager, proactivePrompt.Value));

        if (!string.IsNullOrEmpty(aiResponse))
        {
            if (m_animator != null) m_animator.SetBool("IsSpeaking", true);
            if (clerkAITextUI != null) clerkAITextUI.Show("店員", aiResponse);

            // 音声合成して発話
            behaviorLog?.AddLog(BehaviorLog.BehaviorEventKind.ProactiveSpeech,
                $"\"{aiResponse}\"");
            yield return StartCoroutine(SpeakText(ttsManager, aiResponse));
            behaviorLog?.AddLog(BehaviorLog.BehaviorEventKind.ProactiveSpeech, "proactive speech done");
            if (m_animator != null) m_animator.SetBool("IsSpeaking", false);
            if (clerkAITextUI != null) clerkAITextUI.Hide();
        }
        blackBoard.TriggerProactivelyBan();
        isCompleted = true;
    }

    private IEnumerator GetProactiveGPTResponse(LlmManager llmManager, string prompt)
    {
        var task = llmManager.ProactiveGPTRequest(prompt);

        // Task 完了まで待機
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.Exception != null)
        {
            Debug.LogError($"GPT リクエストエラー: {task.Exception}");
            aiResponse = string.Empty;
        }
        else
        {
            aiResponse = task.Result.response;
            var candidateQuestions = task.Result.candidateQuestions;

            if (m_candidateQuestionTexts != null && candidateQuestions != null)
            {
                for (int i = 0; i < Math.Min(candidateQuestions.Count, m_candidateQuestionTexts.Length); i++)
                {
                    if (m_candidateQuestionTexts[i] != null)
                    {
                        m_candidateQuestionTexts[i].text = candidateQuestions[i];
                    }
                }
            }
        }
    }

    private IEnumerator SpeakText(AzureTTSManager ttsManager, string text)
    {
        var task = ttsManager.TextToSpeech(text);

        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.Exception != null)
        {
            Debug.LogError($"TTS エラー: {task.Exception}");
            behaviorLog?.AddLog(BehaviorLog.BehaviorEventKind.ProactiveSpeech, "error TTS");
        }
    }

    public override void OnEnd()
    {
        // コルーチンのクリーンアップ
        Owner.StopTaskCoroutine("ProcessProactiveSpeech");
    }
}
