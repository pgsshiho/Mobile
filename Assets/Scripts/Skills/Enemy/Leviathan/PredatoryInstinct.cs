using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Leviathan/PredatoryInstinct")]
public class PredatoryInstinct : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 포식 본능 공격이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=red>[포식 본능]</color> {user.Unitname}이(가) 빈사 상태인 {target.Unitname}에게 맹렬한 포식 공격! {damage} 막대한 피해!");
    }
}
