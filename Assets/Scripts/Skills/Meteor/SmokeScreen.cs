using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Meteor/SmokeScreen")]
public class SmokeScreen : SkillBase
{
    [Header("Buff")]
    public BuffData smokeScreenBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (smokeScreenBuff != null)
        {
            user.AddBuff(smokeScreenBuff);
        }

        Debug.Log($"{user.Unitname} 연막 전개! 방어 및 생존력 상승!");
    }
}
