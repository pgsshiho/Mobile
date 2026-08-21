using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Apollo/LightMaintenance")]
public class LightMaintenance : SkillBase
{
    [Header("Heal Amount")]
    public int healAmount = 18;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (target == null) return;

        int finalHeal = Mathf.RoundToInt(healAmount * target.GetHealMultiplier());
        target.Heal(finalHeal);

        Debug.Log($"{user?.Unitname} 경정비 완료! {target.Unitname} {finalHeal} 회복!");
    }
}
