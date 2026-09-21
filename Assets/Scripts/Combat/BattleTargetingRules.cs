using System;
using System.Collections.Generic;

/// <summary>
/// 전투 대상 거리와 스킬 사용 열을 판정한다.
/// MonoBehaviour나 UI에 의존하지 않아 규칙만 독립적으로 검증할 수 있다.
/// </summary>
public sealed class BattleTargetingRules
{
    public int GetPartyColumn(Unit[] partySlots, PlayerUnit player)
    {
        if (partySlots == null || player == null)
            return -1;

        return Array.IndexOf(partySlots, player);
    }

    public bool CanPlayerTargetEnemy(
        Unit[] partySlots,
        IReadOnlyDictionary<Enemy, int> enemyColumns,
        PlayerUnit attacker,
        Enemy target,
        SkillData skill)
    {
        if (enemyColumns == null || attacker == null ||
            target == null || skill == null)
            return false;

        int partyColumn = GetPartyColumn(partySlots, attacker);
        if (partyColumn < 0 ||
            !enemyColumns.TryGetValue(target, out int enemyColumn))
            return false;

        int distance = partyColumn + enemyColumn + 1;
        return distance <= Math.Max(1, skill.maxTargetDistance);
    }

    public bool CanUseSkillAtCurrentColumn(
        Unit[] partySlots,
        PlayerUnit player,
        SkillData skill)
    {
        if (player == null || skill == null)
            return false;

        int partyColumn = GetPartyColumn(partySlots, player);
        if (partyColumn < 0)
            return false;

        int minColumn = Math.Min(skill.minUserColumn, skill.maxUserColumn);
        int maxColumn = Math.Max(skill.minUserColumn, skill.maxUserColumn);
        return partyColumn >= minColumn && partyColumn <= maxColumn;
    }
}
