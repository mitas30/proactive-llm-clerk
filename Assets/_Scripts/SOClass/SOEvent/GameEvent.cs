using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// パブリッシャーとサブスクライバーの間のイベントを管理するクラス
/// 実際は、このクラスから作成した.assetファイルを使用する
/// このクラスは、引数なしの関数のみを登録できるイベントクラス
/// </summary>
[CreateAssetMenu(menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    //TODO: 本当は、OnPublishedはprivateにして、Publishメソッドを通じてのみ呼び出せるようにするべき。
    public UnityEvent OnPublished;
    public void Publish()
    {
        OnPublished?.Invoke();
    }

    /// <summary>
    /// リスナー登録
    /// </summary>
    public void AddListener(UnityAction listener)
    {
        OnPublished.AddListener(listener);
    }

    /// <summary>
    /// リスナー解除
    /// </summary>
    public void RemoveListener(UnityAction listener)
    {
        OnPublished.RemoveListener(listener);
    }
}
