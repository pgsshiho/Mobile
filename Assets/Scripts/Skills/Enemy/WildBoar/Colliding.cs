using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/WildBoar/Colliding")]
public class Colliding : SkillBase
{
    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 부딪치기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"{user.Unitname}의 부딪치기가 {target.Unitname}에게 적중하여 {damage} 피해!");
    }
}