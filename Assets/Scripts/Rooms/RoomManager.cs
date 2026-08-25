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
    None,           // 아무것도 없는 빈 방 (안전 지대)
    Start,          // 시작방
    Enemy,          // 일반 적방
    Boss,           // 보스방

    // ── 보상방 ─────────────────────────────────────────────
    Fountain,       // 분수 → 확정 버프 획득
    SageStone,      // 현자의 석판 → 경험치 획득
    TrainingRoom,   // 훈련 교관 → 돈 지불로 능력치 업

    // ── 마을방 ─────────────────────────────────────────────
    ItemShop,       // 철물점 (아이템 상점)
    RepairShop,     // 수리점 (여관 - 체력 회복)
    Blacksmith,     // 대장간 (장비 강화)

    // ── 기타방 ─────────────────────────────────────────────
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
    public List<EnemyPoolData> enemyPools = new List<EnemyPoolData>();

    [Header("Current")]
    public Room currentRoom;

    [Header("Map Settings")]
    public ZoneType currentZone = ZoneType.Forest;

    [Tooltip("Zone당 생성할 총 방의 개수 (최소 10개: 시작1 + 보스1 + 적방3 + 보상1 + 마을2 + 기타1 + 빈방1)")]
    [Min(10)]
    public int roomsPerZone = 13;

    public List<RoomPoolData> roomPools = new List<RoomPoolData>();

    public Transform roomParent;

    [Header("Debug / Map Overview (현재 생성된 맵 노드 목록)")]
    public List<string> generatedMapOverview = new List<string>();
    public List<RoomNode> allNodes = new List<RoomNode>();

    RoomNode currentNode;

    int nodeId = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartRun();
    }

    public int GetCurrentNodeId()
    {
        if (currentNode == null)
            return -1;

        return currentNode.id;
    }

    public RoomNode GetCurrentNode() => currentNode;

    public List<RoomNode> GetNextRooms() => (currentNode != null) ? currentNode.nextRooms : new List<RoomNode>();

    public void ClearCurrentRoom()
    {
        if (currentNode == null)
            return;

        currentNode.cleared = true;
    }

    public void StartRun()
    {
        GenerateMap(currentZone);

        if (allNodes.Count > 0)
        {
            EnterNode(allNodes[0]);
        }
    }

    /// <summary>
    /// 로그라이크 맵을 생성하고 노드 간의 4방향(전방/후방/좌측/우측) 연결을 구성합니다.
    /// </summary>
    void GenerateMap(ZoneType zone)
    {
        allNodes.Clear();
        generatedMapOverview.Clear();
        nodeId = 0;

        List<RoomType> roomTypes = CreateZoneRoomTypes(zone);
        RoomNode previousNode = null;

        for (int i = 0; i < roomTypes.Count; i++)
        {
            RoomNode node = new RoomNode(
                nodeId++,
                i,
                zone,
                roomTypes[i]
            );

            allNodes.Add(node);
            generatedMapOverview.Add($"[{node.id}] {node.roomType}");

            if (previousNode != null)
            {
                previousNode.nextRooms.Add(node);
                previousNode.forwardRoom = node;
                node.previousRoom = previousNode;
                node.backwardRoom = previousNode;
            }

            previousNode = node;
        }

        // 4방향(좌/우) 분기 경로 연결
        BuildBranchingConnections();

        Debug.Log($"[RoomManager] {zone} 구역 맵 생성 완료 (총 {allNodes.Count}개 방):\n" + string.Join(" ➔ ", generatedMapOverview));
    }

    /// <summary>
    /// 조건에 맞추어 Zone의 방 목록을 구성합니다.
    /// 최소 보장: 시작방 1, 적방 3, 보상방 1, 마을방 2, 기타 1, 아무것도 없는 방 1, 보스 1 (총 최소 10개)
    /// roomsPerZone의 남은 개수는 무작위 카테고리에서 채웁니다.
    /// </summary>
    List<RoomType> CreateZoneRoomTypes(ZoneType zone)
    {
        int total = Mathf.Max(10, roomsPerZone);
        int middleCount = total - 2; // 시작방(1개)과 보스방(1개)을 제외한 중간 방 수

        List<RoomType> middleRooms = new List<RoomType>();

        // 1. 적방 카테고리 (최소 3개)
        for (int i = 0; i < 3; i++)
        {
            middleRooms.Add(GetRandomEnemyRoomType(zone));
        }

        // 2. 보상방 카테고리 (최소 1개)
        RoomType[] rewardRooms = new RoomType[] { RoomType.Fountain, RoomType.SageStone, RoomType.TrainingRoom };
        middleRooms.Add(rewardRooms[Random.Range(0, rewardRooms.Length)]);

        // 3. 마을방 카테고리 (최소 2개)
        RoomType[] townRooms = new RoomType[] { RoomType.ItemShop, RoomType.RepairShop, RoomType.Blacksmith };
        for (int i = 0; i < 2; i++)
        {
            middleRooms.Add(townRooms[Random.Range(0, townRooms.Length)]);
        }

        // 4. 기타방 카테고리 (최소 1개)
        RoomType[] etcRooms = new RoomType[] { RoomType.Altar, RoomType.GamblingRoom, RoomType.Archive };
        middleRooms.Add(etcRooms[Random.Range(0, etcRooms.Length)]);

        // 5. 아무것도 없는 방 (최소 1개)
        middleRooms.Add(RoomType.None);

        // 필수 중간방 개수: 3 + 1 + 2 + 1 + 1 = 8개
        int requiredMiddleCount = 8;
        int remaining = middleCount - requiredMiddleCount;

        // 6. 남은 방들을 랜덤 카테고리에서 추가
        for (int i = 0; i < remaining; i++)
        {
            int category = Random.Range(0, 5);
            switch (category)
            {
                case 0: // 적방 추가
                    middleRooms.Add(GetRandomEnemyRoomType(zone));
                    break;
                case 1: // 보상방 추가
                    middleRooms.Add(rewardRooms[Random.Range(0, rewardRooms.Length)]);
                    break;
                case 2: // 마을방 추가
                    middleRooms.Add(townRooms[Random.Range(0, townRooms.Length)]);
                    break;
                case 3: // 기타방 추가
                    middleRooms.Add(etcRooms[Random.Range(0, etcRooms.Length)]);
                    break;
                case 4: // 빈방 추가
                    middleRooms.Add(RoomType.None);
                    break;
            }
        }

        // 7. 중간 방들 무작위 셔플
        Shuffle(middleRooms);

        // 8. 최종 방 리스트: 시작방(1) + 중간방들(셔플) + 보스방(1) = 총 roomsPerZone개
        List<RoomType> result = new List<RoomType>();
        result.Add(RoomType.Start);
        result.AddRange(middleRooms);
        result.Add(RoomType.Boss);

        return result;
    }

    /// <summary>
    /// 분기 경로를 형성하여 좌측(leftRoom) / 우측(rightRoom) 갈림길을 연결합니다.
    /// </summary>
    void BuildBranchingConnections()
    {
        for (int i = 0; i < allNodes.Count - 1; i++)
        {
            RoomNode current = allNodes[i];

            // 좌측 갈림길 (대체 경로 / 지름길)
            if (i + 2 < allNodes.Count && i % 3 == 1)
            {
                current.leftRoom = allNodes[i + 2];
                if (!current.nextRooms.Contains(allNodes[i + 2]))
                {
                    current.nextRooms.Add(allNodes[i + 2]);
                }
            }

            // 우측 갈림길
            if (i + 2 < allNodes.Count && i % 3 == 2)
            {
                current.rightRoom = allNodes[i + 2];
                if (!current.nextRooms.Contains(allNodes[i + 2]))
                {
                    current.nextRooms.Add(allNodes[i + 2]);
                }
            }
        }
    }

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

    public void EnterNode(RoomNode node)
    {
        if (node == null) return;

        currentNode = node;
        currentZone = node.zoneType;

        // 기존 방 제거 (단, roomParent 컨테이너 자체는 파괴되지 않도록 보호)
        if (currentRoom != null)
        {
            if (roomParent == null || currentRoom.gameObject != roomParent.gameObject)
            {
                Destroy(currentRoom.gameObject);
            }
            currentRoom = null;
        }

        Room prefab = GetRandomRoomPrefab(node.zoneType, node.roomType);

        if (prefab == null)
        {
            Debug.LogError($"[RoomManager] {node.zoneType} / {node.roomType} 방 프리팹이 roomPools에 없습니다.");
            return;
        }

        currentRoom = Instantiate(prefab, roomParent);
        currentRoom.gameObject.name = $"{node.zoneType}_{node.roomType}_Node{node.id}";
        currentRoom.Setup(node);

        Debug.Log($"[RoomManager] 방 생성 완료: {currentRoom.gameObject.name} (Parent: {(roomParent != null ? roomParent.name : "Root")})");

        // 화살표 버튼 자동 갱신
        if (RoomNavigationUI.instance != null)
        {
            RoomNavigationUI.instance.Refresh(node);
        }
    }

    Room GetRandomRoomPrefab(ZoneType zone, RoomType type)
    {
        // 1. 해당 Zone 및 RoomType에 해당하는 풀 검색
        foreach (RoomPoolData pool in roomPools)
        {
            if (pool != null && pool.zoneType == zone && pool.roomType == type)
            {
                if (pool.roomPrefabs != null && pool.roomPrefabs.Length > 0)
                {
                    return pool.roomPrefabs[Random.Range(0, pool.roomPrefabs.Length)];
                }
            }
        }

        // 2. None(아무것도 없는 방)인 경우 Start(빈 방) 프리팹을 기본으로 활용
        if (type == RoomType.None)
        {
            return GetRandomRoomPrefab(zone, RoomType.Start);
        }

        return null;
    }

    public Enemy[] SpawnEnemiesForRoom(Room room)
    {
        EnemyPoolData pool = GetEnemyPool(room.zoneType);

        if (pool == null || pool.enemyPrefabs == null || pool.enemyPrefabs.Length <= 0)
        {
            return new Enemy[0];
        }

        // 스폰 포인트: BattleManager의 Enemy 위치 배열을 우선 사용, 없으면 room.enemySpawnPoints 사용
        Transform[] spawnPoints = (BattleManager.instance != null && BattleManager.instance.Enemy != null && BattleManager.instance.Enemy.Length > 0)
            ? BattleManager.instance.Enemy
            : room.enemySpawnPoints;

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return new Enemy[0];
        }

        int count = Random.Range(1, spawnPoints.Length + 1);
        List<Enemy> spawned = new List<Enemy>();

        for (int i = 0; i < count; i++)
        {
            Enemy prefab = pool.enemyPrefabs[Random.Range(0, pool.enemyPrefabs.Length)];
            if (prefab == null) continue;

            Transform spawnPoint = spawnPoints[i];
            Vector3 pos = (spawnPoint != null) ? spawnPoint.position : Vector3.zero;
            Quaternion rot = (spawnPoint != null) ? spawnPoint.rotation : Quaternion.identity;

            Enemy enemy = Instantiate(
                prefab,
                pos,
                rot,
                room.transform
            );

            spawned.Add(enemy);
        }

        return spawned.ToArray();
    }

    EnemyPoolData GetEnemyPool(ZoneType zone)
    {
        foreach (EnemyPoolData pool in enemyPools)
        {
            if (pool != null && pool.zoneType == zone)
                return pool;
        }
        return null;
    }

    // ============================================================
    // 방향별 방 이동 API (버튼 연동용)
    // ============================================================

    /// <summary>전방(다음 방)으로 이동</summary>
    public void MoveForward()
    {
        if (!CanMove()) return;

        if (currentNode.forwardRoom != null)
        {
            EnterNode(currentNode.forwardRoom);
        }
        else if (currentNode.nextRooms != null && currentNode.nextRooms.Count > 0)
        {
            EnterNode(currentNode.nextRooms[0]);
        }
        else
        {
            Debug.Log("[RoomManager] 전방에 연결된 방이 없습니다.");
        }
    }

    /// <summary>후방(이전 방)으로 이동</summary>
    public void MoveBackward()
    {
        if (!CanMove()) return;

        if (currentNode.previousRoom != null)
        {
            EnterNode(currentNode.previousRoom);
        }
        else if (currentNode.backwardRoom != null)
        {
            EnterNode(currentNode.backwardRoom);
        }
        else
        {
            Debug.Log("[RoomManager] 이전 방이 없습니다.");
        }
    }

    /// <summary>좌측 분기 방으로 이동</summary>
    public void MoveLeft()
    {
        if (!CanMove()) return;

        if (currentNode.leftRoom != null)
        {
            EnterNode(currentNode.leftRoom);
        }
        else if (currentNode.nextRooms != null && currentNode.nextRooms.Count > 1)
        {
            EnterNode(currentNode.nextRooms[1]);
        }
        else
        {
            Debug.Log("[RoomManager] 좌측에 연결된 방이 없습니다.");
        }
    }

    /// <summary>우측 분기 방으로 이동</summary>
    public void MoveRight()
    {
        if (!CanMove()) return;

        if (currentNode.rightRoom != null)
        {
            EnterNode(currentNode.rightRoom);
        }
        else if (currentNode.nextRooms != null && currentNode.nextRooms.Count > 2)
        {
            EnterNode(currentNode.nextRooms[2]);
        }
        else
        {
            Debug.Log("[RoomManager] 우측에 연결된 방이 없습니다.");
        }
    }

    /// <summary>인덱스 기반 다음 방 이동</summary>
    public void MoveToNext(int index = 0)
    {
        if (!CanMove()) return;

        if (currentNode.nextRooms == null || currentNode.nextRooms.Count == 0)
        {
            Debug.Log("[RoomManager] 현재 구역의 마지막 방입니다!");
            return;
        }

        if (index < 0 || index >= currentNode.nextRooms.Count)
            index = 0;

        EnterNode(currentNode.nextRooms[index]);
    }

    private bool CanMove()
    {
        if (currentNode == null) return false;

        if (BattleManager.instance != null && BattleManager.instance.isBattle)
        {
            Debug.Log("[RoomManager] 전투 중에는 방을 이동할 수 없습니다.");
            return false;
        }

        return true;
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

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}