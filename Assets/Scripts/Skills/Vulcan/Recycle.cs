using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Vulcan/Recycle")]
public class Recycle : SkillBase
{
    [Header("Sacrifice & Heal Values")]
    public int selfHpCost = 15;
    public int healAmount = 30;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 시전자 또는 희생 대상의 체력 차감
        user.TakeDamage(selfHpCost, Unit.DamageType.Normal);

        // 대상 아군에게 2배 분량 회복
        int finalHeal = Mathf.RoundToInt(healAmount * target.GetHealMultiplier());
        target.Heal(finalHeal);

        Debug.Log($"{user.Unitname} 재활용 발동: 자신 체력 {selfHpCost} 소모하여 {target.Unitname}에게 {finalHeal} 회복!");
    }
}
