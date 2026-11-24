using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 거대고래 전용 Enemy 클래스.
/// 플레이어를 삼켜 일정 턴 동안 전투에서 제외시키며, 고래가 사망하거나 지속 시간이 끝나면 복귀시킵니다.
/// 시작 시 합선 / 과열 상태이상을 보유합니다.
/// </summary>
public class GiantWhaleUnit : Enemy
{
    [Header("삼키기 기믹")]
    public Unit swallowedUnit;
    public int remainingSwallowTurns = 0;

    protected override void Start()
    {
        base.Start();

        // 초기 상태: 합선 및 과열
        AddStatus(StatusType.ShortCircuit, 3);
        AddStatus(StatusType.Overheat, 3);
    }

    public void Swallow(Unit target, int turns)
    {
        if (target == null || swallowedUnit != null) return;

        swallowedUnit = target;
        remainingSwallowTurns = turns;

        // 전투에서 일시 제외
        if (TurnManager.instance != null)
        {
            TurnManager.instance.turnList.Remove(target);
        }

        target.gameObject.SetActive(false);
        Debug.Log($"<color=purple>[거대고래 삼키기]</color> {Unitname}이(가) {target.Unitname}을(를) 삼켰습니다! ({turns}턴 동안 제외)");
    }

    public void ReleaseSwallowedUnit()
    {
        if (swallowedUnit != null)
        {
            swallowedUnit.gameObject.SetActive(true);
            if (TurnManager.instance != null && !TurnManager.instance.turnList.Contains(swallowedUnit))
            {
                TurnManager.instance.turnList.Add(swallowedUnit);
            }

            Debug.Log($"<color=cyan>[소화 실패 - 복귀]</color> 삼켜졌던 {swallowedUnit.Unitname}이(가) 전장으로 뱉어졌습니다!");
            swallowedUnit = null;
            remainingSwallowTurns = 0;
        }
    }

    public override void MyTurn()
    {
        if (swallowedUnit != null)
        {
            remainingSwallowTurns--;
            Debug.Log($"[거대고래] {swallowedUnit.Unitname} 삼킴 유지 중... (남은 턴: {remainingSwallowTurns})");
            if (remainingSwallowTurns <= 0)
            {
                ReleaseSwallowedUnit();
            }
        }

        base.MyTurn();
    }

    public override void Die()
    {
        // 고래 사망 시 삼킨 유닛 즉시 구출
        ReleaseSwallowedUnit();
        base.Die();
    }
}
