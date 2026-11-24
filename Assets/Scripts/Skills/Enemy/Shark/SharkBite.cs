using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Shark/SharkBite")]
public class SharkBite : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 상어 물기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=red>[상어 물기]</color> {user.Unitname}이(가) {target.Unitname}을(를) 강하게 물어뜯음! {damage} 피해!");
    }
}
