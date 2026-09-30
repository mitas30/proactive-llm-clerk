using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

public class ShowSpeechUI : Action
{
    [Tooltip("発話ボタンUIのgameObjectの参照")]
    public SharedGameObject shopAIGameObject;

    public override TaskStatus OnUpdate()
    {
        var canvasGroup = shopAIGameObject.Value.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        return TaskStatus.Success;
    }
}
