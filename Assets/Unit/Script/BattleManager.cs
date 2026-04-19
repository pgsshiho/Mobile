using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    public List<UnitBase> playerUnits = new List<UnitBase>();
    public List<UnitBase> enemyUnits = new List<UnitBase>();
    private List<UnitBase> turnOrder = new List<UnitBase>();

    private bool isBattleOver = false;

    void Start()
    {
        StartCoroutine(BattleSequence());
    }

    IEnumerator BattleSequence()
    {
        while (!isBattleOver)
        {
            // 1. 턴 순서 결정 (Speed 기준 정렬)
            DetermineTurnOrder();

            foreach (var unit in turnOrder)
            {
                if (unit == null || unit.currentHp <= 0) continue;

                Debug.Log($"{unit.name}의 턴 시작");

                // 2. 상태 이상 처리
                unit.TurnStart();
                yield return new WaitForSeconds(0.5f); // 연출 대기

                // 3. 유닛 행동 실행 및 완료 대기 (시퀀스 관리)
                bool unitActionDone = false;
                unit.ExecuteTurn(() => unitActionDone = true);

                // 유닛이 행동을 마칠 때까지 코루틴 정지
                yield return new WaitUntil(() => unitActionDone);

                // 4. 승패 판정
                if (CheckBattleOver())
                {
                    isBattleOver = true;
                    break;
                }
            }
            yield return new WaitForSeconds(1f); // 라운드 간 휴식
        }
        Debug.Log("전투 종료!");
    }

    void DetermineTurnOrder()
    {
        turnOrder.Clear();
        turnOrder.AddRange(playerUnits.Where(u => u != null));
        turnOrder.AddRange(enemyUnits.Where(u => u != null));
        turnOrder = turnOrder.OrderByDescending(u => u.speed).ToList();
    }

    bool CheckBattleOver()
    {
        if (playerUnits.All(u => u == null || u.currentHp <= 0)) { Debug.Log("패배..."); return true; }
        if (enemyUnits.All(u => u == null || u.currentHp <= 0)) { Debug.Log("승리!"); return true; }
        return false;
    }
}