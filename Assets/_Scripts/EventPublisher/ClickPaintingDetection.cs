using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Collider))]
public class ClickPaintingDetection : MonoBehaviour
{
    private PaintingInfoPanelView m_infoPanelView;

    [Header("絵画データ")]
    [SerializeField, Tooltip("絵画のデータを保持する ScriptableObject")]
    private PaintingDataStruct m_paintingData;

    public PaintingDataStruct PaintingData => m_paintingData;

    [Header("SOイベント")]
    [SerializeField, Tooltip("クリック時に発火するイベント")]
    private GameEventInt m_clickDetectionEvent;

    // ? 絵画の数が多いので、動的に要素を取得するような設計にしている
    void Start()
    {
        // タグから PaintingInfoPanel を取得
        GameObject infoPanelObject = GameObject.FindWithTag("PaintingInfoPanel");
        if (infoPanelObject == null)
        {
            Debug.LogError($"[{nameof(ClickPaintingDetection)}] タグ 'PaintingInfoPanel' のオブジェクトが見つかりません。");
            return;
        }

        m_infoPanelView = infoPanelObject.GetComponent<PaintingInfoPanelView>();
        if (m_infoPanelView == null)
        {
            Debug.LogError($"[{nameof(ClickPaintingDetection)}] PaintingInfoPanel に PaintingInfoPanelView コンポーネントが見つかりません。");
            return;
        }
        m_infoPanelView.Hide();
    }

    /// <summary>
    /// ユーザがこのオブジェクトをクリックしたときに呼ばれる
    /// </summary>
    private void OnMouseDown()
    {
        // UI要素が重なっている場合は処理を停止
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (m_paintingData == null)
        {
            Debug.LogError($"[{nameof(ClickPaintingDetection)}] PaintingData が null です。");
            return;
        }

        int paintingID = m_paintingData.ID;
        m_clickDetectionEvent.Publish(paintingID);

        ShowInfoPanel(m_paintingData.Name, m_paintingData.Artist, m_paintingData.ProductionYearLabel, m_paintingData.InfoText);
    }

    /// <summary>
    /// 情報パネルを表示し、テキストをセットする
    /// </summary>
    /// <param name="paintingName">表示する絵画名</param>
    /// <param name="artist">表示する作家名</param>
    /// <param name="year">表示する制作年</param>
    /// <param name="paintingInfo">表示する絵画の説明</param>
    private void ShowInfoPanel(string paintingName, string artist, string year, string paintingInfo)
    {
        m_infoPanelView.UpdateInfo(paintingName, artist, year, paintingInfo);
        m_infoPanelView.Show();
    }
}
