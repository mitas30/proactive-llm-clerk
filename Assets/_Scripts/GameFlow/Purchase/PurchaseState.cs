using UnityEngine;
using BehaviorDesigner.Runtime;

/// <summary>
/// step4 : 購入フェーズを担当する State
/// </summary>
public class PurchaseState : IFlowState
{
    public FlowManager Manager { get; set; }
    private BlackBoard m_blackBoard;
    private CanvasGroup purchaseUI;
    private float m_purchaseDuration;
    float elapsedTime;
    private BehaviorTree m_behaviorTree;
    private bool tFlag;

    public PurchaseState(FlowManager manager, PurchaseStateConfig config)
    {
        Manager = manager;
        m_blackBoard = manager.BlackBoard;
        purchaseUI = Manager.purchaseUI.GetComponent<CanvasGroup>();
        m_purchaseDuration = config.purchaseDuration;
        m_behaviorTree = manager.BehaviorTree;
    }

    public void Enter()
    {
        Debug.Log("PurchaseState entered");
        elapsedTime = 0f;
        m_blackBoard.CurrentPhase = Phase.Purchase;
        purchaseUI.alpha = 1f;
        purchaseUI.interactable = true;
        purchaseUI.blocksRaycasts = true;
        Manager.m_purchaseButton.interactable = true;
        m_behaviorTree.EnableBehavior();
        tFlag = false;
    }

    public void Update()
    {
        elapsedTime += Time.deltaTime;
        if (!tFlag && elapsedTime >= m_purchaseDuration)
        {
            Exit();
            tFlag = true;
        }
    }

    public void Exit()
    {
        Debug.Log("体験終了");
        // ステート終了時にもゲーム終了フロー（スピーチ→終了）を実行
        var closer = Manager.m_gameCloser;
        if (closer != null)
        {
            closer.RequestQuitFromState();
        }
        else
        {
            Debug.LogWarning("[PurchaseState] GameCloser が見つからなかったため、終了処理を呼び出せませんでした。");
        }
    }
}
