using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 単純にTTSで発話し、発話が終わったら完了するステップ。
/// </summary>
[CreateAssetMenu(menuName = "Tutorial/Steps/SpeakOnly", fileName = "SpeakOnlyStep")]
public class SpeakOnlyStep : TutorialStepBase
{
    [SerializeField, TextArea, Tooltip("ステップ開始時にTTSで読み上げるテキスト。空なら発話しません（即完了）。")]
    private string m_speechText = "ご案内いたします。画面の指示に従って進めてください。";

    public override IEnumerator Execute(TutorialContext ctx)
    {
        ctx.BlackBoard.IsShutUp = true;

        Task speakTask = Task.CompletedTask;
        AzureTTSManager tts = ctx.TtsManager;

        if (!string.IsNullOrWhiteSpace(m_speechText) && tts != null)
        {
            try
            {
                speakTask = tts.TextToSpeech(m_speechText);

                if (ctx?.ClerkAIAnimator != null)
                {
                    ctx.ClerkAIAnimator.SetBool("IsSpeaking", true);
                }
                if (ctx?.ClerkAITextUI != null)
                {
                    ctx.ClerkAITextUI.Show("店員", m_speechText);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"SpeakOnlyStep: TTS 起動に失敗しました。{ex.Message}");
                speakTask = Task.CompletedTask; // 失敗時は即完了へ
            }
        }

        // 発話完了を待機
        while (!speakTask.IsCompleted)
        {
            yield return null;
        }

        //ctx.BlackBoard.IsShutUp = false;
        if (ctx?.ClerkAIAnimator != null)
        {
            ctx.ClerkAIAnimator.SetBool("IsSpeaking", false);
        }
        if (ctx?.ClerkAITextUI != null)
        {
            ctx.ClerkAITextUI.Hide();
        }
    }
}
