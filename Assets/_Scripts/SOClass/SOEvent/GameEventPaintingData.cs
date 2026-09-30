using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/GameEventPaintingData")]
public class GameEventPaintingData : ScriptableObject
{
    [Header("絵画データを引数に持つイベント")]
    [SerializeField]
    UnityEvent<PaintingDataStruct> m_OnPublished;

    /// <summary>
    /// 絵画データをリスナーに通知
    /// </summary>
    public void Publish(PaintingDataStruct paintingData)
    {
        m_OnPublished?.Invoke(paintingData);
    }

    /// <summary>
    /// リスナー登録
    /// </summary>
    public void AddListener(UnityAction<PaintingDataStruct> listener)
    {
        m_OnPublished.AddListener(listener);
    }

    /// <summary>
    /// リスナー解除
    /// </summary>
    public void RemoveListener(UnityAction<PaintingDataStruct> listener)
    {
        m_OnPublished.RemoveListener(listener);
    }
}
