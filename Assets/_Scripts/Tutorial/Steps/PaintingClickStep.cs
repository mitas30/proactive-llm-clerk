using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

[CreateAssetMenu(menuName = "Tutorial/Steps/PaintingClick", fileName = "PaintingClickStep")]
public class PaintingClickStep : TutorialStepBase
{
    [SerializeField, TextArea, Tooltip("ステップ開始時にTTSで読み上げるテキスト。空なら発話しません。")]
    private string m_speechText = "いらっしゃいませ、気になる絵画を1つ見つけて、その絵画をクリックしてみてください。";

    public override IEnumerator Execute(TutorialContext ctx)
    {
        bool clicked = false;
        void OnClick(int _) => clicked = true;
        ctx.BehaviorTree.EnableBehavior();
        ctx.BlackBoard.IsShutUp = true;

        // 1) クリック受け付けは即開始
        if (ctx.ClickPaintingEvent != null)
        {
            ctx.ClickPaintingEvent.AddListener(OnClick);
        }
        else
        {
            Debug.LogWarning("PaintingClickStep: ClickPaintingEvent が null です。クリックを待機できません。");
        }

        Task speakTask = Task.CompletedTask;
        AzureTTSManager tts = ctx.TtsManager;

        if (tts != null)
        {
            try
            {
                speakTask = tts.TextToSpeech(m_speechText);
                ctx.ClerkAIAnimator.SetBool("IsSpeaking", true);
                ctx.ClerkAITextUI.Show("店員", m_speechText);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"PaintingClickStep: TTS 起動に失敗しました。{ex.Message}");
                speakTask = Task.CompletedTask; // 失敗時は発話待ちをスキップ
            }
        }

        // 3) 発話完了 かつ クリック完了 の両方を待つ
        //    ただし、発話が終わったら即座に IsShutUp を解除する
        while (!(clicked && speakTask.IsCompleted))
        {
            if (speakTask.IsCompleted)
            {
                ctx.BlackBoard.IsShutUp = false;
                ctx.ClerkAIAnimator.SetBool("IsSpeaking", false);
                ctx.ClerkAITextUI.Hide();
                ctx.BlackBoard.TriggerProactivelyBan();
            }
            yield return null;
        }

        // 4) 後始末
        if (ctx.ClickPaintingEvent != null) ctx.ClickPaintingEvent.RemoveListener(OnClick);
    }
}
