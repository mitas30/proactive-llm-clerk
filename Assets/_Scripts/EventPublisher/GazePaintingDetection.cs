using UnityEngine;

/// <summary>
/// メインカメラにアタッチする、中央レイキャスト
/// </summary>
[RequireComponent(typeof(Camera))]
public class GazePaintingDetection : MonoBehaviour
{
    [Header("レイキャスト設定")]
    [SerializeField, Tooltip("レイキャストでヒットするレイヤー")]
    private LayerMask paintingMask;
    [SerializeField, Tooltip("レイの最大距離")]
    private float maxDistance = 10f;
    [SerializeField, Tooltip("視線チェック間隔(秒)")]
    private float interval = 0.05f;

    [Header("絵画イベント(SO)")]
    [SerializeField, Tooltip("視線ヒット時に発火する int 引数付きイベント")]
    private GameEventInt gazeEvent;

    private Camera cam;
    private float timer;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (gazeEvent == null)
            Debug.LogError($"[{nameof(GazePaintingDetection)}] イベントが未設定です。");
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < interval) return;
        timer = 0f;
        CastGaze();
    }

    private void CastGaze()
    {
        Ray ray = cam.ScreenPointToRay(
            new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
        );
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red, interval);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, paintingMask))
        {
            var data = hit.collider.GetComponent<ClickPaintingDetection>().PaintingData;
            if (data != null)
            {
                gazeEvent.Publish(data.ID);
                Debug.Log($"視線ヒット: {data.ID}");
            }
        }
    }
}
