/// <summary>
/// Flow 用ステートが実装すべき共通インターフェース
/// </summary>
public interface IFlowState
{
    /// <summary>ステート突入時に一度だけ呼ばれる</summary>
    void Enter();

    /// <summary>フレーム毎に呼ばれる。完了したら true を返す</summary>
    void Update();

    /// <summary>抜ける直前に呼ばれる（後片付けなど）</summary>
    void Exit();
}