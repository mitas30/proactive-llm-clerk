using UnityEngine;

public class CanvasCameraFinder : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();

        // もしCanvasがあって、かつEvent Cameraが設定されていなければ
        if (canvas != null && canvas.worldCamera == null)
        {
            // シーン内からメインカメラを探してきて設定する
            canvas.worldCamera = Camera.main;
        }
    }
}