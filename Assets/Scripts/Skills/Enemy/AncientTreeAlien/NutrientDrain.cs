using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/AncientTreeAlien/NutrientDrain")]
public class NutrientDrain : SkillBase
{
    [Header("Drain Ratio")]
    [Tooltip("가한 피해량 중 체력으로 흡수할 비율")]
    public float drainRatio = 0.8f;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 양분 흡수 촉수가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        int healAmount = Mathf.RoundToInt(damage * drainRatio);
        user.Heal(healAmount);

        Debug.Log($"<color=green>[양분 흡수]</color> {user.Unitname}이(가) {target.Unitname}에게 {damage} 피해를 입히고 체력을 {healAmount} 흡수 회복했습니다!");
    }
}
