using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Vulcan/Fortify")]
public class Fortify : SkillBase
{
    [Header("Buff")]
    public BuffData fortifyBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        Unit recipient = target != null ? target : user;
        if (recipient == null) return;

        if (fortifyBuff != null)
        {
            recipient.AddBuff(fortifyBuff);
        }

        Debug.Log($"{recipient.Unitname} 진지 구축 버프 적용 (방어력 및 진형 안정 상승)!");
    }
}
