using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime;
using TMPro;

public class TutorialController : MonoBehaviour
{
    [SerializeField, Tooltip("チュートリアルの各ステップで表示するパネル群")] private CanvasGroup[] m_taskPanels;
    [SerializeField, Tooltip("すべてのチュートリアルが終わったら発動")] private GameEvent m_finishTutorialEvent;
    [SerializeField, Tooltip("絵をクリックしたときに発動するイベント")] private GameEventInt m_clickPaintingEvent;
    [SerializeField, Tooltip("Azure TTS Manager")] private AzureTTSManager m_ttsManager;
    [SerializeField, Tooltip("NPC Follow コンポーネント")] private NPCFollow m_nPCFollow;
    [SerializeField, Tooltip("Behavior Tree コンポーネント")] private BehaviorTree m_behaviorTree;
    [SerializeField, Tooltip("BlackBoard コンポーネント")] private BlackBoard m_blackBoard;
    [SerializeField, Tooltip("店員の Dialogue UI コンポーネント")] private DialogueUIView m_clerkAITextUI;
    [SerializeField, Tooltip("店員の Animator コンポーネント")] private Animator m_clerkAIAnimator;
    [SerializeField] private LlmManager m_llmManager;
    [SerializeField] private ProactiveSpeechManager m_proactiveSpeechManager;
    [SerializeField, Tooltip("店員の次の質問候補を表示する TextMeshProUGUI 群（最大3つ想定）")] private TextMeshProUGUI[] m_candidateQuestionTexts;

    [SerializeField] private CanvasGroup m_canvasGroupA;

    [Header("データ駆動シーケンス（設定時はこれを採用）")]
    [SerializeField] private TutorialSequenceAsset m_sequenceAsset;

    public IEnumerator RunSequenceAsset()
    {
        // 事前に全パネル非表示
        if (m_taskPanels != null)
        {
            foreach (var cg in m_taskPanels)
                if (cg != null) cg.alpha = 0f;
        }

        var ctx = new TutorialContext
        {
            TaskPanels = m_taskPanels,
            PlayerTransform = GameObject.FindGameObjectWithTag("Player")?.transform,
            ClickPaintingEvent = m_clickPaintingEvent,
            Host = this,
            TtsManager = m_ttsManager,
            NPCFollow = m_nPCFollow,
            BehaviorTree = m_behaviorTree,
            BlackBoard = m_blackBoard,
            ClerkAITextUI = m_clerkAITextUI,
            ClerkAIAnimator = m_clerkAIAnimator,
            LlmManager = m_llmManager,
            ProactiveSpeechManager = m_proactiveSpeechManager,
            CandidateQuestionsText = m_candidateQuestionTexts,
            CanvasGroupA = m_canvasGroupA
        };

        yield return m_sequenceAsset.RunAll(ctx);

        m_finishTutorialEvent.Publish();
    }
}
