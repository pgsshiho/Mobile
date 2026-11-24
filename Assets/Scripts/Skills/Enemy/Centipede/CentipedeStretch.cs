using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Centipede/CentipedeStretch")]
public class CentipedeStretch : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user is CentipedeUnit centipede)
        {
            centipede.SplitIntoFour();
        }
        else
        {
            Debug.LogWarning("[늘어나기] 시전자가 CentipedeUnit이 아닙니다.");
        }
    }
}
