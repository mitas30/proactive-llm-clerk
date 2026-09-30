using UnityEngine;
using BehaviorDesigner.Runtime;

/// <summary>
/// step2に対応する
/// 現在は、一定の探索時間が経過したら、自動的に次のステートへ遷移するようになっている。
/// </summary>
public class ExploreState : IFlowState
{
    public FlowManager Manager { get; set; }

    float elapsedTime;
    float exploreTime;
    private BlackBoard m_blackBoard;
    private CanvasGroup exploreUI;
    private BehaviorTree m_behaviorTree;

    public ExploreState(FlowManager manager, ExploreStateConfig config)
    {
        Manager = manager;
        exploreTime = config.ExploreTime;
        m_blackBoard = manager.BlackBoard;
        exploreUI = Manager.exploreUI.GetComponent<CanvasGroup>();
        m_behaviorTree = manager.BehaviorTree;
    }

    public void Enter()
    {
        elapsedTime = 0f;
        Debug.Log("ExploreState entered");
        m_blackBoard.CurrentPhase = Phase.Explore;
        exploreUI.alpha = 1f;
        exploreUI.interactable = true;
        exploreUI.blocksRaycasts = true;
        m_behaviorTree.EnableBehavior();
    }

    public void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= exploreTime)
        {
            Manager.TransitionTo(Manager.RecommendState);
        }
    }

    public void Exit()
    {
        exploreUI.alpha = 0f;
        exploreUI.interactable = false;
        exploreUI.blocksRaycasts = false;
        Debug.Log("ExploreState exited");
    }


}
