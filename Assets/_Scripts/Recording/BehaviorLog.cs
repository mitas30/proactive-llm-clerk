using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン内常駐の軽量ログ蓄積クラス。インスペクタ参照経由で他クラスから呼び出す。
/// ・AddLog(kind, message) で追記
/// ・GetLogsAsText(kinds) で指定種別のテキストを古い順に取得
/// ・終了時にTXTで保存（persistentDataPath）
/// </summary>
public class BehaviorLog : MonoBehaviour
{
    public enum BehaviorEventKind
    {
        PaintingInteraction,
        ProactiveTrigger,
        FlowTransition,
        UserRespondSpeech,
        ProactiveSpeech
    }

    [Serializable]
    private struct BehaviorEntry
    {
        public DateTime timestamp;
        public BehaviorEventKind kind;
        public string message;
    }

    [Header("保存設定")]
    [SerializeField] private bool m_SaveOnQuit;
    [SerializeField] private string m_FilePrefix;
    [SerializeField, Tooltip("ログ内の長文をこの長さでスニペット化（0で無制限）")] private int m_TextSnippetLength;

    private readonly List<BehaviorEntry> _entries = new List<BehaviorEntry>();
    private DateTime _sessionStart;
    private string _sceneName = "Scene";
    private string _filePath = string.Empty;

    private void Awake()
    {
        _sessionStart = DateTime.Now;
        _sceneName = SceneManager.GetActiveScene().name;
        _filePath = BuildFilePath();
    }

    private string BuildFilePath()
    {
        var time = _sessionStart.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{m_FilePrefix}_{_sceneName}_{time}.txt";
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    /// <summary>
    /// ログを1件追加する。
    /// </summary>
    /// <remarks>
    /// メッセージは改行をスペースに変換し、長すぎる場合はスニペット化される。
    /// </remarks>
    public void AddLog(BehaviorEventKind kind, string message)
    {
        var entry = new BehaviorEntry
        {
            timestamp = DateTime.Now,
            kind = kind,
            message = OneLine(message, m_TextSnippetLength)
        };
        _entries.Add(entry);
    }

    /// <summary>
    /// 指定種別のログ（未指定なら全件）を古い順に1つのテキストにして返す。
    /// </summary>
    public string GetLogsAsText(params BehaviorEventKind[] kinds)
    {
        IEnumerable<BehaviorEntry> query = _entries;
        if (kinds != null && kinds.Length > 0)
        {
            var set = new HashSet<BehaviorEventKind>(kinds);
            query = query.Where(e => set.Contains(e.kind));
        }

        var sb = new StringBuilder();
        foreach (var e in query.OrderBy(e => e.timestamp))
        {
            sb.AppendLine(FormatEntry(e));
        }
        return sb.ToString();
    }

    private static string FormatEntry(BehaviorEntry e)
    {
        return $"[{e.timestamp:HH:mm:ss.fff}] {e.kind}: {e.message}";
    }

    private static string OneLine(string text, int maxLen)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var normalized = Regex.Replace(text, "\\s+", " ");
        if (maxLen > 0 && normalized.Length > maxLen)
        {
            return normalized.Substring(0, maxLen) + "...";
        }
        return normalized;
    }

    private void OnDisable()
    {
        if (m_SaveOnQuit) SaveToFileSafe();
    }

    private void OnApplicationQuit()
    {
        if (m_SaveOnQuit) SaveToFileSafe();
    }

    /// <summary>
    /// 例外を飲み込んでセーフに保存。
    /// </summary>
    public void SaveToFileSafe()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(_filePath, GetLogsAsText());
            Debug.Log($"BehaviorLog saved: {_filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"BehaviorLog save failed: {ex}");
        }
    }

    public int Count => _entries.Count;
}
