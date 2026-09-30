using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// プロジェクト内の全てのPaintingDataStructアセットを一元管理し、
/// ランタイムでの高速な検索機能とContextの初期化機能を提供する。
/// </summary>
[RequireComponent(typeof(BlackBoard))]
public class AllPaintingsDatabase : MonoBehaviour
{
    [SerializeField, Tooltip("シーン内に飾る予定の全てのPaintingDataStructアセットをここに登録して")] private List<PaintingDataStruct> allPaintings;
    private Dictionary<int, PaintingDataStruct> _lookup;
    private bool _isInitialized;

    private BlackBoard m_blackBoard;

    private void Awake()
    {
        m_blackBoard = GetComponent<BlackBoard>();
    }

    void Start()
    {
        InitializeContext();
    }
    /// <summary>
    /// 指定されたIDを持つPaintingDataStructを返します。
    /// </summary>
    public PaintingDataStruct GetPaintingByID(int id)
    {
        InitializeIfNeeded();
        if (_lookup != null && _lookup.TryGetValue(id, out var painting))
        {
            return painting;
        }
        return null;
    }

    /// <summary>
    /// BlackBoardのPaintingLogsをこのデータベースの内容で初期化します。
    /// </summary>
    public void InitializeContext()
    {
        _isInitialized = false;
        _lookup = null;
        InitializeIfNeeded();
        InitializePaintingLogs();
        Debug.Log($"[AllPaintingsDatabase] DecisionContextが {allPaintings.Count} 件の絵画データで初期化されました。");
    }

    // 全ての絵画データを基に、インタラクションログを初期化するメソッド
    // これにより、AIは未インタラクションの絵画の存在も最初から認識できます。
    public void InitializePaintingLogs()
    {
        m_blackBoard.paintingsInterestScore.Clear();
        if (allPaintings == null) return;

        foreach (var painting in allPaintings)
        {
            if (painting != null && !m_blackBoard.paintingsInterestScore.ContainsKey(painting.ID))
            {
                var log = new PaintingInteractionLog(painting.ID, painting.Name, painting.ArtistName);
                m_blackBoard.paintingsInterestScore.Add(painting.ID, log);
            }
        }
    }

    /// <summary>
    /// 全絵画の概要（画家名と作品名のみ）を人間が読みやすい文字列として返します。
    /// </summary>
    public string GetOverviewText()
    {
        InitializeIfNeeded();
        var sb = new StringBuilder(256);

        int count = allPaintings != null ? allPaintings.Count : 0;
        sb.AppendLine($"— 絵画一覧（全{count}点）—");

        if (count == 0) return sb.ToString().TrimEnd();

        foreach (var p in allPaintings)
        {
            if (p == null)
            {
                sb.AppendLine("• 不明な画家『無題』");
                continue;
            }

            string artist = string.IsNullOrWhiteSpace(p.ArtistName) ? "不明な画家" : p.ArtistName.Trim();
            string title = string.IsNullOrWhiteSpace(p.Name) ? "無題" : p.Name.Trim();
            sb.AppendLine($"• {artist}『{title}』");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// 全絵画の詳細（画家名、作品名、年、説明、様式/主題/色彩）を人間が読みやすい文字列として返します。
    /// 使わないほうがいいと思う
    /// </summary>
    public string GetDetailedText()
    {
        InitializeIfNeeded();
        var sb = new StringBuilder(512);

        int count = allPaintings != null ? allPaintings.Count : 0;
        sb.AppendLine($"— 絵画詳細（全{count}点）— エリアの情報は必要に応じて参照してください。");

        if (count == 0) return sb.ToString().TrimEnd();

        for (int i = 0; i < allPaintings.Count; i++)
        {
            var p = allPaintings[i];

            string artist = p == null || string.IsNullOrWhiteSpace(p.ArtistName) ? "不明な画家" : p.ArtistName.Trim();
            string title = p == null || string.IsNullOrWhiteSpace(p.Name) ? "無題" : p.Name.Trim();
            string yearText = p != null ? p.ProductionYearLabel : "制作年不明";
            string areaCode = p != null ? p.ExhibitionAreaCode : null;

            // 1行目: タイトル — 画家（年）
            sb.AppendLine($"{title} — {artist}（{yearText}）");

            // 2行目: エリア情報
            sb.AppendLine($"  - エリア{areaCode}");

            // 3行目: 追加説明
            string info = p != null ? p.AdditionalInformation : null;
            if (!string.IsNullOrWhiteSpace(info))
            {
                sb.AppendLine($"  - {info.Trim()}");
            }

            // 作品間の空行（最後は不要だがTrimEndするのでOK）
            if (i < allPaintings.Count - 1)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString().TrimEnd();
    }

    private void InitializeIfNeeded()
    {
        if (_isInitialized) return;

        if (allPaintings == null) allPaintings = new List<PaintingDataStruct>();

        try
        {
            _lookup = allPaintings.ToDictionary(p => p.ID);
            _isInitialized = true;
        }
        catch (System.ArgumentException e)
        {
            Debug.LogError($"[AllPaintingsDatabase] 絵画IDに重複があります。データベースを確認してください。エラー: {e.Message}", this);
        }
    }
}
