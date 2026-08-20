using System.Collections.Generic;
using UnityEngine;
public enum ZoneType
{
    Forest,
    Cliff,
    Village,
    Underwater,
    Coast,
    Sea,
    Cave,
    Basement,
    Lab,
    City
}
public enum RoomType
{
    None,
    Start,
    Enemy,
    Boss,
    Event,
    Shop,
    GGangGGang,
    Reward
}
public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;
    [Header("Enemy")]
    public List<EnemyPoolData> enemyPools =
    new List<EnemyPoolData>();

    [Header("Current")]
    public Room currentRoom;

    [Header("Map")]
    public ZoneType currentZone =
        ZoneType.Forest;

    public List<RoomPoolData> roomPools =
        new List<RoomPoolData>();

    public Transform roomParent;

    List<RoomNode> allNodes =
        new List<RoomNode>();

    RoomNode currentNode;

    int nodeId = 0;

    private void Awake()
    {
        instance = this;
    }
    public int GetCurrentNodeId()
    {
        if (currentNode == null)
            return -1;

        return currentNode.id;
    }

    public void ClearCurrentRoom()
    {
        if (currentNode == null)
            return;

        currentNode.cleared = true;
    }
    public void EnterNode(RoomNode node)
    {
        currentNode = node;
        currentZone = node.zoneType;

        if (currentRoom != null)
        {
            Destroy(currentRoom.gameObject);
        }

        Room prefab =
            GetRandomRoomPrefab(
                node.zoneType,
                node.roomType
            );

        if (prefab == null)
        {
            Debug.LogError(
                node.zoneType +
                " / " +
                node.roomType +
                " 방 프리팹 없음"
            );
            return;
        }

        currentRoom =
            Instantiate(
                prefab,
                roomParent
            );

        currentRoom.Setup(node);
    }

    Room GetRandomRoomPrefab(
        ZoneType zone,
        RoomType type
    )
    {
        foreach (RoomPoolData pool
            in roomPools)
        {
            if (pool.zoneType == zone &&
                pool.roomType == type)
            {
                if (pool.roomPrefabs == null ||
                    pool.roomPrefabs.Length <= 0)
                    return null;

                return pool.roomPrefabs[
                    Random.Range(
                        0,
                        pool.roomPrefabs.Length
                    )
                ];
            }
        }

        return null;
    }
    public Enemy[] SpawnEnemiesForRoom(Room room)
    {
        EnemyPoolData pool =
            GetEnemyPool(room.zoneType);

        if (pool == null ||
            pool.enemyPrefabs.Length <= 0)
        {
            return new Enemy[0];
        }

        int count =
            Random.Range(1, room.enemySpawnPoints.Length + 1);

        List<Enemy> spawned =
            new List<Enemy>();

        for (int i = 0;
            i < count;
            i++)
        {
            Enemy prefab =
                pool.enemyPrefabs[
                    Random.Range(
                        0,
                        pool.enemyPrefabs.Length
                    )
                ];

            Enemy enemy =
                Instantiate(
                    prefab,
                    room.enemySpawnPoints[i].position,
                    Quaternion.identity,
                    room.transform
                );

            spawned.Add(enemy);
        }

        return spawned.ToArray();
    }

    EnemyPoolData GetEnemyPool(ZoneType zone)
    {
        foreach (EnemyPoolData pool
            in enemyPools)
        {
            if (pool.zoneType == zone)
                return pool;
        }
        return null;
    }
    [Header("Run Setting")]
    public int roomsPerZone = 8;

    private void Start()
    {
        StartRun();
    }

    public void StartRun()
    {
        GenerateMap(currentZone);

        if (allNodes.Count > 0)
        {
            EnterNode(allNodes[0]);
        }
    }

    void GenerateMap(ZoneType zone)
    {
        allNodes.Clear();
        nodeId = 0;

        List<RoomType> roomTypes =
            CreateZoneRoomTypes();

        RoomNode previousNode = null;

        for (int i = 0;
            i < roomTypes.Count;
            i++)
        {
            RoomNode node =
                new RoomNode(
                    nodeId++,
                    i,
                    zone,
                    roomTypes[i]
                );

            allNodes.Add(node);

            if (previousNode != null)
            {
                previousNode.nextRooms.Add(node);
            }

            previousNode = node;
        }
    }

    List<RoomType> CreateZoneRoomTypes()
    {
        List<RoomType> middleRooms =
            new List<RoomType>();

        middleRooms.Add(RoomType.Enemy);
        middleRooms.Add(RoomType.Event);
        middleRooms.Add(RoomType.Reward);
        middleRooms.Add(RoomType.Shop);
        middleRooms.Add(RoomType.GGangGGang);
        middleRooms.Add(RoomType.Enemy);

        Shuffle(middleRooms);

        List<RoomType> result =
            new List<RoomType>();

        result.Add(RoomType.Start);

        foreach (RoomType type in middleRooms)
        {
            result.Add(type);
        }

        result.Add(RoomType.Boss);

        return result;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0;
            i < list.Count;
            i++)
        {
            int randomIndex =
                Random.Range(i, list.Count);

            T temp = list[i];

            list[i] =
                list[randomIndex];

            list[randomIndex] =
                temp;
        }
    }

    public void MoveToNext(int index)
    {
        if (currentNode == null)
            return;

        if (BattleManager.instance != null &&
            BattleManager.instance.isBattle)
            return;

        if (index < 0 ||
            index >= currentNode.nextRooms.Count)
            return;

        EnterNode(currentNode.nextRooms[index]);
    }

    public void LoadNode(int id)
    {
        foreach (RoomNode node in allNodes)
        {
            if (node.id == id)
            {
                EnterNode(node);
                return;
            }
        }
    }
}