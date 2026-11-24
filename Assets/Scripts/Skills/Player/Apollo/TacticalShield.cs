using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Apollo/TacticalShield")]
public class TacticalShield : SkillBase
{
    [Header("Buff")]
    public BuffData shieldBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        Unit recipient = target != null ? target : user;
        if (recipient == null) return;

        if (shieldBuff != null)
        {
            recipient.AddBuff(shieldBuff);
        }

        Debug.Log($"{recipient.Unitname} 전술 방패 전개! 방어력 상승!");
    }
}
