using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

/// <summary>
/// quitMessage を話してからアプリを終了する Behavior Designer Action。
/// 実行中は Running を返し、TTS完了後に終了する。
/// </summary>
public class QuitWithSpeechAction : Action
{
    [Tooltip("発話内容（Behavior の SharedString 変数をバインド）")]
    public SharedString quitMessage;

    [Tooltip("店員の GameObject（AzureTTSManager を持つ）")]
    public SharedGameObject clerkAIGameObject;

    public DialogueUIView clerkAITextUI;

    [Tooltip("店員の Animator（IsSpeaking を使用）")]
    public Animator animator;

    [Tooltip("BlackBoard（任意：IsShutUp 制御）")]
    public BlackBoard blackBoard;

    [Tooltip("スピーチ中に無効化する CanvasGroup（任意）")]
    public CanvasGroup canvasGroupToDisable;

    private bool isCompleted;

    public override void OnStart()
    {
        isCompleted = false;
        Owner.StartTaskCoroutine(this, nameof(RunQuitFlow));
    }

    public override TaskStatus OnUpdate()
    {
        return isCompleted ? TaskStatus.Success : TaskStatus.Running;
    }

    private IEnumerator RunQuitFlow()
    {
        var message = (quitMessage != null && !string.IsNullOrWhiteSpace(quitMessage.Value))
            ? quitMessage.Value
            : "ご利用ありがとうございました。体験を終了します。";

        var clerk = clerkAIGameObject != null ? clerkAIGameObject.Value : null;
        var tts = clerk != null ? clerk.GetComponent<AzureTTSManager>() : null;

        if (canvasGroupToDisable != null)
        {
            canvasGroupToDisable.alpha = 0f;
            canvasGroupToDisable.interactable = false;
            canvasGroupToDisable.blocksRaycasts = false;
        }
        bool prevShutUp = false;
        if (blackBoard != null)
        {
            prevShutUp = blackBoard.IsShutUp;
            blackBoard.IsShutUp = true;
        }

        if (animator != null) animator.SetBool("IsSpeaking", true);
        if (clerkAITextUI != null) clerkAITextUI.Show("店員", message);

        if (tts == null)
        {
            Debug.LogWarning("[QuitWithSpeechAction] AzureTTSManager が見つからないため、即終了します。");
            yield return null;
            DoQuit();
            isCompleted = true;
            yield break;
        }

        var task = tts.TextToSpeech(message);
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (animator != null) animator.SetBool("IsSpeaking", false);
        if (clerkAITextUI != null) clerkAITextUI.Hide();

        if (blackBoard != null) blackBoard.IsShutUp = prevShutUp;

        DoQuit();
        isCompleted = true;
    }

    public override void OnEnd()
    {
        Owner.StopTaskCoroutine(nameof(RunQuitFlow));
    }

    private void DoQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
