using UnityEngine;

public abstract class SkillBase : ScriptableObject
{
    public abstract void Use(
        Unit user,
        Unit target,
        SkillData skill
    );

    /// <summary>
    /// SkillData에 설정된 상태이상을 target에게 부여합니다.
    /// statusChance(%) 확률 판정 후, statusTurns 턴 동안 지속됩니다.
    /// 스킬 구현체 내에서 공격 후 TryApplyStatus(target, skill)를 호출하면 됩니다.
    /// </summary>
    protected void TryApplyStatus(Unit target, SkillData skill)
    {
        if (skill == null) return;
        if (skill.statusEffect == StatusType.None) return;
        if (skill.statusTurns == 0) return;

        int roll = Random.Range(0, 100);
        if (roll < skill.statusChance)
        {
            target.AddStatus(skill.statusEffect, skill.statusTurns);
            Debug.Log($"[{skill.skillName}] {target.Unitname}에게 [{skill.statusEffect}] {skill.statusTurns}턴 부여 (확률 {skill.statusChance}%)");
        }
    }
}