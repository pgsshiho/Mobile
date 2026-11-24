using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/RedButterfly/ProboscisStab")]
public class ProboscisStab : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 주둥이 찌르기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        Debug.Log($"{user.Unitname}의 날카로운 주둥이 찌르기 적중! {target.Unitname}에게 {damage} 피해!");
    }
}
