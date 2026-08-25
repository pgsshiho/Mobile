using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Meteor/DualShot")]
public class DualShot : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 명중 체크
        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 양손 사격이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        TryApplyStatus(target, skill);

        Debug.Log($"{user.Unitname} 이(가) {target.Unitname} 에게 {damage} 양손 사격 피해!");
    }
}
