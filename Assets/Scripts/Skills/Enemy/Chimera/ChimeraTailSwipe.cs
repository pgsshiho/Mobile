using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Chimera/ChimeraTailSwipe")]
public class ChimeraTailSwipe : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        Debug.Log($"{user.Unitname}의 거대한 꼬리 휘두르기! {target.Unitname}에게 {damage} 광역 피해!");
    }
}
