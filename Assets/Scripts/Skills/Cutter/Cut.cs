using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Cut")]
public class Cut : SkillBase
{
    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        // 명중 체크
        if (!user.CheckHit(target, skill))
        {
            Debug.Log("빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        target.AddStatus(StatusType.Bleeding, 3);
        TryApplyStatus(target, skill);

        Debug.Log(user.name + " 이 " + target.name + " 에게 " + damage + " 피해 / 출혈!");
    }
}