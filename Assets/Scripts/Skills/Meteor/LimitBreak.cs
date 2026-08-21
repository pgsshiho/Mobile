using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Meteor/LimitBreak")]
public class LimitBreak : SkillBase
{
    [Header("Buff")]
    public BuffData limitBreakBuff;

    [Header("Self Recoil Damage")]
    public int selfDamage = 10;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (limitBreakBuff != null)
        {
            user.AddBuff(limitBreakBuff);
        }

        // 반동 데미지
        user.TakeDamage(selfDamage, Unit.DamageType.Normal);
        Debug.Log($"{user.Unitname} 리미트 해제 발동! 공격력 대폭 상승 및 반동 {selfDamage} 피해!");
    }
}
