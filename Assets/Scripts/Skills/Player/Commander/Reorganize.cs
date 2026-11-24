using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Commander/Reorganize")]
public class Reorganize : SkillBase
{
    public BuffData reorganizeBuff;

    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        Unit recipient = target != null ? target : user;
        if (recipient == null) return;

        // 디버프 제거
        recipient.ClearDebuffs();

        // 회복 버프 부여
        if (reorganizeBuff != null)
        {
            recipient.AddBuff(reorganizeBuff);
        }

        Debug.Log(recipient.name + " 재정비 완료 (디버프 해제 및 회복 버프)!");
    }
}
