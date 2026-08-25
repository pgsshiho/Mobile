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
        // 마찰열로 화재 상태 부여 (공격 없이 순수 상태이상)
        target.AddStatus(StatusType.Fire, 4);
        TryApplyStatus(target, skill);

        Debug.Log(target.name + " 화재!");

        if (target.health <= 0)
        {
            target.Die();
        }
    }
}