using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 整数引数付きのゲームイベントを表す ScriptableObject
/// </summary>
[CreateAssetMenu(menuName = "Events/GameEventInt")]
public class GameEventInt : ScriptableObject
{
    /// <summary>
    /// イベント購読時に呼び出される UnityEvent。int 引数付き。
    /// </summary>
    [SerializeField] UnityEvent<int> m_OnPublished;

    /// <summary>
    /// イベントを発行するメソッド
    /// </summary>
    /// <param name="value">イベント引数として渡す整数値</param>
    public void Publish(int value)
    {
        m_OnPublished?.Invoke(value);
    }

    /// <summary>
    /// リスナー登録
    /// </summary>
    public void AddListener(UnityAction<int> listener)
    {
        m_OnPublished.AddListener(listener);
    }

    /// <summary>
    /// リスナー解除
    /// </summary>
    public void RemoveListener(UnityAction<int> listener)
    {
        m_OnPublished.RemoveListener(listener);
    }
}
