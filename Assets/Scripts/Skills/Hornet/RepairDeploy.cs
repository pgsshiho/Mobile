using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Hornet/RepairDeploy")]
public class RepairDeploy : SkillBase
{
    [Header("Continuous Heal Buff")]
    public BuffData repairBuff;
    public int immediateHeal = 10;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (target == null) return;

        if (immediateHeal > 0)
        {
            target.Heal(Mathf.RoundToInt(immediateHeal * target.GetHealMultiplier()));
        }

        if (repairBuff != null)
        {
            target.AddBuff(repairBuff);
        }

        Debug.Log($"{target.Unitname} 수리 드론 배치 완료 (지속 회복 지원)!");
    }
}
