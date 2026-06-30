using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Friction")]
public class Friction : SkillBase
{
    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        target.isFires = true;
        target.fireCount += 1;
        Debug.Log(
            target.name +
            " 화상!"
        );

        if (target.health <= 0)
        {
            target.Die();
        }
    }
}