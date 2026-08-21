using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Apollo/TacticalShield")]
public class TacticalShield : SkillBase
{
    [Header("Buff")]
    public BuffData shieldBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (shieldBuff != null)
        {
            user.AddBuff(shieldBuff);
        }

        Debug.Log($"{user.Unitname} 전술 방패 전개! 방어력 상승!");
    }
}
