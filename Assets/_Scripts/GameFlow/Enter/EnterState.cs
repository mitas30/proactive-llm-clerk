using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnterState : IFlowState
{
    public FlowManager Manager { get; set; }
    GameEvent m_finishTutorial;
    private TutorialController tutorialController;
    private BlackBoard m_blackBoard;

    public EnterState(FlowManager manager, EnterStateConfig config)
    {
        Manager = manager;
        m_finishTutorial = config.FinishTutorialEvent;
        m_blackBoard = manager.BlackBoard;
        tutorialController = manager.m_tutorialController;
    }

    public void Enter()
    {
        m_finishTutorial.AddListener(OnFinishTutorial);
        m_blackBoard.CurrentPhase = Phase.Enter;
        Manager.StartCoroutine(tutorialController.RunSequenceAsset());
    }

    public void Update() { return; }

    public void Exit()
    {
        Debug.Log("EnterState exited");
        m_finishTutorial.RemoveListener(OnFinishTutorial);
    }

    void OnFinishTutorial()
    {
        Manager.TransitionTo(Manager.ExploreState);
    }
}
