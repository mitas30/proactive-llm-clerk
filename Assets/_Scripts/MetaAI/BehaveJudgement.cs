using UnityEngine;

/// <summary>
/// BehaveJudgementはクリックと凝視の生入力を検知し、blackboardに対して絵画の興味スコアを更新する責務。スコアの計算も担当する。
/// </summary>
/// <remarks>
/// この改訂版では、視線入力の判定を回数ベースから時間ベースに変更し、継続的な視線に対して定期的にイベントを発火させるようにしています。
/// </remarks>
[RequireComponent(typeof(BlackBoard))]
public class BehaveJudgement : MonoBehaviour
{
    [Header("inputイベント判定")]
    // ? 複数の絵画がpublisherなので、SOイベントを利用する
    [SerializeField] private GameEventInt m_rawClickPaintingEvent;
    [SerializeField] private GameEventInt m_rawGazePaintingEvent;

    [Header("調整用パラメータ")]
    [SerializeField, Tooltip("この秒数ごとに「認識」イベントが発火します。0以下の場合は発火しません。")] private float m_recognizeIntervalSec = 1.0f;
    [SerializeField, Tooltip("この秒数ごとに「凝視」イベントが発火します。0以下の場合は発火しません。")] private float m_stareIntervalSec = 5.0f;
    [SerializeField, Tooltip("視線入力が何秒空いたらリセットするか")] private float m_resetTime = 2.0f;

    [Header("データプロバイダ")]
    [SerializeField] private AllPaintingsDatabase m_dataProvider;

    [Header("ログ出力")]
    [SerializeField] private BehaviorLog m_behaviorLog;

    private BlackBoard m_blackBoard;

    // --- 状態変数 ---
    private int m_currentGazePaintingID = -1; // 現在注視している絵画のID
    private float m_gazeStartTime = 0f;       // 現在の注視が始まった時刻 (Time.time)
    private float m_lastGazeTime = 0f;        // 最後に視線入力を受け取った時刻
    private float m_nextRecognizeTriggerTime = 0f;
    private float m_nextStareTriggerTime = 0f;

    // 視線入力が途切れた場合、注視中の絵IDをクリアする（"最後に見た絵"が残り続けるのを防ぐ）
    private void Update()
    {
        if (m_currentGazePaintingID == -1) return;

        float now = Time.time;
        if (now - m_lastGazeTime <= m_resetTime) return;

        m_currentGazePaintingID = -1;
        m_blackBoard.CurrentFocusedPaintingID = -1;
    }

    private void Awake()
    {
        m_blackBoard = GetComponent<BlackBoard>();
    }
    private void OnEnable()
    {
        m_rawClickPaintingEvent.AddListener(OnRawClick);
        m_rawGazePaintingEvent.AddListener(OnRawGaze);
    }

    private void OnDisable()
    {
        m_rawClickPaintingEvent.RemoveListener(OnRawClick);
        m_rawGazePaintingEvent.RemoveListener(OnRawGaze);
    }

    /// <summary>
    /// 生のクリック入力を受け取り、興味度を即座に更新
    /// </summary>
    private void OnRawClick(int paintingID)
    {
        var paintingData = m_dataProvider.GetPaintingByID(paintingID);
        RecordPaintingInteraction(SensorEventType.PaintingClicked, paintingData);
        Debug.Log($"[BehaveJudgement] Click判定: paintingID={paintingID}");
    }

    /// <summary>
    /// 視線入力を受け取り、継続時間に応じて定期的に興味度を更新
    /// </summary>
    private void OnRawGaze(int paintingID)
    {
        float now = Time.time;
        bool isSamePainting = paintingID == m_currentGazePaintingID;

        // 生の視線入力を受け取った時点で「現在注視中」の絵を更新
        // （認識/凝視イベントの発火間隔に依存して注視IDが遅延しないようにする）
        m_blackBoard.CurrentFocusedPaintingID = paintingID;

        // 視線状態のリセット判定
        if (!isSamePainting || now - m_lastGazeTime > m_resetTime)
        {
            m_currentGazePaintingID = paintingID;
            m_gazeStartTime = now;

            // 次のイベント発火時刻を初期化
            // 最初のイベントは、指定されたインターバル時間が経過したときに発火する
            m_nextRecognizeTriggerTime = m_recognizeIntervalSec;
            m_nextStareTriggerTime = m_stareIntervalSec;

            Debug.Log($"[BehaveJudgement] Gaze state reset. Now watching painting ID: {paintingID}");
        }

        // 最後の視線時刻を更新
        m_lastGazeTime = now;

        // 継続的なイベント発火判定
        float currentGazeDuration = now - m_gazeStartTime;

        if (m_recognizeIntervalSec > 0 && currentGazeDuration >= m_nextRecognizeTriggerTime)
        {
            var paintingData = m_dataProvider.GetPaintingByID(paintingID);
            RecordPaintingInteraction(SensorEventType.PaintingRecognized, paintingData);
            m_nextRecognizeTriggerTime += m_recognizeIntervalSec;
            Debug.Log($"[BehaveJudgement] Recognize判定: paintingID={paintingID}");
        }

        if (m_stareIntervalSec > 0 && currentGazeDuration >= m_nextStareTriggerTime)
        {
            var paintingData = m_dataProvider.GetPaintingByID(paintingID);
            RecordPaintingInteraction(SensorEventType.PaintingStared, paintingData);
            m_nextStareTriggerTime += m_stareIntervalSec;
            Debug.Log($"[BehaveJudgement] Stare判定: paintingID={paintingID}");
        }
    }

    /// <summary>
    /// ユーザーのインタラクションを記録し、対応する絵画の興味スコアを更新するメソッド
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="paintingData"></param>
    private void RecordPaintingInteraction(SensorEventType eventType, PaintingDataStruct paintingData)
    {
        int paintingId = paintingData.ID;
        m_blackBoard.CurrentFocusedPaintingID = paintingId;

        if (!m_blackBoard.paintingsInterestScore.TryGetValue(paintingId, out PaintingInteractionLog log))
        {
            log = new PaintingInteractionLog(paintingId, paintingData.Name, paintingData.ArtistName);
            m_blackBoard.paintingsInterestScore[paintingId] = log;
        }

        switch (eventType)
        {
            case SensorEventType.PaintingClicked:
                log.ClickCount++;
                break;
            case SensorEventType.PaintingRecognized:
                log.RecognizeCount++;
                break;
            case SensorEventType.PaintingStared:
                log.StareCount++;
                break;
        }
        UpdatePaintingScore(log);

        // ログ出力（イベント名のみを含める）
        if (m_behaviorLog != null)
        {
            string verb = eventType switch
            {
                SensorEventType.PaintingClicked => "絵画のクリック[高い注目]",
                SensorEventType.PaintingRecognized => "見ている",
                SensorEventType.PaintingStared => "ずっと見ている",
                _ => eventType.ToString()
            };
            m_behaviorLog.AddLog(BehaviorLog.BehaviorEventKind.PaintingInteraction,
                $"{verb} ID={paintingData.ID} '{paintingData.Name}' by {paintingData.ArtistName}");
        }
    }

    /// <summary>
    /// 特定の絵画ログのスコアを計算し、更新するメソッド
    /// </summary>
    private void UpdatePaintingScore(PaintingInteractionLog log)
    {
        if (log == null) return;

        log.CurrentScore = log.ClickCount * m_blackBoard.ClickMultiplier +
                           log.RecognizeCount * m_blackBoard.RecognizeMultiplier +
                           log.StareCount * m_blackBoard.StareMultiplier;
    }
}
