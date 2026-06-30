using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    // 턴 순서 리스트
    public List<Unit> turnList = new();

    // 현재 턴 유닛
    public Unit currentUnit;

    // 현재 턴 인덱스
    int currentTurnIndex = 0;
    bool battleEnded = false;

    // 타겟 선택 대기중?
    public bool waitingForTarget = false;
    public GameObject reward;
    private void Awake()
    {
        instance = this;
    }

    // 방 전투 시작
    public void RegisterRoom(Room room)
    {
        battleEnded = false;
        waitingForTarget = false;
        currentUnit = null;

        turnList.Clear();

        // 파티 등록
        foreach (Unit party in PartyManager.instance.partySlots)
        {
            if (party != null)
            {
                turnList.Add(party);
            }
        }

        // 적 등록
        foreach (Enemy enemy in room.enemies)
        {
            if (enemy != null)
            {
                turnList.Add(enemy);
            }
        }

        // speed 높은 순 정렬
        turnList.Sort((a, b) =>
           b.GetSpeed().CompareTo(a.GetSpeed()));

        currentTurnIndex = 0;

        StartTurn();
    }

    // 현재 턴 시작
    public void StartTurn()
    {
        if (battleEnded)
            return;

        if (CheckBattleEnd())
            return;

        if (turnList.Count <= 0)
        {
            Debug.Log("전투 종료");
            return;
        }

        if (currentTurnIndex >= turnList.Count)
            currentTurnIndex = 0;

        currentUnit = turnList[currentTurnIndex];

        if (currentUnit == null || currentUnit.health <= 0)
        {
            EndTurn();
            return;
        }

        BattleManager.instance.StartTurn(currentUnit);

        Debug.Log(currentUnit.Unitname + " 턴");

        currentUnit.MyTurn();
    }
    bool CheckBattleEnd()
    {
        if (battleEnded)
            return true;
        bool allPlayersDead = true;
        bool allEnemiesDead = true;

        foreach (Unit unit in turnList)
        {
            if (unit == null || unit.health <= 0)
                continue;

            if (unit.gameObject.layer == LayerMask.NameToLayer("Player"))
                allPlayersDead = false;

            if (unit.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                allEnemiesDead = false;
        }

        if (allEnemiesDead)
        {
            battleEnded = true;
            BattleManager.instance.EndBattle(true);
            return true;
        }

        if (allPlayersDead)
        {
            battleEnded = true;
            BattleManager.instance.EndBattle(false);
            return true;
        }

        return false;
    }

    // 턴 종료
    public void EndTurn()
    {
        if (currentUnit != null &&
            currentUnit.myturnUI != null)
        {
            currentUnit.myturnUI.SetActive(false);
        }


        if (CheckBattleEnd())
            return;

        currentTurnIndex++;

        if (currentTurnIndex >= turnList.Count)
            currentTurnIndex = 0;

        StartTurn();
    }
    // 유닛 제거
    public void RemoveUnit(Unit unit)
    {
        if (turnList.Contains(unit))
        {
            turnList.Remove(unit);
        }

        // 현재 인덱스 보정
        if (currentTurnIndex >= turnList.Count)
        {
            currentTurnIndex = 0;
        }
    }
}