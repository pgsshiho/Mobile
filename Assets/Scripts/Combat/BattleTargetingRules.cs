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

        // 1. 공격 가능한 대상 열(allowedTargetColumns)이 설정되어 있는 경우
        if (skill.allowedTargetColumns != null && skill.allowedTargetColumns.Length > 0)
        {
            return skill.IsAllowedAtTargetColumn(enemyColumn);
        }

        // 2. 대상 열이 따로 지정되지 않은 경우 기존 최대 거리(maxTargetDistance) 규칙 적용
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

        // allowedUserColumns 배열 기반 판정
        return skill.IsAllowedAtColumn(partyColumn);
    }
}
