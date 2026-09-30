using UnityEngine;
using BehaviorDesigner.Runtime;

/// <summary>
/// step3 : 推薦フェーズを担当するState
/// 推薦が終わったら、購入フェーズに移行するような設計にする。
/// </summary>
public class RecommendState : IFlowState
{
    public FlowManager Manager { get; set; }
    private BlackBoard m_blackBoard;
    private float m_recommendDuration;
    float elapsedTime;
    private CanvasGroup recommendUI;
    private BehaviorTree m_behaviorTree;

    public RecommendState(FlowManager manager, RecommendStateConfig config)
    {
        Manager = manager;
        m_blackBoard = manager.BlackBoard;
        m_recommendDuration = config.recommendDuration;
        recommendUI = Manager.exploreUI.GetComponent<CanvasGroup>();
        m_behaviorTree = manager.BehaviorTree;
        m_behaviorTree.EnableBehavior();
    }

    public void Enter()
    {
        elapsedTime = 0f;
        m_blackBoard.CurrentPhase = Phase.Recommend;
        Debug.Log("RecommendState entered");
        recommendUI.alpha = 1f;
        recommendUI.interactable = true;
        recommendUI.blocksRaycasts = true;
    }

    public void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= m_recommendDuration)
        {
            Manager.TransitionTo(Manager.PurchaseState);
        }
    }

    public void Exit()
    {
        recommendUI.alpha = 0f;
        recommendUI.interactable = false;
        recommendUI.blocksRaycasts = false;
        Debug.Log("RecommendState exited");
    }
}
