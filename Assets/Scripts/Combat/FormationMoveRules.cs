/// <summary>
/// 파티 열 교대 규칙만 담당한다. 실제 위치 갱신과 턴 종료는 BattleManager의 책임이다.
/// </summary>
public sealed class FormationMoveRules
{
    private readonly BattleTargetingRules targetingRules;

    public FormationMoveRules(BattleTargetingRules targetingRules)
    {
        this.targetingRules = targetingRules;
    }

    public bool CanStartMove(Unit[] partySlots, PlayerUnit player)
    {
        if (partySlots == null || player == null)
            return false;

        int currentColumn = targetingRules.GetPartyColumn(partySlots, player);
        if (currentColumn < 0)
            return false;

        foreach (Unit unit in partySlots)
        {
            if (unit is PlayerUnit target && target != player &&
                target.health > 0 &&
                CanMoveToColumn(player, currentColumn,
                    targetingRules.GetPartyColumn(partySlots, target)))
                return true;
        }

        return false;
    }

    public bool TrySwap(
        Unit[] partySlots,
        PlayerUnit mover,
        PlayerUnit target,
        out int currentColumn,
        out int targetColumn)
    {
        if (partySlots == null || mover == null || target == null)
        {
            currentColumn = -1;
            targetColumn = -1;
            return false;
        }

        currentColumn = targetingRules.GetPartyColumn(partySlots, mover);
        targetColumn = targetingRules.GetPartyColumn(partySlots, target);

        if (currentColumn < 0 || targetColumn < 0 ||
            currentColumn == targetColumn ||
            !CanMoveToColumn(mover, currentColumn, targetColumn))
            return false;

        partySlots[targetColumn] = mover;
        partySlots[currentColumn] = target;
        return true;
    }

    private bool CanMoveToColumn(
        PlayerUnit player,
        int currentColumn,
        int targetColumn)
    {
        int distance = targetColumn - currentColumn;
        return distance < 0
            ? -distance <= player.maxForwardMoveColumns
            : distance > 0 &&
              distance <= player.maxBackwardMoveColumns;
    }
}
