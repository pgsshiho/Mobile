using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public RoomType roomType;
    public ZoneType zoneType;

    public string roomName;

    public Transform[] enemySpawnPoints;

    public Enemy[] enemies;
    RoomNode node;

    [Header("Room Rewards (ScriptableObject)")]
    [Tooltip("재사용 가능한 방 보상 SO 데이터 (등록 시 아래 직접 설정보다 우선 적용)")]
    public RoomRewardData rewardData;

    [Header("Direct Room Rewards (직접 설정)")]
    [Tooltip("rewardData가 비어있을 때 적용되는 골드 최소/최대 범위")]
    public int minMoney = 10;
    public int maxMoney = 30;

    [Tooltip("rewardData가 비어있을 때 적용되는 재료 최소/최대 범위")]
    public int minMaterial = 0;
    public int maxMaterial = 2;

    [Tooltip("이 방에서 드랍할 아이템 및 확률(0~100%) 목록")]
    public List<ItemDropEntry> dropItems = new List<ItemDropEntry>();

    public void Setup(RoomNode roomNode)
    {
        node = roomNode;

        roomType = node.roomType;
        zoneType = node.zoneType;

        AutoFindSpawnPoints();

        SpawnByRoomType();

        EnterRoom();
    }

    void AutoFindSpawnPoints()
    {
        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            List<Transform> list = new List<Transform>();
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("SpawnPoint"))
                {
                    list.Add(child);
                }
            }
            if (list.Count > 0)
            {
                enemySpawnPoints = list.ToArray();
            }
        }
    }

    void SpawnByRoomType()
    {
        if (roomType == RoomType.Enemy ||
            roomType == RoomType.Boss ||
            roomType == RoomType.GrassRoom ||
            roomType == RoomType.FloodedRoom ||
            roomType == RoomType.CloudRoom ||
            roomType == RoomType.PollutedRoom)
        {
            enemies =
                RoomManager.instance
                .SpawnEnemiesForRoom(
                    this
                );
        }
    }

    void EnterRoom()
    {
        switch (roomType)
        {
            // ── 전투방 ──────────────────────────────────────────────
            case RoomType.Enemy:
            case RoomType.Boss:
            case RoomType.GrassRoom:
            case RoomType.FloodedRoom:
            case RoomType.CloudRoom:
            case RoomType.PollutedRoom:
                if (!node.cleared)
                {
                    BattleManager.instance.StartBattle(this);
                }
                break;

            // ── 상점 ────────────────────────────────────────────────
            case RoomType.ItemShop:
                Debug.Log("철물점 (아이템 상점)");
                // TODO: ItemShopUI.instance.Open();
                break;

            case RoomType.Blacksmith:
                Debug.Log("대장간 (장비 강화)");
                // TODO: BlacksmithUI.instance.Open();
                break;

            // ── 회복 ────────────────────────────────────────────────
            case RoomType.RepairShop:
                Debug.Log("수리점 입장 - 전원 체력 일부 회복");
                HealAllParty(0.3f); // 최대 체력의 30% 회복
                break;

            // ── 보상방 ──────────────────────────────────────────────
            case RoomType.Fountain:
                Debug.Log("분수 - 확정 버프 획득");
                // TODO: BuffSelectUI.instance.Open();
                break;

            case RoomType.SageStone:
                Debug.Log("현자의 석판 - 경험치 획득");
                // TODO: ExpGrantManager.instance.GrantExp();
                break;

            case RoomType.TrainingRoom:
                Debug.Log("훈련 교관 - 능력치 업 구매");
                // TODO: TrainingUI.instance.Open();
                break;

            case RoomType.Altar:
                Debug.Log("제단 - 버프 또는 디버프 발동");
                // TODO: AltarManager.instance.Trigger();
                break;

            case RoomType.GamblingRoom:
                Debug.Log("도박방 - 돈을 도박 가능");
                // TODO: GamblingUI.instance.Open();
                break;

            case RoomType.Archive:
                Debug.Log("기록 보관소 - 스토리 로그");
                // TODO: ArchiveUI.instance.Open();
                break;

            // ── 시작방 ──────────────────────────────────────────────
            case RoomType.Start:
                Debug.Log($"[Room] {zoneType} 구역 시작 지점(Start)에 진입했습니다.");
                break;

            case RoomType.None:
                Debug.Log($"[Room] 아무것도 없는 빈 방({zoneType})에 진입했습니다.");
                break;

            default:
                Debug.Log($"미구현 방 타입: {roomType}");
                break;
        }
    }

    /// <summary>
    /// 이 방에 설정된 확률에 따라 보상(골드, 재료, 드랍 아이템)을 계산합니다.
    /// </summary>
    public (int money, int material, List<ItemData> items) CalculateRewards()
    {
        // 1. ScriptableObject가 연결되어 있으면 SO에서 롤링
        if (rewardData != null)
        {
            return rewardData.RollRewards();
        }

        // 2. 직접 인스펙터에 설정된 값으로 롤링
        int rolledMoney = (maxMoney >= minMoney) ? Random.Range(minMoney, maxMoney + 1) : minMoney;
        int rolledMaterial = (maxMaterial >= minMaterial) ? Random.Range(minMaterial, maxMaterial + 1) : minMaterial;

        List<ItemData> rolledItems = new List<ItemData>();
        if (dropItems != null)
        {
            foreach (var entry in dropItems)
            {
                if (entry == null || entry.item == null) continue;

                float roll = Random.Range(0f, 100f);
                if (roll <= entry.dropRate)
                {
                    int dropCount = Mathf.Max(1, entry.count);
                    for (int i = 0; i < dropCount; i++)
                    {
                        rolledItems.Add(entry.item);
                    }
                }
            }
        }

        return (rolledMoney, rolledMaterial, rolledItems);
    }

    /// <summary>
    /// 방 보상을 계산하여 Reward UI에 세팅하고 창을 엽니다.
    /// </summary>
    public void GenerateAndOpenReward()
    {
        var (money, material, items) = CalculateRewards();

        if (Reward.Instance != null)
        {
            Reward.Instance.SetRewards(money, material, items);
            Reward.Instance.RewardOpen();
        }
        else
        {
            Debug.LogWarning("[Room] Reward.Instance가 씬에 존재하지 않습니다.");
        }
    }

    /// <summary>파티 전원의 체력을 최대 체력의 ratio만큼 회복</summary>
    private void HealAllParty(float ratio)
    {
        if (PartyManager.instance == null) return;

        foreach (Unit unit in PartyManager.instance.partySlots)
        {
            if (unit != null && unit.health > 0)
            {
                int healAmount = Mathf.RoundToInt(unit.maxHealth * ratio);
                unit.Heal(healAmount);
            }
        }
    }
}