using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Neptune/armorfuge")]
public class armorfuge : SkillBase
{
    [Header("Buff")]
    public BuffData Armorfuge;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (target == null) return;

        if (Armorfuge != null)
        {
            target.AddBuff(Armorfuge);
        }
        user.Heal(user.maxHealth * 0.05f);
        Debug.Log($"{target.Unitname} 왜곡장 효과로 회피율 및 방어력 증가!");
    }
}
