using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// チュートリアルの順序付きコレクション。
/// </summary>
[CreateAssetMenu(menuName = "Tutorial/Sequence Asset", fileName = "TutorialSequence")]
public class TutorialSequenceAsset : ScriptableObject
{
    [SerializeField]
    private List<TutorialStepBase> m_steps = new List<TutorialStepBase>();

    public IReadOnlyList<TutorialStepBase> Steps => m_steps;

    /// <summary>
    /// すべてのステップ説明を結合して返す。
    /// </summary>
    public string BuildSummary(string separator = "\n", bool includeIndexAndTitle = false)
    {
        if (m_steps == null || m_steps.Count == 0) return string.Empty;
        var list = new List<string>(m_steps.Count);
        for (int i = 0; i < m_steps.Count; i++)
        {
            var s = m_steps[i];
            if (s == null) continue;
            var desc = s.Description ?? string.Empty;
            if (string.IsNullOrWhiteSpace(desc)) continue;
            if (includeIndexAndTitle)
            {
                var head = $"{i + 1}. ";
                if (!string.IsNullOrWhiteSpace(s.Title)) head += s.Title + ": ";
                list.Add(head + desc.Trim());
            }
            else
            {
                list.Add(desc.Trim());
            }
        }
        return string.Join(separator, list);
    }

    /// <summary>
    /// 連続実行するコルーチンを提供。
    /// </summary>
    public IEnumerator RunAll(TutorialContext ctx)
    {
        if (m_steps == null) yield break;
        foreach (var step in m_steps)
        {
            if (step == null) continue;
            // 表示すべきパネルをフェードインまたは即時表示
            if (ctx != null && ctx.TaskPanels != null && step.PanelIndex >= 0 && step.PanelIndex < ctx.TaskPanels.Length)
            {
                var cg = ctx.TaskPanels[step.PanelIndex];
                if (cg != null)
                {
                    float inDur = Mathf.Max(0f, step.FadeInDuration);
                    if (inDur <= 0f)
                    {
                        cg.alpha = 1f;
                    }
                    else
                    {
                        // 現在値から1へ
                        yield return FadeCanvasGroup(cg, cg.alpha, 1f, inDur);
                    }
                }
            }

            yield return step.Execute(ctx);

            if (ctx != null && ctx.TaskPanels != null && step.PanelIndex >= 0 && step.PanelIndex < ctx.TaskPanels.Length)
            {
                var cg = ctx.TaskPanels[step.PanelIndex];
                if (cg != null)
                {
                    float dur = Mathf.Max(0f, step.FadeOutDuration);
                    if (dur <= 0f)
                    {
                        cg.alpha = 0f;
                    }
                    else
                    {
                        yield return FadeCanvasGroup(cg, cg.alpha, 0f, dur);
                    }
                }
            }
        }
    }

    private static IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;
        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            float r = Mathf.Clamp01(t / duration);
            cg.alpha = Mathf.Lerp(from, to, r);
            yield return null;
        }
        cg.alpha = to;
    }
}
