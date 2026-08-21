using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Vulcan/BatchRepair")]
public class BatchRepair : SkillBase
{
    [Header("Heal Amount Per Target")]
    public int healAmount = 15;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        int finalHeal = Mathf.RoundToInt(healAmount * target.GetHealMultiplier());
        target.Heal(finalHeal);

        Debug.Log($"{user.Unitname}의 일괄 수리로 {target.Unitname} {finalHeal} 회복!");
    }
}
