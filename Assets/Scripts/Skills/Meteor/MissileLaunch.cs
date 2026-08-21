using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Meteor/MissileLaunch")]
public class MissileLaunch : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 명중 체크
        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 미사일 폭격이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        Debug.Log($"{user.Unitname} 이(가) {target.Unitname} 에게 {damage} 미사일 폭격 피해!");
    }
}
