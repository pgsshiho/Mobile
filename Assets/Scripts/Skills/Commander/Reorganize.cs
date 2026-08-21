using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Reorganize")]
public class Reorganize : SkillBase
{
    public BuffData reorganizeBuff;

    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        // 디버프 제거
        target.ClearDebuffs();

        // 회복 버프 부여
        target.AddBuff(reorganizeBuff);

        Debug.Log(
            target.name +
            " 재정비!"
        );
    }
}