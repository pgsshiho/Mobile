using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public List<Unit> turnList = new();

    public Unit currentUnit;

    int currentTurnIndex = 0;
    bool battleEnded = false;

    public bool waitingForTarget = false;
    public GameObject reward;

    private void Awake()
    {
        instance = this;
    }

    public void RegisterRoom(Room room)
    {
        if (room.roomType != RoomType.Enemy &&
            room.roomType != RoomType.Boss)
        {
            return;
        }

        battleEnded = false;
        waitingForTarget = false;
        currentUnit = null;

        turnList.Clear();

        foreach (Unit party
            in PartyManager.instance.partySlots)
        {
            if (party != null &&
                party.health > 0)
            {
                turnList.Add(party);
            }
        }

        foreach (Enemy enemy
            in room.enemies)
        {
            if (enemy != null &&
                enemy.health > 0)
            {
                turnList.Add(enemy);
            }
        }

        turnList.Sort((a, b) =>
            b.GetSpeed().CompareTo(a.GetSpeed()));

        currentTurnIndex = 0;

        StartTurn();
    }

    public void StartTurn()
    {
        if (battleEnded)
            return;

        if (CheckBattleEnd())
            return;

        if (turnList.Count <= 0)
            return;

        if (currentTurnIndex >= turnList.Count)
            currentTurnIndex = 0;

        currentUnit = turnList[currentTurnIndex];

        if (currentUnit == null ||
            currentUnit.health <= 0)
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

        bool playerAlive = false;
        bool enemyAlive = false;

        foreach (Unit unit in turnList)
        {
            if (unit == null ||
                unit.health <= 0)
                continue;

            if (unit.gameObject.layer ==
                LayerMask.NameToLayer("Player"))
            {
                playerAlive = true;
            }

            if (unit.gameObject.layer ==
                LayerMask.NameToLayer("Enemy"))
            {
                enemyAlive = true;
            }
        }

        if (!enemyAlive)
        {
            battleEnded = true;
            BattleManager.instance.EndBattle(true);
            return true;
        }

        if (!playerAlive)
        {
            battleEnded = true;
            BattleManager.instance.EndBattle(false);
            return true;
        }

        return false;
    }

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

    public void RemoveUnit(Unit unit)
    {
        if (turnList.Contains(unit))
        {
            int removedIndex =
                turnList.IndexOf(unit);

            turnList.Remove(unit);

            if (removedIndex <= currentTurnIndex)
            {
                currentTurnIndex--;
            }
        }

        if (currentTurnIndex < 0)
            currentTurnIndex = 0;

        if (currentTurnIndex >= turnList.Count)
            currentTurnIndex = 0;
    }
}