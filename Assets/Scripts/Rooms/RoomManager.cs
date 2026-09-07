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
    AddRobot,

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
    [Tooltip("한 Zone당 생성할 총 방의 개수 (필수 방 포함 최소 10개, 기본 13개)")]
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

    [SerializeField]
    private List<string> generatedMapOverview = new List<string>();

    // 현재 방
    private RoomNode currentNode;

    // 방 ID
    private int nodeId = 0;

    // 노드별로 한 번만 생성한 방을 보관한다.
    // 다시 방문할 때 전투/보상/상점 상태가 초기화되지 않도록 재사용한다.
    private Dictionary<RoomNode, Room> spawnedRooms =
        new Dictionary<RoomNode, Room>();

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

    // ============================================================
    // Current Node
    // ============================================================

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

    // ============================================================
    // 뒤로 가기 가능 여부
    // ============================================================

    public bool CanMoveBackward()
    {
        return currentNode != null &&
               currentNode.backwardRoom != null;
    }

    // ============================================================
    // 게임 시작
    // ============================================================

    public void StartRun()
    {
        GenerateMap();

        if (allNodes.Count > 0)
        {
            EnterNode(allNodes[0]);
        }
    }

    // ============================================================
    // 맵 생성
    // ============================================================

    public void GenerateMap()
    {
        ClearSpawnedRooms();
        allNodes.Clear();
        generatedMapOverview.Clear();
        nodeId = 0;

        // 1. 방 타입 리스트 구성
        List<RoomType> roomTypes =
            CreateZoneRoomTypes(currentZone, roomsPerZone);

        // 2. RoomNode 생성
        for (int i = 0; i < roomTypes.Count; i++)
        {
            RoomNode node =
                new RoomNode(
                    nodeId++,
                    i,
                    currentZone,
                    roomTypes[i]
                );

            allNodes.Add(node);
        }

        // 3. 연결 생성
        BuildBranchingConnections(allNodes);

        // 4. 맵의 모든 방을 한 번에 생성한다.
        // 이후 이동에서는 새 방을 생성하지 않고, 해당 방만 활성화한다.
        SpawnAllRooms();

        // 5. 디버그 정보
        foreach (var node in allNodes)
        {
            string connections = "";

            if (node.forwardRoom != null)
            {
                connections +=
                    $"[전방: Node{node.forwardRoom.id}({node.forwardRoom.roomType})] ";
            }

            if (node.backwardRoom != null)
            {
                connections +=
                    $"[후방: Node{node.backwardRoom.id}({node.backwardRoom.roomType})] ";
            }

            if (node.leftRoom != null)
            {
                connections +=
                    $"[좌측: Node{node.leftRoom.id}({node.leftRoom.roomType})] ";
            }

            if (node.rightRoom != null)
            {
                connections +=
                    $"[우측: Node{node.rightRoom.id}({node.rightRoom.roomType})] ";
            }

            string info =
                $"Node {node.id,2} " +
                $"(F{node.floor,2}) " +
                $"[{node.roomType,-12}] -> 연결: " +
                $"{(string.IsNullOrEmpty(connections) ? "없음" : connections)}";

            generatedMapOverview.Add(info);
        }

        Debug.Log(
            $"<color=cyan>[RoomManager]</color> " +
            $"{currentZone} 맵 생성 완료! " +
            $"(총 {allNodes.Count}개 방, 시작 방: Node 0)"
        );
    }

    // ============================================================
    // 방 타입 생성
    // ============================================================

    private List<RoomType> CreateZoneRoomTypes(
        ZoneType zone,
        int totalCount)
    {
        // 시작, 보스, 전투 3개, 보상 2개, 상점 2개, 빈 방을
        // 모두 보장하려면 최소 10개가 필요하다.
        totalCount = Mathf.Max(11, totalCount);

        List<RoomType> types = new List<RoomType>();

        // 시작 방
        types.Add(RoomType.Start);

        // 보스 방
        int middleCount = totalCount - 2;

        List<RoomType> middleRooms =
            new List<RoomType>();

        // 전투 방 최소 3개
        for (int i = 0; i < 3; i++)
        {
            middleRooms.Add(
                GetRandomEnemyRoomType(zone)
            );
        }
        middleRooms.Add(RoomType.AddRobot);
        // 보상 방 3종 중 서로 다른 2개를 필수로 넣는다.
        List<RoomType> requiredRewards = new List<RoomType>
        {
            RoomType.Fountain,
            RoomType.SageStone,
            RoomType.TrainingRoom
        };
        ShuffleList(requiredRewards);
        middleRooms.Add(requiredRewards[0]);
        middleRooms.Add(requiredRewards[1]);

        // 상점 방 3종 중 서로 다른 2개를 필수로 넣는다.
        List<RoomType> requiredShops = new List<RoomType>
        {
            RoomType.ItemShop,
            RoomType.Blacksmith,
            RoomType.RepairShop
        };
        ShuffleList(requiredShops);
        middleRooms.Add(requiredShops[0]);
        middleRooms.Add(requiredShops[1]);

        // 아무것도 없는 방도 최소 1개 포함한다.
        middleRooms.Add(RoomType.None);

        // 남은 방 랜덤 생성
        while (middleRooms.Count < middleCount)
        {
            int r = Random.Range(0, 4);

            switch (r)
            {
                case 0:
                    middleRooms.Add(
                        GetRandomEnemyRoomType(zone)
                    );
                    break;

                case 1:
                    middleRooms.Add(
                        GetRandomRewardRoomType()
                    );
                    break;

                case 2:
                    middleRooms.Add(
                        GetRandomVillageRoomType()
                    );
                    break;

                case 3:
                    middleRooms.Add(RoomType.None);
                    break;
            }
        }

        // 셔플
        ShuffleList(middleRooms);

        // Start -> Middle -> Boss
        types.AddRange(middleRooms);
        types.Add(RoomType.Boss);

        return types;
    }

    private RoomType GetRandomEnemyRoomType(
        ZoneType zone)
    {
        List<RoomType> list =
            new List<RoomType>
            {
                RoomType.Enemy
            };

        switch (zone)
        {
            case ZoneType.Forest:
                list.Add(RoomType.GrassRoom);
                break;

            case ZoneType.Underwater:
                list.Add(RoomType.FloodedRoom);
                break;

            case ZoneType.Cliff:
                list.Add(RoomType.CloudRoom);
                break;

            case ZoneType.Lab:
                list.Add(RoomType.PollutedRoom);
                break;
        }

        list.Add(RoomType.EliteEnemy);

        return list[
            Random.Range(0, list.Count)
        ];
    }

    private RoomType GetRandomRewardRoomType()
    {
        RoomType[] rewards =
        {
            RoomType.Fountain,
            RoomType.SageStone,
            RoomType.TrainingRoom
        };

        return rewards[
            Random.Range(0, rewards.Length)
        ];
    }

    private RoomType GetRandomVillageRoomType()
    {
        RoomType[] village =
        {
            RoomType.ItemShop,
            RoomType.Blacksmith,
            RoomType.RepairShop
        };

        return village[
            Random.Range(0, village.Length)
        ];
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd =
                Random.Range(i, list.Count);

            (list[i], list[rnd]) =
                (list[rnd], list[i]);
        }
    }

    // ============================================================
    // 맵 연결
    // ============================================================

    private void BuildBranchingConnections(
        List<RoomNode> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            return;

        // 모든 방을 한 번만 부모에 붙여 순환이 없는 랜덤 트리를 만든다.
        // 어떤 방향으로 들어왔든 반대 방향은 반드시 원래 방을 가리킨다.
        foreach (RoomNode node in nodes)
        {
            node.nextRooms.Clear();
            node.previousRoom = null;
            node.forwardRoom = null;
            node.backwardRoom = null;
            node.leftRoom = null;
            node.rightRoom = null;
        }

        RoomNode startNode = nodes[0];
        startNode.floor = 0;

        // 보스는 마지막에 가장 깊은 말단에 붙여, 여러 갈래를 탐험한 뒤
        // 도달하는 최종 방으로 유지한다.
        int bossIndex = nodes.Count - 1;
        List<RoomNode> expandableNodes = new List<RoomNode> { startNode };

        for (int i = 1; i < bossIndex; i++)
        {
            RoomNode child = nodes[i];
            RoomNode parent = ChooseRandomBranchParent(expandableNodes);
            ConnectTreeNodes(parent, child);

            if (GetFreeDirectionCount(parent) == 0)
            {
                expandableNodes.Remove(parent);
            }

            expandableNodes.Add(child);
        }

        RoomNode bossParent = ChooseDeepestBranchParent(expandableNodes);
        ConnectTreeNodes(bossParent, nodes[bossIndex]);
    }

    private RoomNode ChooseRandomBranchParent(
        List<RoomNode> expandableNodes)
    {
        // 최근에 생성된 방을 조금 더 자주 뽑아 가지 안에 또 가지가
        // 생성되도록 하되, 이전 방들도 후보로 남겨 매번 다른 나무가 된다.
        int recentCount = Mathf.Min(4, expandableNodes.Count);
        int startIndex = expandableNodes.Count - recentCount;
        return expandableNodes[
            Random.Range(startIndex, expandableNodes.Count)
        ];
    }

    private RoomNode ChooseDeepestBranchParent(
        List<RoomNode> expandableNodes)
    {
        int deepestFloor = -1;
        List<RoomNode> candidates = new List<RoomNode>();

        foreach (RoomNode node in expandableNodes)
        {
            if (node.floor > deepestFloor)
            {
                deepestFloor = node.floor;
                candidates.Clear();
                candidates.Add(node);
            }
            else if (node.floor == deepestFloor)
            {
                candidates.Add(node);
            }
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    private void ConnectTreeNodes(RoomNode parent, RoomNode child)
    {
        List<int> freeDirections = new List<int>();

        if (parent.forwardRoom == null) freeDirections.Add(0);
        if (parent.leftRoom == null) freeDirections.Add(1);
        if (parent.rightRoom == null) freeDirections.Add(2);
        // 시작 방의 뒤쪽은 비워 둔다. 시작부터 뒤로 가는 길이 생기면
        // 플레이어가 출발 방향을 혼동하기 쉽기 때문이다.
        if (parent.backwardRoom == null && parent.previousRoom != null)
            freeDirections.Add(3);

        int direction = freeDirections[
            Random.Range(0, freeDirections.Count)
        ];

        switch (direction)
        {
            case 0:
                parent.forwardRoom = child;
                child.backwardRoom = parent;
                break;
            case 1:
                parent.leftRoom = child;
                child.rightRoom = parent;
                break;
            case 2:
                parent.rightRoom = child;
                child.leftRoom = parent;
                break;
            default:
                parent.backwardRoom = child;
                child.forwardRoom = parent;
                break;
        }

        child.previousRoom = parent;
        child.floor = parent.floor + 1;
        parent.nextRooms.Add(child);
    }

    private int GetFreeDirectionCount(RoomNode node)
    {
        int count = 0;
        if (node.forwardRoom == null) count++;
        if (node.backwardRoom == null && node.previousRoom != null)
            count++;
        if (node.leftRoom == null) count++;
        if (node.rightRoom == null) count++;
        return count;
    }

    // ============================================================
    // 방 이동
    // ============================================================

    public void MoveForward()
    {
        if (currentNode == null)
            return;

        if (currentNode.forwardRoom != null)
        {
            Debug.Log(
                $"[RoomManager] 앞으로 이동: " +
                $"Node {currentNode.id} → " +
                $"Node {currentNode.forwardRoom.id}"
            );

            EnterNode(currentNode.forwardRoom);
        }
        MoveSound();
    }

    public void MoveBackward()
    {
        if (currentNode == null ||
            currentNode.backwardRoom == null)
        {
            Debug.Log(
                "[RoomManager] 후방으로 갈 수 있는 방이 없습니다."
            );

            return;
        }

        RoomNode target = currentNode.backwardRoom;

        Debug.Log(
            $"[RoomManager] 뒤로 이동: " +
            $"현재 Node {currentNode.id} → " +
            $"Node {target.id}"
        );

        EnterNode(target);
        MoveSound();
    }

    public void MoveLeft()
    {
        if (currentNode == null)
            return;

        if (currentNode.leftRoom != null)
        {
            Debug.Log(
                $"[RoomManager] 왼쪽 이동: " +
                $"Node {currentNode.id} → " +
                $"Node {currentNode.leftRoom.id}"
            );

            EnterNode(currentNode.leftRoom);
        }
        MoveSound();
    }

    public void MoveRight()
    {
        if (currentNode == null)
            return;

        if (currentNode.rightRoom != null)
        {
            Debug.Log(
                $"[RoomManager] 오른쪽 이동: " +
                $"Node {currentNode.id} → " +
                $"Node {currentNode.rightRoom.id}"
            );

            EnterNode(currentNode.rightRoom);
        }
        MoveSound();
    }

    public void MoveToNext(int index)
    {
        if (currentNode == null)
            return;

        if (index < 0 ||
            index >= currentNode.nextRooms.Count)
            return;

        EnterNode(currentNode.nextRooms[index]);
    }

    public void MoveSound()
    {
        if (currentNode == null)
            return;
            AudioManager.instance?.PlaySfx(AudioManager.instance.moveSound);
    }
    // ============================================================
    // 방 진입
    // ============================================================

    public void EnterNode(RoomNode node)
    {
        if (node == null)
            return;

        // 현재 노드 변경
        currentNode = node;

        // 이전 방은 제거하지 않고 비활성화한다.
        // 같은 RoomNode를 다시 방문하면 이 인스턴스를 재사용한다.
        if (currentRoom != null)
        {
            currentRoom.gameObject.SetActive(false);
        }

        if (spawnedRooms.TryGetValue(node, out Room savedRoom) &&
            savedRoom != null)
        {
            currentRoom = savedRoom;
            currentRoom.gameObject.SetActive(true);
            currentRoom.EnterRoom();
        }
        else
        {
            Debug.LogWarning(
                $"[RoomManager] Node {node.id} 방 인스턴스를 찾지 못했습니다."
            );
            currentRoom = null;
        }

        if (RoomNavigationUI.instance != null)
        {
            if (BattleManager.instance != null &&
                BattleManager.instance.isBattle)
            {
                // Room.EnterRoom()에서 전투가 시작된 경우,
                // 아래 Refresh가 이동 버튼을 다시 표시하지 않게 한다.
                RoomNavigationUI.instance.HideAll();
            }
            else
            {
                RoomNavigationUI.instance.Refresh(node);
            }
        }
    }

    // ============================================================
    // ID로 방 이동
    // ============================================================

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

    // ============================================================
    // 현재 방 클리어
    // ============================================================

    public void ClearCurrentRoom()
    {
        if (currentNode != null)
        {
            currentNode.cleared = true;
        }

        // 전투 종료 후 화살표 갱신
        if (RoomNavigationUI.instance != null &&
            currentNode != null)
        {
            RoomNavigationUI.instance.Refresh(
                currentNode
            );
        }
    }

    private void ClearSpawnedRooms()
    {
        foreach (Room room in spawnedRooms.Values)
        {
            if (room != null)
            {
                Destroy(room.gameObject);
            }
        }

        spawnedRooms.Clear();
        currentRoom = null;
        currentNode = null;
    }

    private void SpawnAllRooms()
    {
        Transform parentToUse =
            roomParent != null
                ? roomParent
                : transform;

        foreach (RoomNode node in allNodes)
        {
            Room prefab = GetRandomRoomPrefab(
                node.zoneType,
                node.roomType
            );

            if (prefab == null)
            {
                Debug.LogWarning(
                    $"[RoomManager] " +
                    $"{node.zoneType} / " +
                    $"{node.roomType} " +
                    $"에 해당하는 방 프리팹이 없습니다."
                );
                continue;
            }

            Room room = Instantiate(prefab, parentToUse);
            room.Setup(node);
            room.gameObject.SetActive(false);
            spawnedRooms[node] = room;
        }
    }

    // ============================================================
    // 방 프리팹
    // ============================================================

    Room GetRandomRoomPrefab(
        ZoneType zone,
        RoomType type)
    {
        foreach (RoomPoolData pool in roomPools)
        {
            if (pool != null &&
                pool.zoneType == zone &&
                pool.roomType == type)
            {
                if (pool.roomPrefabs != null &&
                    pool.roomPrefabs.Length > 0)
                {
                    return pool.roomPrefabs[
                        Random.Range(
                            0,
                            pool.roomPrefabs.Length
                        )
                    ];
                }
            }
        }

        // None 방은 Start 프리팹 사용
        if (type == RoomType.None)
        {
            return GetRandomRoomPrefab(
                zone,
                RoomType.Start
            );
        }

        return null;
    }

    // ============================================================
    // 적 스폰
    // ============================================================

    public Enemy[] SpawnEnemiesForRoom(
        Room room)
    {
        EnemyPoolData pool =
            GetEnemyPool(
                room.zoneType
            );

        if (pool == null ||
            pool.enemyPrefabs == null ||
            pool.enemyPrefabs.Length <= 0)
        {
            return new Enemy[0];
        }

        // BattleManager의 Enemy 위치 배열 우선
        Transform[] spawnPoints =
            (
                BattleManager.instance != null &&
                BattleManager.instance.Enemy != null &&
                BattleManager.instance.Enemy.Length > 0
            )
            ? BattleManager.instance.Enemy
            : room.enemySpawnPoints;

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            return new Enemy[0];
        }

        int maxEnemyCount = Mathf.Min(4, spawnPoints.Length);
        if (maxEnemyCount < 2)
        {
            Debug.LogWarning(
                "[RoomManager] 전투 적을 2마리 이상 배치할 스폰 위치가 부족합니다."
            );
            return new Enemy[0];
        }

        int count = Random.Range(2, maxEnemyCount + 1);

        List<Enemy> spawned =
            new List<Enemy>();

        for (int i = 0; i < count; i++)
        {
            Enemy prefab =
                pool.enemyPrefabs[
                    Random.Range(
                        0,
                        pool.enemyPrefabs.Length
                    )
                ];

            if (prefab == null)
                continue;

            Transform spawnPoint =
                spawnPoints[i];

            Vector3 pos =
                spawnPoint != null
                    ? spawnPoint.position
                    : Vector3.zero;

            Quaternion rot =
                spawnPoint != null
                    ? spawnPoint.rotation
                    : Quaternion.identity;

            Enemy enemy =
                Instantiate(
                    prefab,
                    pos,
                    rot,
                    room.transform
                );

            spawned.Add(enemy);
        }

        return spawned.ToArray();
    }

    private EnemyPoolData GetEnemyPool(
        ZoneType zone)
    {
        foreach (EnemyPoolData pool in enemyPools)
        {
            if (pool != null &&
                pool.zoneType == zone)
            {
                return pool;
            }
        }

        return null;
    }
}
