using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/readytodel")]
public class readytodel : SkillBase
{
    public BuffData readytodels;

    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        user.AddBuff(readytodels);

        Debug.Log(
            user.name +
            " 마찰 가속!"
        );
    }
}