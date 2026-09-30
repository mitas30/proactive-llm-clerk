using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class ForbiddenCursorOnDisabled : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("対象ボタン（未指定なら自動取得）")]
    [SerializeField] private Button targetButton;

    [Header("禁止カーソル画像（TextureType=Cursor 推奨）")]
    [SerializeField] private Texture2D forbiddenCursor;

    [SerializeField] private Vector2 hotspot = new Vector2(16, 16);
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private bool cursorApplied;

    private void Reset()
    {
        // アタッチ先が Button なら自動で拾う
        if (!targetButton) targetButton = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || targetButton == null) return;

        // このボタンが無効のときだけカーソル差し替え
        if (!targetButton.interactable)
        {
            Cursor.SetCursor(forbiddenCursor, hotspot, cursorMode);
            cursorApplied = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ホバー解除で必ず元に戻す
        if (cursorApplied)
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
            cursorApplied = false;
        }
    }

    private void OnDisable()
    {
        // オブジェクトが無効化/破棄されたときの取りこぼし対策
        if (cursorApplied)
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
            cursorApplied = false;
        }
    }

    private void Update()
    {
        // ホバー中に外部で interactable が true に戻った場合のフォロー
        if (cursorApplied && targetButton != null && targetButton.interactable)
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
            cursorApplied = false;
        }
    }
}
