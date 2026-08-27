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
        target.AddStatus(StatusType.Fire, 4);
        Debug.Log(target.name + " 화재 발생!");
    }
}