using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Drowned/FeastOfDrinks")]
public class FeastOfDrinks : SkillBase
{
    [Tooltip("체력 회복량 (기획: 20)")]
    public int healAmount = 20;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        Unit recipient = target != null ? target : user;
        if (recipient == null) return;

        float prevHp = recipient.health;
        recipient.health = Mathf.Min(recipient.maxHealth, recipient.health + healAmount);
        float healed = recipient.health - prevHp;
        recipient.UpdateHealthBar();

        Debug.Log($"<color=green>[술 잔치]</color> {user.Unitname}의 술 잔치로 {recipient.Unitname}의 체력이 {healed} 회복되었습니다! ({recipient.health}/{recipient.maxHealth})");
    }
}
