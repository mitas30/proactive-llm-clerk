using System;
using UnityEngine;
using UnityEngine.UI;
using BehaviorDesigner.Runtime;

/// <summary>
/// ゲーム進行のStateMachine
/// </summary>
public class FlowManager : MonoBehaviour
{
    public GameObject exploreUI;
    public GameObject purchaseUI;
    public TutorialController m_tutorialController;
    public Button m_purchaseButton;
    public GameCloser m_gameCloser;

    [Header("Debug Settings")]
    [Tooltip("ゲーム開始時のステートを指定します。")]
    [SerializeField] private Phase m_initialState;

    [Header("フェーズ用config")]
    [SerializeField] EnterStateConfig enterStateConfig;
    [SerializeField] ExploreStateConfig exploreStateConfig;
    [SerializeField] RecommendStateConfig recommendStateConfig;
    [SerializeField] PurchaseStateConfig purchaseStateConfig;

    [Header("現在のステート")]
    [SerializeReference] IFlowState currentState;

    [Header("Shared Context & Channels")]
    [SerializeField] private BlackBoard m_BlackBoard;

    [Header("ログ出力")]
    [SerializeField] private BehaviorLog m_behaviorLog;

    [Header("BT")]
    [SerializeField] private BehaviorTree m_behaviorTree;

    EnterState m_enterState;
    ExploreState m_exploreState;
    RecommendState m_recommendState;
    PurchaseState m_purchaseState;

    public ExploreState ExploreState => m_exploreState;
    public RecommendState RecommendState => m_recommendState;
    public PurchaseState PurchaseState => m_purchaseState;
    public BlackBoard BlackBoard => m_BlackBoard;
    public BehaviorTree BehaviorTree => m_behaviorTree;

    public event Action<Phase> PhaseChanged;

    public float ExploreDuration => exploreStateConfig != null ? exploreStateConfig.ExploreTime : 0f;
    public float RecommendDuration => recommendStateConfig != null ? recommendStateConfig.recommendDuration : 0f;
    public float PurchaseDuration => purchaseStateConfig != null ? purchaseStateConfig.purchaseDuration : 0f;
    public Phase CurrentPhase => m_BlackBoard != null ? m_BlackBoard.CurrentPhase : Phase.Init;

    void Awake()
    {
        // 各ステートのインスタンスを生成
        m_enterState = new EnterState(this, enterStateConfig);
        m_exploreState = new ExploreState(this, exploreStateConfig);
        m_recommendState = new RecommendState(this, recommendStateConfig);
        m_purchaseState = new PurchaseState(this, purchaseStateConfig);
    }

    /// <summary>ゲーム開始時に最初の State を設定する</summary>
    void Start()
    {
        switch (m_initialState)
        {
            case Phase.Enter:
                TransitionTo(m_enterState);
                break;
            case Phase.Explore:
                TransitionTo(m_exploreState);
                break;
            case Phase.Recommend:
                TransitionTo(m_recommendState);
                break;
            case Phase.Purchase:
                TransitionTo(m_purchaseState);
                break;
            default:
                Debug.LogWarning($"未対応の初期ステート: {m_initialState}。EnterStateから開始します。");
                TransitionTo(m_enterState);
                break;
        }
    }
    void Update()
    {
        currentState?.Update();
    }

    /// <summary>
    /// State から呼ばれる唯一の遷移 API
    /// </summary>
    /// <param name="nextState">遷移先の State インスタンス</param>
    public void TransitionTo(IFlowState nextState)
    {
        string fromName = currentState != null ? currentState.GetType().Name : "None";
        string toName = nextState != null ? nextState.GetType().Name : "None";

        currentState?.Exit();
        currentState = nextState;
        currentState.Enter();

        PhaseChanged?.Invoke(CurrentPhase);

        if (m_behaviorLog != null)
        {
            m_behaviorLog.AddLog(BehaviorLog.BehaviorEventKind.FlowTransition, $"{fromName} -> {toName}");
        }
    }
}