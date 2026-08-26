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
    Reward,
    // 세부 방 종류
    ItemShop,
    Blacksmith,
    RepairShop,
    Fountain,
    SageStone,
    TrainingRoom,
    GrassRoom,
    FloodedRoom,
    CloudRoom,
    PollutedRoom,
    EliteEnemy
}

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    [Header("Zone & Room Settings")]
    [Tooltip("한 Zone당 생성할 총 방의 개수 (기본 13개)")]
    [Min(10)]
    public int roomsPerZone = 13;

    [Header("Enemy")]
    public List<EnemyPoolData> enemyPools = new List<EnemyPoolData>();

    [Header("Current")]
    public Room currentRoom;

    [Header("Map")]
    public ZoneType currentZone = ZoneType.Forest;
    public List<RoomPoolData> roomPools = new List<RoomPoolData>();
    public Transform roomParent;

    [Header("Debug / Inspector View")]
    public List<RoomNode> allNodes = new List<RoomNode>();
    [SerializeField] private List<string> generatedMapOverview = new List<string>();

    RoomNode currentNode;
    int nodeId = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (allNodes.Count == 0)
        {
            StartRun();
        }
    }

    public int GetCurrentNodeId()
    {
        if (currentNode == null)
            return -1;

        return currentNode.id;
    }

    public RoomNode GetCurrentNode()
    {
        return currentNode;
    }

    public void StartRun()
    {
        GenerateMap();

        if (allNodes.Count > 0)
        {
            EnterNode(allNodes[0]);
        }
    }

    // ────────────────────────────────────────────────────────────────
    //  맵 생성 로직 (로그라이크 분기 맵 생성)
    // ────────────────────────────────────────────────────────────────

    public void GenerateMap()
    {
        allNodes.Clear();
        generatedMapOverview.Clear();
        nodeId = 0;

        // 1. 방 타입 리스트 구성 (최소 보장 규칙 적용)
        List<RoomType> roomTypes = CreateZoneRoomTypes(currentZone, roomsPerZone);

        // 2. RoomNode 인스턴스 생성
        for (int i = 0; i < roomTypes.Count; i++)
        {
            RoomNode node = new RoomNode(nodeId++, i, currentZone, roomTypes[i]);
            allNodes.Add(node);
        }

        // 3. 4방향 배치 및 분기 연결
        BuildBranchingConnections(allNodes);

        // 4. 인스펙터 및 콘솔 디버그 출력
        foreach (var node in allNodes)
        {
            string connections = "";
            if (node.forwardRoom != null) connections += $"[전방: Node{node.forwardRoom.id}({node.forwardRoom.roomType})] ";
            if (node.backwardRoom != null) connections += $"[후방: Node{node.backwardRoom.id}({node.backwardRoom.roomType})] ";
            if (node.leftRoom != null) connections += $"[좌측: Node{node.leftRoom.id}({node.leftRoom.roomType})] ";
            if (node.rightRoom != null) connections += $"[우측: Node{node.rightRoom.id}({node.rightRoom.roomType})] ";

            string info = $"Node {node.id,2} (F{node.floor,2}) [{node.roomType,-12}] -> 연결: {(string.IsNullOrEmpty(connections) ? "없음" : connections)}";
            generatedMapOverview.Add(info);
        }

        Debug.Log($"<color=cyan>[RoomManager]</color> {currentZone} 맵 생성 완료! (총 {allNodes.Count}개 방, 시작 방: Node 0)");
    }

    /// <summary>
    /// 최소 방 규칙을 적용하여 방 목록을 생성합니다.
    /// 적 최소 3, 보상 최소 1, 마을 최소 2, 기타 최소 1, 시작방 1, 빈방 최소 1 + 보스 1 (마지막)
    /// </summary>
    private List<RoomType> CreateZoneRoomTypes(ZoneType zone, int totalCount)
    {
        List<RoomType> types = new List<RoomType>();

        // [필수 1] 시작 방 1개 (항상 0번 인덱스)
        types.Add(RoomType.Start);

        // [필수 2] 보스 방 1개 (항상 마지막 인덱스)
        int middleCount = totalCount - 2;

        List<RoomType> middleRooms = new List<RoomType>();

        // [최소 보장 1] 적 방 최소 3개
        for (int i = 0; i < 3; i++)
            middleRooms.Add(GetRandomEnemyRoomType(zone));

        // [최소 보장 2] 보상 방 최소 1개
        middleRooms.Add(GetRandomRewardRoomType());

        // [최소 보장 3] 마을 방(상점/대장간/수리점) 최소 2개
        middleRooms.Add(GetRandomVillageRoomType());
        middleRooms.Add(GetRandomVillageRoomType());

        // [최소 보장 4] 기타 방(이벤트 등) 최소 1개
        middleRooms.Add(RoomType.Event);

        // [최소 보장 5] 아무것도 없는 방 최소 1개
        middleRooms.Add(RoomType.None);

        // [남은 방] 랜덤하게 채움
        while (middleRooms.Count < middleCount)
        {
            int r = Random.Range(0, 5);
            switch (r)
            {
                case 0: middleRooms.Add(GetRandomEnemyRoomType(zone)); break;
                case 1: middleRooms.Add(GetRandomRewardRoomType()); break;
                case 2: middleRooms.Add(GetRandomVillageRoomType()); break;
                case 3: middleRooms.Add(RoomType.Event); break;
                case 4: middleRooms.Add(RoomType.None); break;
            }
        }

        // 중간 방들 셔플
        ShuffleList(middleRooms);

        // 최종 리스트 조합: Start -> Middle Rooms -> Boss
        types.AddRange(middleRooms);
        types.Add(RoomType.Boss);

        return types;
    }

    private RoomType GetRandomEnemyRoomType(ZoneType zone)
    {
        List<RoomType> list = new List<RoomType> { RoomType.Enemy };
        switch (zone)
        {
            case ZoneType.Forest: list.Add(RoomType.GrassRoom); break;
            case ZoneType.Underwater: list.Add(RoomType.FloodedRoom); break;
            case ZoneType.Cliff: list.Add(RoomType.CloudRoom); break;
            case ZoneType.Lab: list.Add(RoomType.PollutedRoom); break;
        }
        list.Add(RoomType.EliteEnemy);
        return list[Random.Range(0, list.Count)];
    }

    private RoomType GetRandomRewardRoomType()
    {
        RoomType[] rewards = { RoomType.Reward, RoomType.Fountain, RoomType.SageStone, RoomType.TrainingRoom };
        return rewards[Random.Range(0, rewards.Length)];
    }

    private RoomType GetRandomVillageRoomType()
    {
        RoomType[] village = { RoomType.ItemShop, RoomType.Blacksmith, RoomType.RepairShop, RoomType.Shop };
        return village[Random.Range(0, village.Length)];
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }

    /// <summary>
    /// 방 노드들을 전방/후방/좌측/우측 4방향으로 배치하고 연결합니다.
    /// </summary>
    private void BuildBranchingConnections(List<RoomNode> nodes)
    {
        if (nodes == null || nodes.Count == 0) return;

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            RoomNode curr = nodes[i];
            RoomNode next = nodes[i + 1];

            // 1. 주 진행 방향 (전방 연결)
            curr.forwardRoom = next;
            next.backwardRoom = curr;
            next.previousRoom = curr;
            curr.nextRooms.Add(next);

            // 2. 가끔 좌/우 분기 생성 (중간 노드들)
            if (i >= 1 && i <= nodes.Count - 3)
            {
                int branchRoll = Random.Range(0, 100);
                if (branchRoll < 40 && i + 2 < nodes.Count)
                {
                    RoomNode sideNode = nodes[i + 2];
                    if (branchRoll < 20 && curr.leftRoom == null)
                    {
                        curr.leftRoom = sideNode;
                        if (!curr.nextRooms.Contains(sideNode)) curr.nextRooms.Add(sideNode);
                    }
                    else if (curr.rightRoom == null)
                    {
                        curr.rightRoom = sideNode;
                        if (!curr.nextRooms.Contains(sideNode)) curr.nextRooms.Add(sideNode);
                    }
                }
            }
        }
    }

    // ────────────────────────────────────────────────────────────────
    //  방 이동 로직 (화살표 버튼 호출용)
    // ────────────────────────────────────────────────────────────────

    public void MoveForward()
    {
        if (currentNode != null && currentNode.forwardRoom != null)
        {
            EnterNode(currentNode.forwardRoom);
        }
    }

    public void MoveBackward()
    {
        if (currentNode != null)
        {
            RoomNode target = currentNode.previousRoom != null ? currentNode.previousRoom : currentNode.backwardRoom;
            if (target != null)
            {
                EnterNode(target);
            }
        }
    }

    public void MoveLeft()
    {
        if (currentNode != null && currentNode.leftRoom != null)
        {
            EnterNode(currentNode.leftRoom);
        }
    }

    public void MoveRight()
    {
        if (currentNode != null && currentNode.rightRoom != null)
        {
            EnterNode(currentNode.rightRoom);
        }
    }

    public void MoveToNext(int index)
    {
        if (currentNode == null) return;
        if (index < 0 || index >= currentNode.nextRooms.Count) return;

        EnterNode(currentNode.nextRooms[index]);
    }

    // ────────────────────────────────────────────────────────────────
    //  방 진입 및 프리팹 스폰
    // ────────────────────────────────────────────────────────────────

    public void EnterNode(RoomNode node)
    {
        currentNode = node;

        if (currentRoom != null)
        {
            Destroy(currentRoom.gameObject);
        }

        Room prefab = GetRandomRoomPrefab(node.zoneType, node.roomType);

        if (prefab != null)
        {
            Transform parentToUse = roomParent != null ? roomParent : transform;
            currentRoom = Instantiate(prefab, parentToUse);
            currentRoom.Setup(node);
        }
        else
        {
            Debug.LogWarning($"[RoomManager] {node.zoneType} / {node.roomType} 에 해당하는 방 프리팹이 없습니다.");
        }

        // 방향 화살표 UI 갱신
        if (RoomNavigationUI.instance != null)
        {
            RoomNavigationUI.instance.Refresh(node);
        }
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

    public void ClearCurrentRoom()
    {
        if (currentNode != null)
        {
            currentNode.cleared = true;
        }

        // 전투 종료 후 화살표 UI 다시 활성화
        if (RoomNavigationUI.instance != null && currentNode != null)
        {
            RoomNavigationUI.instance.Refresh(currentNode);
        }
    }

    // ────────────────────────────────────────────────────────────────
    //  프리팹 풀 조회
    // ────────────────────────────────────────────────────────────────

    Room GetRandomRoomPrefab(ZoneType zone, RoomType type)
    {
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
}