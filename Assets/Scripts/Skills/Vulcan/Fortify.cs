using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Vulcan/Fortify")]
public class Fortify : SkillBase
{
    [Header("Buff")]
    public BuffData fortifyBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (target == null) return;

        if (fortifyBuff != null)
        {
            target.AddBuff(fortifyBuff);
        }

        Debug.Log($"{target.Unitname} 진지 구축 버프 적용 (방어력 및 진형 안정 상승)!");
    }
}
