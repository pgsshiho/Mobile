using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Targeting")]
public class Targeting : SkillBase
{
    public int markDuration = 3;

    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        if (target == null) return;
        target.AddMark(markDuration);
        TryApplyStatus(target, skill);
    }
}