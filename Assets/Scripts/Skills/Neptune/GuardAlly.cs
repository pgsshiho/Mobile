using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Neptune/GuardAlly")]
public class GuardAlly : SkillBase
{
    [Header("Buffs")]
    public BuffData allyDefenseBuff;
    public BuffData selfDefensePenaltyBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (allyDefenseBuff != null)
        {
            target.AddBuff(allyDefenseBuff);
        }

        if (selfDefensePenaltyBuff != null)
        {
            user.AddBuff(selfDefensePenaltyBuff);
        }

        Debug.Log($"{user.Unitname}이(가) {target.Unitname}을 수호! 아군 방어력 대폭 증가!");
    }
}
