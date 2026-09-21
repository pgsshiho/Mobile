using System.Collections.Generic;
using UnityEngine;

public enum ZoneType
{
    // 기존 저장/에셋의 숫자 값을 유지해 직렬화된 Zone이 바뀌지 않게 한다.
    Forest = 0,
    Coast = 4,
    Cave = 6,
    Basement = 7,
    Lab = 8,
    City = 9
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

    EliteEnemy,

    // 로봇 영입/제작 등 로봇 관련 기능을 담당하는 필수 특수 방
    RobotFactory
}

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    [Header("Zone & Room Settings")]
    [Tooltip("한 Zone당 생성할 총 방의 개수 (항상 최소 13개)")]
    [Min(13)]
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
                $"({node.gridPos.x,2},{node.gridPos.y,2}) " +
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
        // 시작, 보스, 전투 3개, 로봇 공장, 보상 2개, 상점 2개, 빈 방을
        // 필수 방을 포함해 항상 13개 이상 생성한다.
        totalCount = Mathf.Max(13, totalCount);

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
        // AddRobot은 일반 방으로 남기되 필수 생성에서는 제외한다.
        // RobotFactory는 매 맵에 정확히 하나 이상 포함한다.
        middleRooms.Add(RoomType.RobotFactory);
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

            case ZoneType.Coast:
                list.Add(RoomType.FloodedRoom);
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

    private static readonly Vector2Int[] GridDirections = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // Forward (전방/위)
        new Vector2Int(0, -1),  // Backward (후방/아래)
        new Vector2Int(-1, 0),  // Left (좌측)
        new Vector2Int(1, 0)    // Right (우측)
    };

    private void BuildBranchingConnections(List<RoomNode> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            return;

        foreach (RoomNode node in nodes)
        {
            node.nextRooms.Clear();
            node.previousRoom = null;
            node.forwardRoom = null;
            node.backwardRoom = null;
            node.leftRoom = null;
            node.rightRoom = null;
        }

        // 2D 그리드 맵: 각 좌표당 유일한 방 노드만 존재
        Dictionary<Vector2Int, RoomNode> grid = new Dictionary<Vector2Int, RoomNode>();

        RoomNode startNode = nodes[0];
        startNode.gridPos = Vector2Int.zero;
        startNode.floor = 0;
        grid[Vector2Int.zero] = startNode;

        List<RoomNode> placedNodes = new List<RoomNode> { startNode };
        int bossIndex = nodes.Count - 1;

        // 1. 중간 방들을 2D 그리드에 배치 (서로 다른 좌표에 1개씩만 배치)
        for (int i = 1; i < bossIndex; i++)
        {
            RoomNode child = nodes[i];
            Vector2Int chosenPos = ChooseGridPositionForChild(placedNodes, grid);

            child.gridPos = chosenPos;
            grid[chosenPos] = child;
            placedNodes.Add(child);
        }

        // 2. 보스 방을 가장 깊고 단일 진입로를 가진 외곽에 배치
        RoomNode bossNode = nodes[bossIndex];
        Vector2Int bossPos = ChooseGridPositionForBoss(placedNodes, grid);
        bossNode.gridPos = bossPos;
        grid[bossPos] = bossNode;
        placedNodes.Add(bossNode);

        // 3. 2D 그리드 좌표를 기준으로 인접한 방들끼리 4방향 양방향 연결 설정
        // (오른쪽 위방과 위 오른쪽방이 동일한 좌표 (x, y)의 동일 방을 공유)
        ConnectAllAdjacentRooms(nodes, grid);

        // 4. 시작 방 기준 BFS 거리(floor) 및 nextRooms / previousRoom 설정
        CalculateFloorsAndPathReferences(startNode, nodes);
    }

    private Vector2Int ChooseGridPositionForChild(
        List<RoomNode> placedNodes,
        Dictionary<Vector2Int, RoomNode> grid)
    {
        // 최근에 배치된 방들을 우선하여 나뭇가지 형태로 뻗어나가도록 유도
        int recentCount = Mathf.Min(4, placedNodes.Count);
        int startIndex = placedNodes.Count - recentCount;

        for (int attempt = 0; attempt < 30; attempt++)
        {
            RoomNode parent = placedNodes[Random.Range(startIndex, placedNodes.Count)];
            List<Vector2Int> freeNeighbors = GetFreeNeighbors(parent.gridPos, grid);

            if (freeNeighbors.Count > 0)
            {
                // 인접 방 개수가 2개 이하인 곳을 우선하여 과도한 덩어리 형성 방지
                List<Vector2Int> nonCrowded = new List<Vector2Int>();
                foreach (var pos in freeNeighbors)
                {
                    if (CountOccupiedNeighbors(pos, grid) <= 2)
                    {
                        nonCrowded.Add(pos);
                    }
                }

                List<Vector2Int> candidates = nonCrowded.Count > 0 ? nonCrowded : freeNeighbors;
                return candidates[Random.Range(0, candidates.Count)];
            }
        }

        // Fallback: 모든 배치된 노드 주변 탐색
        List<Vector2Int> allFree = new List<Vector2Int>();
        List<Vector2Int> allNonCrowded = new List<Vector2Int>();

        for (int i = placedNodes.Count - 1; i >= 0; i--)
        {
            List<Vector2Int> free = GetFreeNeighbors(placedNodes[i].gridPos, grid);
            foreach (var pos in free)
            {
                if (!allFree.Contains(pos))
                {
                    allFree.Add(pos);
                    if (CountOccupiedNeighbors(pos, grid) <= 2)
                    {
                        allNonCrowded.Add(pos);
                    }
                }
            }
        }

        List<Vector2Int> pool = allNonCrowded.Count > 0 ? allNonCrowded : allFree;
        return pool[Random.Range(0, pool.Count)];
    }

    private Vector2Int ChooseGridPositionForBoss(
        List<RoomNode> placedNodes,
        Dictionary<Vector2Int, RoomNode> grid)
    {
        List<Vector2Int> allCandidates = new List<Vector2Int>();

        foreach (RoomNode node in placedNodes)
        {
            List<Vector2Int> free = GetFreeNeighbors(node.gridPos, grid);
            foreach (var pos in free)
            {
                if (!allCandidates.Contains(pos))
                {
                    allCandidates.Add(pos);
                }
            }
        }

        // 보스는 문이 1개만 있는 막다른 골목(Dead-end)을 1순위로 선호
        List<Vector2Int> singleEntranceCandidates = new List<Vector2Int>();
        foreach (var pos in allCandidates)
        {
            if (CountOccupiedNeighbors(pos, grid) == 1)
            {
                singleEntranceCandidates.Add(pos);
            }
        }

        List<Vector2Int> searchPool = singleEntranceCandidates.Count > 0
            ? singleEntranceCandidates
            : allCandidates;

        // 시작점으로부터 가장 먼 거리(X축 거리 + Y축 가중치)를 가진 위치 선택
        int maxScore = -1;
        List<Vector2Int> bestCandidates = new List<Vector2Int>();

        foreach (var pos in searchPool)
        {
            int score = Mathf.Abs(pos.x) + pos.y * 2;
            if (score > maxScore)
            {
                maxScore = score;
                bestCandidates.Clear();
                bestCandidates.Add(pos);
            }
            else if (score == maxScore)
            {
                bestCandidates.Add(pos);
            }
        }

        return bestCandidates[Random.Range(0, bestCandidates.Count)];
    }

    private List<Vector2Int> GetFreeNeighbors(
        Vector2Int pos,
        Dictionary<Vector2Int, RoomNode> grid)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (Vector2Int dir in GridDirections)
        {
            Vector2Int next = pos + dir;

            if (grid.ContainsKey(next))
                continue;

            // 시작 방 뒤쪽(y < 0)은 방을 배치하지 않아 시작 시 후방으로 이동하는 혼란 방지
            if (next.y < 0)
                continue;

            result.Add(next);
        }

        return result;
    }

    private int CountOccupiedNeighbors(
        Vector2Int pos,
        Dictionary<Vector2Int, RoomNode> grid)
    {
        int count = 0;
        foreach (Vector2Int dir in GridDirections)
        {
            if (grid.ContainsKey(pos + dir))
            {
                count++;
            }
        }
        return count;
    }

    private void ConnectAllAdjacentRooms(
        List<RoomNode> nodes,
        Dictionary<Vector2Int, RoomNode> grid)
    {
        foreach (RoomNode node in nodes)
        {
            Vector2Int pos = node.gridPos;

            // 전방: (x, y + 1)
            if (grid.TryGetValue(pos + new Vector2Int(0, 1), out RoomNode upNode))
            {
                node.forwardRoom = upNode;
            }

            // 후방: (x, y - 1)
            if (grid.TryGetValue(pos + new Vector2Int(0, -1), out RoomNode downNode))
            {
                node.backwardRoom = downNode;
            }

            // 좌측: (x - 1, y)
            if (grid.TryGetValue(pos + new Vector2Int(-1, 0), out RoomNode leftNode))
            {
                node.leftRoom = leftNode;
            }

            // 우측: (x + 1, y)
            if (grid.TryGetValue(pos + new Vector2Int(1, 0), out RoomNode rightNode))
            {
                node.rightRoom = rightNode;
            }
        }
    }

    private void CalculateFloorsAndPathReferences(
        RoomNode startNode,
        List<RoomNode> nodes)
    {
        // 시작 방 기준 BFS 거리(floor) 계산
        Queue<RoomNode> queue = new Queue<RoomNode>();
        HashSet<RoomNode> visited = new HashSet<RoomNode>();

        startNode.floor = 0;
        visited.Add(startNode);
        queue.Enqueue(startNode);

        while (queue.Count > 0)
        {
            RoomNode current = queue.Dequeue();

            RoomNode[] neighbors = {
                current.forwardRoom,
                current.backwardRoom,
                current.leftRoom,
                current.rightRoom
            };

            foreach (RoomNode neighbor in neighbors)
            {
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    neighbor.floor = current.floor + 1;
                    neighbor.previousRoom = current;
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // nextRooms 리스트에 인접한 방 연결 등록
        foreach (RoomNode node in nodes)
        {
            node.nextRooms.Clear();
            RoomNode[] neighbors = {
                node.forwardRoom,
                node.rightRoom,
                node.leftRoom,
                node.backwardRoom
            };

            foreach (RoomNode neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    node.nextRooms.Add(neighbor);
                }
            }
        }
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
