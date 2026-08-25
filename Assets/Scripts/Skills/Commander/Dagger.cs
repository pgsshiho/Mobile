using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Daager")]
public class Daager : SkillBase
{
    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log("빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        TryApplyStatus(target, skill);

        Debug.Log($"{user.Unitname} 단검 공격! {target.Unitname}에게 {damage} 피해");
    }
}