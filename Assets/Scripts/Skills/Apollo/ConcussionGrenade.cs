using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Apollo/ConcussionGrenade")]
public class ConcussionGrenade : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 충격 유탄이 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        Debug.Log($"{user.Unitname} 충격 유탄 적중! {target.Unitname}에게 {damage} 피해 및 넉백!");
    }
}
