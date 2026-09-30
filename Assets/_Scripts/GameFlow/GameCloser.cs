using UnityEngine;
using BehaviorDesigner.Runtime;

/// <summary>
/// 終了リクエストの発行専用クラス。
/// - BlackBoard.IsQuit を true にセット
/// - Behavior の変数 "quitMessage" にメッセージを書き込む
/// 呼び出し口を2種類用意（それぞれ別メッセージを使用）。
/// </summary>
public class GameCloser : MonoBehaviour
{
    [Header("Messages")]
    [SerializeField, TextArea, Tooltip("ボタンからの終了時に設定するメッセージ")] private string m_messageFromButton = "ご利用ありがとうございました。体験を終了します。";
    [SerializeField, TextArea, Tooltip("PurchaseState 終了時に設定するメッセージ")] private string m_messageFromState = "お買い物は以上です。体験を終了します。";

    [Header("References")]
    [SerializeField] private BlackBoard m_blackBoard;
    [SerializeField, Tooltip("Behavior Designer の Behavior コンポーネント（変数 quitMessage を持つ）")] private Behavior m_aiCharacterBehavior;

    [Header("Variable Names")]
    [SerializeField, Tooltip("Behavior 変数名（SharedString）")] private string m_quitMessageVarName = "quitMessage";

    public void RequestQuitFromButton()
    {
        PublishQuitRequest(m_messageFromButton);
    }

    public void RequestQuitFromState()
    {
        PublishQuitRequest(m_messageFromState);
    }

    private void PublishQuitRequest(string message)
    {
        if (m_blackBoard != null)
        {
            m_blackBoard.IsQuit = true;
        }
        else
        {
            Debug.LogWarning("[GameCloser] BlackBoard が設定されていません。");
        }

        if (m_aiCharacterBehavior != null)
        {
            try
            {
                m_aiCharacterBehavior.SetVariableValue(m_quitMessageVarName, message);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[GameCloser] Behavior 変数設定に失敗: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("[GameCloser] Behavior コンポーネントが設定されていません。");
        }
    }
}