using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Situation Command")]
public class SC : SkillBase
{
    public BuffData situationBuff;


    public override void Use(
    Unit user,
    Unit target,
    SkillData skill
)
    {
        user.AddBuff(situationBuff);
    }
}