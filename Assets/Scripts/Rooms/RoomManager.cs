using System.Collections.Generic;
using UnityEngine;
public enum ZoneType
{
    Forest,
    Coast,
    Underwater,
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

    // ── 보상방 ─────────────────────────────────────────────
    Fountain,       // 분수 → 확정 버프 획득
    SageStone,      // 현자의 석판 → 경험치 획득
    TrainingRoom,   // 훈련 교관 → 돈 지불로 능력치 업

    // ── 마을방 ─────────────────────────────────────────────
    ItemShop,       // 철물점 (아이템 상점)
    RepairShop,     // 수리점 (여관 - 체력 회복)
    Blacksmith,     // 대장간 (장비 강화)

    // ── 기타 ───────────────────────────────────────────────
    Altar,          // 제단 (버프 혹은 디버프 획득)
    GamblingRoom,   // 도박방 (돈 도박)
    Archive,        // 기록 보관소 (스토리/로어)

    // ── 적 환경방 ───────────────────────────────────────────
    GrassRoom,      // 풀이 가득한 방
    FloodedRoom,    // 물이 차있는 방
    CloudRoom,      // 구름 방
    PollutedRoom,   // 오염된 방
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
    public int roomsPerZone = 13;

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
            CreateZoneRoomTypes(zone);

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

    // 각 지역마다 총 13개 방 생성 (시작 1 + 중간 11 + 보스 1 = 13개)
    // 조건: 적방 최소 4개, 기타방 최소 2개, 마을방 최소 2개, 보상방 최소 2개
    List<RoomType> CreateZoneRoomTypes(ZoneType zone)
    {
        List<RoomType> middleRooms = new List<RoomType>();

        // 1. 적방 카테고리 (최소 4개)
        for (int i = 0; i < 4; i++)
        {
            middleRooms.Add(GetRandomEnemyRoomType(zone));
        }

        // 2. 기타방 카테고리 (최소 2개)
        RoomType[] etcRooms = new RoomType[] { RoomType.Altar, RoomType.GamblingRoom, RoomType.Archive };
        for (int i = 0; i < 2; i++)
        {
            middleRooms.Add(etcRooms[Random.Range(0, etcRooms.Length)]);
        }

        // 3. 마을방 카테고리 (최소 2개)
        RoomType[] townRooms = new RoomType[] { RoomType.ItemShop, RoomType.RepairShop, RoomType.Blacksmith };
        for (int i = 0; i < 2; i++)
        {
            middleRooms.Add(townRooms[Random.Range(0, townRooms.Length)]);
        }

        // 4. 보상방 카테고리 (최소 2개)
        RoomType[] rewardRooms = new RoomType[] { RoomType.Fountain, RoomType.SageStone, RoomType.TrainingRoom };
        for (int i = 0; i < 2; i++)
        {
            middleRooms.Add(rewardRooms[Random.Range(0, rewardRooms.Length)]);
        }

        // 5. 남은 1개 중간 방 (총 11개 맞춤: 4+2+2+2 = 10개 -> +1개 추가)
        List<RoomType> allCandidates = new List<RoomType>();
        allCandidates.Add(GetRandomEnemyRoomType(zone));
        allCandidates.Add(etcRooms[Random.Range(0, etcRooms.Length)]);
        allCandidates.Add(townRooms[Random.Range(0, townRooms.Length)]);
        allCandidates.Add(rewardRooms[Random.Range(0, rewardRooms.Length)]);
        middleRooms.Add(allCandidates[Random.Range(0, allCandidates.Count)]);

        // 6. 중간 방들 무작위 셔플
        Shuffle(middleRooms);

        // 7. 결과 구성: Start(1개) + 중간방(11개) + Boss(1개) = 총 13개
        List<RoomType> result = new List<RoomType>();
        result.Add(RoomType.Start);
        result.AddRange(middleRooms);
        result.Add(RoomType.Boss);

        return result;
    }

    // Zone에 적합한 적방/환경적방 반환
    RoomType GetRandomEnemyRoomType(ZoneType zone)
    {
        List<RoomType> enemyPool = new List<RoomType> { RoomType.Enemy };

        switch (zone)
        {
            case ZoneType.Forest:
                enemyPool.Add(RoomType.GrassRoom);
                enemyPool.Add(RoomType.PollutedRoom);
                break;
            case ZoneType.Coast:
                enemyPool.Add(RoomType.FloodedRoom);
                enemyPool.Add(RoomType.CloudRoom);
                break;
            case ZoneType.Underwater:
                enemyPool.Add(RoomType.FloodedRoom);
                break;
            case ZoneType.Basement:
                enemyPool.Add(RoomType.PollutedRoom);
                enemyPool.Add(RoomType.CloudRoom);
                break;
            case ZoneType.Lab:
                enemyPool.Add(RoomType.PollutedRoom);
                break;
            case ZoneType.City:
                enemyPool.Add(RoomType.CloudRoom);
                enemyPool.Add(RoomType.PollutedRoom);
                break;
        }

        return enemyPool[Random.Range(0, enemyPool.Count)];
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