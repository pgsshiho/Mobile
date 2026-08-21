using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Neptune/DistortionField")]
public class DistortionField : SkillBase
{
    [Header("Buff")]
    public BuffData distortionBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (target == null) return;

        if (distortionBuff != null)
        {
            target.AddBuff(distortionBuff);
        }

        Debug.Log($"{target.Unitname} 왜곡장 효과로 회피율 및 방어력 증가!");
    }
}
