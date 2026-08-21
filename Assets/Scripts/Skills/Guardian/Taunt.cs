using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Guardian/Taunt")]
public class Taunt : SkillBase
{
    [Header("Buff")]
    public BuffData tauntBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (tauntBuff != null)
        {
            user.AddBuff(tauntBuff);
        }

        Debug.Log($"{user.Unitname} 도발 발동! 2턴간 적 공격 유도 및 방어력 증가!");
    }
}
