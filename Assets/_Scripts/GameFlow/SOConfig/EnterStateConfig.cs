using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

[CreateAssetMenu(fileName = "EnterStateConfig", menuName = "Flow/EnterStateConfig", order = 0)]
public class EnterStateConfig : ScriptableObject
{
    [Header("チュートリアル完了イベント")]
    [SerializeField, Tooltip("チュートリアル完了時に発火するイベント")] private GameEvent finishTutorialEvent;
    public GameEvent FinishTutorialEvent => finishTutorialEvent;
}
