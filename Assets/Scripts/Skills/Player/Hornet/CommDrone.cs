using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Hornet/CommDrone")]
public class CommDrone : SkillBase
{
    [Header("Buff")]
    public BuffData commBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        Unit recipient = target != null ? target : user;
        if (recipient == null) return;

        if (commBuff != null)
        {
            recipient.AddBuff(commBuff);
        }

        Debug.Log($"{recipient.Unitname} 통신 드론 지원으로 전술 능력 향상!");
    }
}
