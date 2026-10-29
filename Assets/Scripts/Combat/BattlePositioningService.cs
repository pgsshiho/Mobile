using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 중 파티/적 배치와 적 열 정보를 담당한다.
/// BattleManager는 전투 흐름과 UI만 조정하도록 분리한다.
/// </summary>
public sealed class BattlePositioningService
{
    private readonly Dictionary<Enemy, int> enemyColumns = new Dictionary<Enemy, int>();
    private readonly List<Enemy> aliveEnemies = new List<Enemy>();

    public IReadOnlyDictionary<Enemy, int> EnemyColumns => enemyColumns;

    public void Setup(Room room, PartyManager partyManager, Transform[] partyPositions, Transform[] enemyPositions)
    {
        enemyColumns.Clear();

        if (partyManager != null && partyPositions != null && partyPositions.Length > 0)
            partyManager.PlacePartyAtPositions(partyPositions);

        if (room?.enemies == null)
            return;

        for (int i = 0; i < room.enemies.Length; i++)
        {
            Enemy enemy = room.enemies[i];
            if (enemy == null)
                continue;

            Transform point = FindSpawnPoint(room, enemyPositions, i);
            if (point != null)
            {
                enemy.transform.SetPositionAndRotation(point.position, point.rotation);
            }

            enemy.gameObject.SetActive(true);
            enemyColumns[enemy] = i;
        }
    }

    public void DisableEnemies()
    {
        foreach (Enemy enemy in enemyColumns.Keys)
        {
            if (enemy != null)
                enemy.gameObject.SetActive(false);
        }
        enemyColumns.Clear();
    }

    public void Rearrange(Transform[] enemyPositions)
    {
        if (enemyPositions == null || enemyPositions.Length == 0)
            return;

        aliveEnemies.Clear();
        foreach (KeyValuePair<Enemy, int> pair in enemyColumns)
        {
            Enemy enemy = pair.Key;
            if (enemy != null && enemy.gameObject.activeSelf && enemy.health > 0)
                aliveEnemies.Add(enemy);
        }

        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            Enemy enemy = aliveEnemies[i];
            if (i < enemyPositions.Length && enemyPositions[i] != null)
            {
                Transform point = enemyPositions[i];
                enemy.transform.SetPositionAndRotation(point.position, point.rotation);
            }
            enemyColumns[enemy] = i;
        }
    }

    private static Transform FindSpawnPoint(Room room, Transform[] enemyPositions, int index)
    {
        if (enemyPositions != null && index < enemyPositions.Length && enemyPositions[index] != null)
            return enemyPositions[index];

        if (room.enemySpawnPoints != null && index < room.enemySpawnPoints.Length)
            return room.enemySpawnPoints[index];

        return null;
    }
}
