using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Vulcan/EmergencyRepair")]
public class EmergencyRepair : SkillBase
{
    [Header("Heal Amount")]
    public int healAmount = 25;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        int finalHeal = Mathf.RoundToInt(healAmount * target.GetHealMultiplier());
        target.Heal(finalHeal);

        Debug.Log($"{user.Unitname} 이(가) {target.Unitname} 에게 {finalHeal} 긴급 정비 수리 완료!");
    }
}
