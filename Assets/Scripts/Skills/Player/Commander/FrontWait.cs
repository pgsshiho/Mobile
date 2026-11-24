using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Commander/FrontWait")]
public class FrontWait : SkillBase
{
    public BuffData frontWait;

    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        Unit recipient = target != null ? target : user;
        if (recipient == null) return;

        if (frontWait != null)
        {
            recipient.AddBuff(frontWait);
        }

        Debug.Log(recipient.name + " 전선유지 버프 적용!");
    }
}
