using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecommendStateConfig", menuName = "Flow/RecommendStateConfig", order = 2)]
public class RecommendStateConfig : ScriptableObject
{
    [Tooltip("推薦フェーズの時間")] public float recommendDuration = 180f;
}
