using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Hornet/CommDrone")]
public class CommDrone : SkillBase
{
    [Header("Buff")]
    public BuffData commBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (target == null) return;

        if (commBuff != null)
        {
            target.AddBuff(commBuff);
        }

        Debug.Log($"{target.Unitname} 통신 드론 지원으로 전술 능력 향상!");
    }
}
