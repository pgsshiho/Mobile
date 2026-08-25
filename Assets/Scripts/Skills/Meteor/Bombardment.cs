using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Meteor/Bombardment")]
public class Bombardment : SkillBase
{
    [Header("Bonus Critical Multiplier")]
    public float criticalMultiplierBonus = 1.2f;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 명중 체크
        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 포격이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        damage = Mathf.RoundToInt(damage * criticalMultiplierBonus);

        target.TakeDamage(damage);
        TryApplyStatus(target, skill);

        Debug.Log($"{user.Unitname} 이(가) {target.Unitname} 에게 {damage} 강력한 포격 피해!");
    }
}
