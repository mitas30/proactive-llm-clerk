using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "ExploreStateConfig", menuName = "Flow/ExploreStateConfig", order = 1)]
public class ExploreStateConfig : ScriptableObject
{
    [SerializeField] float exploreTime = 180f;
    [SerializeField] SensorEventChannel sensorEventChannel;

    public float ExploreTime => exploreTime;
    public SensorEventChannel SensorEventChannel => sensorEventChannel;
}
