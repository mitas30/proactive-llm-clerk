using UnityEngine;

/// <summary>
/// パブリッシャーのクラス
/// </summary>
public class KeyPressPublisher : MonoBehaviour
{
    [SerializeField] KeyCode key;
    [SerializeField] GameEvent igniteEvent;
    [SerializeField, Tooltip("このcanvasGroupが無効化されたときに、このキーも無効化される")] CanvasGroup canvasGroup;

    void Update()
    {
        // CanvasGroupが有効で、alpha > 0、かつinteractableがtrueの場合のみキー入力を受け付ける
        if (canvasGroup != null)
        {
            if (canvasGroup.alpha > 0 && Input.GetKeyDown(key))
            {
                igniteEvent.Publish();
            }
        }
        else
        {
            Debug.LogWarning("CanvasGroup component not found on " + gameObject.name);
        }
    }
}
