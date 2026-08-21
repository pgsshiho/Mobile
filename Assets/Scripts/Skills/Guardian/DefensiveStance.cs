using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Guardian/DefensiveStance")]
public class DefensiveStance : SkillBase
{
    [Header("Buff")]
    public BuffData defenseBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (defenseBuff != null)
        {
            user.AddBuff(defenseBuff);
        }

        Debug.Log($"{user.Unitname} 방어 자세 돌입! 방어력 50% 증가!");
    }
}
