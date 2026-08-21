using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Eagle/LockOn")]
public class LockOn : SkillBase
{
    [Header("Mark Duration")]
    public int markDuration = 3;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (target == null) return;

        target.AddMark(markDuration);
        Debug.Log($"{target.Unitname}에게 조준 고정 표식 부여 완료 (3턴 지속, 아군 명중률 보장)!");
    }
}
