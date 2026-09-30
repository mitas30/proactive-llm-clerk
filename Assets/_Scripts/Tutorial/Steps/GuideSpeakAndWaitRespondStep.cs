using System.Collections;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Tutorial/Steps/GuideSpeakAndWaitRespond", fileName = "GuideSpeakAndWaitRespondStep")]
public class GuideSpeakAndWaitRespondStep : TutorialStepBase
{
    [SerializeField, TextArea(3, 6), Tooltip("LLMに渡すspecificPrompt（インスペクタ編集）。Logsと組み合わせたプロンプトを生成します。")]
    private string m_promptText = "";

    [SerializeField, TextArea(5, 6), Tooltip("店員が実際に発話するテキスト（インスペクタ編集）。LLMのresponseは使いません。")]
    private string m_speechText = "";

    [SerializeField, Tooltip("このステップで表示するCanvasGroup配列のインデックス。-1で非表示。")] private int m_middlePanelIndex = -1;

    public override IEnumerator Execute(TutorialContext ctx)
    {
        if (ctx == null)
        {
            Debug.LogWarning("GuideSpeakAndWaitRespondStep: ctx is null");
            yield break;
        }

        var bb = ctx.BlackBoard;
        var tts = ctx.TtsManager;
        var llm = ctx.LlmManager;
        var proactive = ctx.ProactiveSpeechManager;
        var ui = ctx.ClerkAITextUI;
        var anim = ctx.ClerkAIAnimator;
        var panel = ctx.TaskPanels;

        // 前処理: 無条件で黙らせる
        if (bb != null) bb.IsShutUp = true;

        // ? LLM の response は使わず、m_speechText を TTS で発話
        if (anim != null) anim.SetBool("IsSpeaking", true);
        if (ui != null) ui.Show("店員", m_speechText);

        System.Threading.Tasks.Task speak = null;
        if (tts != null && !string.IsNullOrEmpty(m_speechText))
        {
            try
            {
                speak = tts.TextToSpeech(m_speechText);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"GuideSpeakAndWaitRespondStep: TTS start failed. {ex.Message}");
            }
        }

        // 1) specificPrompt + Logs でプロンプト構築し、ProactiveGPTRequest を実行
        string prompt = m_promptText;
        if (proactive != null)
        {
            prompt = proactive.ComposeProactivePromptFromSpecific(m_promptText);
        }

        System.Threading.Tasks.Task<(string response, System.Collections.Generic.List<string> candidateQuestions)> task = null;
        if (llm != null)
        {
            try
            {
                // ? ユーザの想定質問(1ターン目)がほしいので、質問応答ではなく、能動発話用の関数を使っている
                task = llm.ProactiveGPTRequest(prompt);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"GuideSpeakAndWaitRespondStep: ProactiveGPTRequest start failed. {ex.Message}");
            }
        }

        if (task != null)
        {
            while (!task.IsCompleted)
                yield return null;

            if (task.Exception != null)
            {
                Debug.LogWarning($"GuideSpeakAndWaitRespondStep: ProactiveGPTRequest error. {task.Exception}");
            }
            else
            {
                var candidateQuestions = task.Result.candidateQuestions;
                if (ctx.CandidateQuestionsText != null && candidateQuestions != null)
                {
                    for (int i = 0; i < Math.Min(candidateQuestions.Count, ctx.CandidateQuestionsText.Length); i++)
                    {
                        if (ctx.CandidateQuestionsText[i] != null)
                        {
                            ctx.CandidateQuestionsText[i].text = candidateQuestions[i];
                        }
                    }
                }
            }
        }

        if (speak != null)
        {
            while (!speak.IsCompleted)
                yield return null;
            if (speak.Exception != null)
            {
                Debug.LogWarning($"GuideSpeakAndWaitRespondStep: TTS error. {speak.Exception}");
            }
        }

        if (anim != null) anim.SetBool("IsSpeaking", false);
        if (ui != null) ui.Hide();
        bb.TriggerProactivelyBan();
        if (bb != null) bb.IsShutUp = false;
        panel[m_middlePanelIndex].alpha = 1f;

        // 3) CanvasGroup A を On
        var cgA = ctx.CanvasGroupA;
        if (cgA != null)
        {
            cgA.alpha = 1f;
            cgA.interactable = true;
            cgA.blocksRaycasts = true;
        }
        else
        {
            Debug.LogWarning("GuideSpeakAndWaitRespondStep: CanvasGroupA is null");
        }

        // 4) RespondUserSpeaking が1回実行されるまで待機（フラグは前処理でリセットしない）
        if (bb != null)
        {
            while (!bb.HasRespondUserSpeakingRunOnce)
                yield return null;
        }
        panel[m_middlePanelIndex].alpha = 0f;
    }
}
