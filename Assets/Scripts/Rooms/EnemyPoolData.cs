using UnityEngine;

[CreateAssetMenu(menuName = "Map/Enemy Pool")]
public class EnemyPoolData : ScriptableObject
{
    public ZoneType zoneType;

    public Enemy[] enemyPrefabs;
}