using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Kraken/TentacleStrike")]
public class TentacleStrike : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 촉수 강타가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=red>[촉수 강타]</color> {user.Unitname}이(가) 거대한 촉수로 {target.Unitname}을(를) 강타하여 {damage} 피해!");
    }
}
