using System.Collections;
using UnityEngine;

/// <summary>
/// 1つのチュートリアルステップを表す基底クラス。
/// 説明テキストと実行処理(コルーチン)を同居させ、単一情報源とする。
/// </summary>
public abstract class TutorialStepBase : ScriptableObject
{
    [SerializeField, Tooltip("UIに表示するステップ名（任意）")] private string m_title;
    [SerializeField, TextArea, Tooltip("このステップの説明（必須）")] private string m_description;
    [SerializeField, Tooltip("このステップで表示するCanvasGroup配列のインデックス。-1で非表示。")] private int m_panelIndex = -1;
    [SerializeField, Tooltip("このステップ開始時に現在のパネルをフェードインする秒数。0で即時表示。")] private float m_fadeInDuration = 0f;
    [SerializeField, Tooltip("このステップ完了時に現在のパネルをフェードアウトする秒数。0で即時非表示。")] private float m_fadeOutDuration = 0f;
    public string Title => m_title;
    public string Description => m_description;
    public int PanelIndex => m_panelIndex;
    public float FadeInDuration => m_fadeInDuration;
    public float FadeOutDuration => m_fadeOutDuration;

    /// <summary>
    /// ステップの本体処理。必要な参照は <see cref="TutorialContext"/> から取得する。
    /// </summary>
    public abstract IEnumerator Execute(TutorialContext ctx);

    protected void SetPanelAlpha(TutorialContext ctx, int index, float alpha)
    {
        if (ctx == null || ctx.TaskPanels == null) return;
        if (index < 0 || index >= ctx.TaskPanels.Length) return;
        var cg = ctx.TaskPanels[index];
        if (cg != null) cg.alpha = alpha;
    }
}
