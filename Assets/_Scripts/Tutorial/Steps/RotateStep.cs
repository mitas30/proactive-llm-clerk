using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Steps/Rotate", fileName = "RotateStep")]
public class RotateStep : TutorialStepBase
{
    [SerializeField, Tooltip("完了とみなす最小回転角（度）。0に近いほど敏感です。")]
    private float m_minRotationDegrees = 1f;

    public override IEnumerator Execute(TutorialContext ctx)
    {
        Transform player = ctx.PlayerTransform;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 初期方向を記録し、その方向から一定以上ずれたら完了。
        Vector3? initialForward = player ? player.forward : (Vector3?)null;

        while (true)
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player")?.transform;
                if (player != null)
                    initialForward = player.forward;
            }
            else if (initialForward.HasValue)
            {
                float angleFromStart = Vector3.Angle(initialForward.Value, player.forward);
                if (angleFromStart >= Mathf.Max(0f, m_minRotationDegrees))
                    break; // ちょっとでも回転したら完了
            }
            yield return null;
        }
    }
}
