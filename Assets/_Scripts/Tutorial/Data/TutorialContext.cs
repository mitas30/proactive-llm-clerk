using BehaviorDesigner.Runtime;
using UnityEngine;
using TMPro;

/// <summary>
/// チュートリアル実行時に各ステップへ渡す参照をまとめたコンテキスト。
/// </summary>
public class TutorialContext
{
    public CanvasGroup[] TaskPanels { get; set; }
    public CanvasGroup CanvasGroupA { get; set; }
    public Transform PlayerTransform { get; set; }
    public GameEventInt ClickPaintingEvent { get; set; }
    public MonoBehaviour Host { get; set; } // コルーチン開始元
    public AzureTTSManager TtsManager { get; set; }
    public LlmManager LlmManager { get; set; }
    public ProactiveSpeechManager ProactiveSpeechManager { get; set; }
    public NPCFollow NPCFollow { get; set; }
    public BehaviorTree BehaviorTree { get; set; }
    public BlackBoard BlackBoard { get; set; }
    public DialogueUIView ClerkAITextUI { get; set; }
    public Animator ClerkAIAnimator { get; set; }
    public TextMeshProUGUI[] CandidateQuestionsText { get; set; }
}
