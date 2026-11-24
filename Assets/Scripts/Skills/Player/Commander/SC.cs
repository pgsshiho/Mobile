using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Commander/Situation Command")]
public class SC : SkillBase
{
    public BuffData situationBuff;

    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        Unit recipient = target != null ? target : user;
        if (recipient != null && situationBuff != null)
        {
            recipient.AddBuff(situationBuff);
            Debug.Log($"{recipient.Unitname} 상황 지휘 버프 적용 (명중 및 치명타 상승)!");
        }
    }
}
