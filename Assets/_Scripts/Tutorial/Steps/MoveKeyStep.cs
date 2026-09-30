using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Steps/MoveKey", fileName = "MoveKeyStep")]
public class MoveKeyStep : TutorialStepBase
{
    public override IEnumerator Execute(TutorialContext ctx)
    {
        while (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))) yield return null;
        ctx.NPCFollow.IsFollowing = true;
    }
}
