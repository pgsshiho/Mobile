using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Frog/FrogLeap")]
public class FrogLeap : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 도약 공격이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=green>[도약]</color> {user.Unitname}의 강력한 도약 착지로 {target.Unitname}에게 {damage} 피해!");
    }
}
