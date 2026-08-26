using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Apollo/BayonetThrust")]
public class BayonetThrust : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 총검술이 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        target.StatusHandler.AddStatus(StatusType.Bleeding);
        Debug.Log($"{user.Unitname} 총검 찌르기 적중! {target.Unitname}에게 {damage} 피해 및 출혈!");
    }
}
