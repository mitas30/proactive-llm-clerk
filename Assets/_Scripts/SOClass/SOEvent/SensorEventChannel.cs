using UnityEngine;
using UnityEngine.Events;

// メタAIに関係するイベント名の定義
public enum SensorEventType
{
    PlayerEntered,
    NotFinishedAnyTutorial,
    PaintingClicked,
    PaintingRecognized,
    PaintingStared,
}

[CreateAssetMenu(menuName = "Events/Game Event Channel")]
public class SensorEventChannel : ScriptableObject
{
    public UnityAction<SensorEventType, object[]> OnEventRaised;

    public void Raise(SensorEventType type, params object[] args)
    {
        OnEventRaised?.Invoke(type, args);
    }
}