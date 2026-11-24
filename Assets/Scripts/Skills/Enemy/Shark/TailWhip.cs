using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Shark/TailWhip")]
public class TailWhip : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 꼬리 채찍이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=red>[꼬리 채찍]</color> {user.Unitname}이(가) {target.Unitname}을(를) 꼬리로 강타! {damage} 피해!");
    }
}
