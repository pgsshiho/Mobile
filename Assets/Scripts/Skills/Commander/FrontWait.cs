using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/FrontWait")]
public class FrontWait : SkillBase
{
    public BuffData frontWait;

    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {

        target.AddBuff(frontWait);

        Debug.Log(
            target.name +
            " 전선유지!"
        );
    }
}