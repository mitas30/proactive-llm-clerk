using UnityEngine;

[System.Serializable]
public class PaintingInteractionLog
{
    public int PaintingID;
    public string PaintingName;
    public string ArtistName;
    public int ClickCount = 0;
    public int RecognizeCount = 0;
    public int StareCount = 0;
    public float CurrentScore = 0f;

    public PaintingInteractionLog(int paintingId, string paintingName, string artistName)
    {
        this.PaintingID = paintingId;
        this.PaintingName = paintingName;
        this.ArtistName = artistName;
    }
}

[CreateAssetMenu(fileName = "PaintingData", menuName = "ScriptableObjects/PaintingDataStruct")]
public sealed class PaintingDataStruct : ScriptableObject
{
    [SerializeField, Tooltip("1から数え始める")] int paintingID;
    [SerializeField] string paintingName;
    [SerializeField] string artistName;
    [SerializeField, Tooltip("この絵画の説明"), TextArea(3, 6)] string infoText;
    [Header("制作年")]
    [SerializeField, Tooltip("開始年 (単年の場合はこの年のみを使用)")] int yearOfProduction;
    [SerializeField, Tooltip("終了年 (未設定や単年の場合は 0 のまま)")] int endYearOfProduction = 0;

    // ? サイズ情報に合わせて、実際の絵画のサイズを設定する
    // ? エディタ操作側に見せるためであって、実際のゲーム内では使用しない
    [Header("サイズ情報 cm")]
    [SerializeField] float width;
    [SerializeField] float height;

    [Header("展示エリア")]
    [SerializeField, Tooltip("この絵画が展示されているエリアの名前")] string exhibitionAreaName;

    [Header("追加情報")]
    [SerializeField, Tooltip("プロンプトに入れる絵画の追加情報"), TextArea(3, 10)] string additionalInfomation;

    // --- Public Properties ---
    public int ID => paintingID;
    public string Name => paintingName;
    public string ArtistName => artistName;
    public string Artist => artistName;
    public string InfoText => infoText;
    public string AdditionalInformation => additionalInfomation;
    public string ProductionYearLabel
    {
        get
        {
            // 未設定
            if (yearOfProduction <= 0 && endYearOfProduction <= 0) return "制作年不明";
            // 終了年未設定 or 同一年 → 単年表示
            if (endYearOfProduction <= 0 || endYearOfProduction == yearOfProduction) return yearOfProduction > 0 ? yearOfProduction.ToString() : "制作年不明";
            return $"{yearOfProduction}-{endYearOfProduction}";
        }
    }
    public string ExhibitionAreaName => exhibitionAreaName;
    public string ExhibitionAreaCode
    {
        get
        {
            if (string.IsNullOrWhiteSpace(exhibitionAreaName)) return null;
            return exhibitionAreaName.Trim().ToUpperInvariant();
        }
    }
}