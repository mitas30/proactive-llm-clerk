using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

/// <summary>
/// ユーザの発話用UIを非表示にする
/// </summary>
public class HideSpeechUI : Action
{
    [Tooltip("発話ボタンUIのgameObjectの参照")]
    public SharedGameObject shopAIGameObject;

    public override TaskStatus OnUpdate()
    {
        var canvasGroup = shopAIGameObject.Value.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        return TaskStatus.Success;
    }
}
