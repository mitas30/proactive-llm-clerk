using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Steps/SimpleSpeech", fileName = "SimpleSpeechStep")]
public class SimpleSpeechStep : TutorialStepBase
{
    [SerializeField, TextArea, Tooltip("ステップ開始時にTTSで読み上げるテキスト。インスペクタで設定してください。")] private string m_speechText = "こんにちは。次に進む準備ができたらお知らせします。";

    public override IEnumerator Execute(TutorialContext ctx)
    {
        // 話す前に CanvasGroupA を無効化
        ctx.CanvasGroupA.alpha = 0f;
        ctx.CanvasGroupA.interactable = false;
        ctx.CanvasGroupA.blocksRaycasts = false;
        ctx.BlackBoard.IsShutUp = true;

        // 発話開始
        Task speakTask = ctx.TtsManager.TextToSpeech(m_speechText);
        ctx.ClerkAIAnimator.SetBool("IsSpeaking", true);
        ctx.ClerkAITextUI.Show("店員", m_speechText);

        // 発話完了を待機
        while (!speakTask.IsCompleted)
        {
            yield return null;
        }

        // 発話終了後の後処理
        ctx.ClerkAIAnimator.SetBool("IsSpeaking", false);
        ctx.ClerkAITextUI.Hide();
        ctx.BlackBoard.IsShutUp = false;
        ctx.BlackBoard.TriggerProactivelyBan();

        // CanvasGroupA を有効化
        ctx.CanvasGroupA.alpha = 1f;
        ctx.CanvasGroupA.interactable = true;
        ctx.CanvasGroupA.blocksRaycasts = true;
    }
}
