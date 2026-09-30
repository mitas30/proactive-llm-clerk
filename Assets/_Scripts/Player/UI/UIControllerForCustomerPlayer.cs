using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIControllerForCustomerPlayer : MonoBehaviour
{
    const string DefRecogText = "(認識結果がここに表示されます。)";

    [SerializeField] BlackBoard blackBoard;

    [Header("イベントアセット")]
    [SerializeField] GameEvent SpeechStarted;
    [SerializeField] GameEvent SpeechCanceled;

    [SerializeField] TextMeshProUGUI recognizeAudioTextUI;

    void Start()
    {
        ResetRecognizedResult();
    }

    void OnEnable()
    {
        SpeechStarted.OnPublished.AddListener(OnStartRecoginezed);
        SpeechCanceled.OnPublished.AddListener(OnCancelRecognized);
    }
    void OnDisable()
    {
        SpeechStarted.OnPublished.RemoveListener(OnStartRecoginezed);
        SpeechCanceled.OnPublished.RemoveListener(OnCancelRecognized);
    }

    /// <summary>
    /// 音声認識を開始する関数
    /// </summary>
    void OnStartRecoginezed()
    {
        blackBoard.IsShutUp = true;
        ResetRecognizedResult();
    }

    void OnCancelRecognized()
    {
        blackBoard.IsShutUp = false;
        ResetRecognizedResult();
    }
    void ResetRecognizedResult()
    {
        recognizeAudioTextUI.text = DefRecogText;
    }
}
