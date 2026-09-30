using UnityEngine;

[DisallowMultipleComponent]
public class PaintingInfoPanelAutoHideOnGazeLost : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private PaintingInfoPanelView m_panelView;
    [SerializeField, Tooltip("視線ヒット時に発火する GameEventInt（例: GazePaintingDetection の gazeEvent）")]
    private GameEventInt m_rawGazePaintingEvent;
    [SerializeField, Tooltip("絵をクリックしたときに発火する GameEventInt（例: ClickPaintingDetection の m_clickDetectionEvent）\n未設定の場合は『表示中の絵ID』を追跡できないため、ID変化による Hide は行いません")]
    private GameEventInt m_clickPaintingEvent;

    [Header("挙動")]
    [SerializeField, Tooltip("視線入力がこの秒数来なければ Hide を実行します")]
    private float m_hideDelaySec = 1.5f;
    private bool m_requireFirstGaze = true;
    private bool m_hideWhenGazePaintingIdDiffersFromDisplayed = true;

    private float m_lastGazeReceivedTime = float.NegativeInfinity;
    private bool m_hasReceivedGaze;
    private bool m_hideTriggered;
    private int m_displayedPaintingId = -1;

    private void Awake()
    {
        if (m_panelView == null)
        {
            m_panelView = GetComponent<PaintingInfoPanelView>();
        }

        if (m_panelView == null)
        {
            GameObject infoPanelObject = GameObject.FindWithTag("PaintingInfoPanel");
            if (infoPanelObject != null)
            {
                m_panelView = infoPanelObject.GetComponent<PaintingInfoPanelView>();
            }
        }

        if (m_panelView == null)
        {
            Debug.LogError($"[{nameof(PaintingInfoPanelAutoHideOnGazeLost)}] {nameof(PaintingInfoPanelView)} が見つかりません。", this);
        }
    }

    private void OnEnable()
    {
        if (m_rawGazePaintingEvent == null)
        {
            Debug.LogError($"[{nameof(PaintingInfoPanelAutoHideOnGazeLost)}] 視線イベント（GameEventInt）が未設定です。", this);
            return;
        }

        m_rawGazePaintingEvent.AddListener(OnRawGaze);

        if (m_clickPaintingEvent != null)
        {
            m_clickPaintingEvent.AddListener(OnPaintingClicked);
        }
    }

    private void OnDisable()
    {
        if (m_rawGazePaintingEvent != null)
        {
            m_rawGazePaintingEvent.RemoveListener(OnRawGaze);
        }

        if (m_clickPaintingEvent != null)
        {
            m_clickPaintingEvent.RemoveListener(OnPaintingClicked);
        }
    }

    private void Update()
    {
        if (m_hideDelaySec <= 0f) return;
        if (m_hideTriggered) return;
        if (m_requireFirstGaze && !m_hasReceivedGaze) return;
        if (m_panelView == null) return;

        if (Time.time - m_lastGazeReceivedTime >= m_hideDelaySec)
        {
            m_panelView.Hide();
            m_hideTriggered = true;
        }
    }

    private void OnRawGaze(int paintingId)
    {
        m_lastGazeReceivedTime = Time.time;
        m_hasReceivedGaze = true;

        if (m_hideWhenGazePaintingIdDiffersFromDisplayed && m_displayedPaintingId >= 0 && paintingId != m_displayedPaintingId)
        {
            if (m_panelView != null)
            {
                m_panelView.Hide();
            }
            m_hideTriggered = true;
            return;
        }

        m_hideTriggered = false;
    }

    private void OnPaintingClicked(int paintingId)
    {
        m_displayedPaintingId = paintingId;

        // クリック直後にタイマー起因で即 Hide されないようにリセット
        m_lastGazeReceivedTime = Time.time;
        m_hideTriggered = false;
    }
}
