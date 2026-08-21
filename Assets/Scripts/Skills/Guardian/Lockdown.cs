using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Guardian/Lockdown")]
public class Lockdown : SkillBase
{
    [Header("Heal Percent")]
    public float healPercent = 0.3f;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        int healAmount = Mathf.RoundToInt(user.maxHealth * healPercent);
        user.Heal(healAmount);

        // 다음 턴 스킵 (기절 부여)
        user.isStunned = true;

        Debug.Log($"{user.Unitname} 강제 고정! 체력 {healAmount} 회복 및 다음 턴 충전 대기!");
    }
}
