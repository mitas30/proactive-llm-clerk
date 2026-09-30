using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/GameEventString")]
public class GameEventString : ScriptableObject
{
    [SerializeField] UnityEvent<string> OnPublished;
    public void Publish(string s_arg)
    {
        OnPublished?.Invoke(s_arg);
    }

    /// <summary>
    /// リスナー登録
    /// </summary>
    public void AddListener(UnityAction<string> listener)
    {
        OnPublished.AddListener(listener);
    }

    /// <summary>
    /// リスナー解除
    /// </summary>
    public void RemoveListener(UnityAction<string> listener)
    {
        OnPublished.RemoveListener(listener);
    }
}
