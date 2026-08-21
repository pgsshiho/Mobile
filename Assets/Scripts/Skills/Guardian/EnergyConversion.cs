using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Guardian/EnergyConversion")]
public class EnergyConversion : SkillBase
{
    [Header("Conversion Buff")]
    public BuffData energyBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (energyBuff != null)
        {
            user.AddBuff(energyBuff);
        }

        Debug.Log($"{user.Unitname} 에너지 전환 태세 가동! 공격력 및 회복 효율 증가!");
    }
}
