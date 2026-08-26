using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Info")]
    public RoomType roomType;
    public ZoneType zoneType;
    public string roomName;

    [Header("Spawn Points & Enemies")]
    public Transform[] enemySpawnPoints;
    public Enemy[] enemies;

    [Header("Reward Settings (SO or Direct)")]
    [Tooltip("방 보상 데이터 ScriptableObject (연결 시 우선 사용)")]
    public RoomRewardData rewardData;

    [Tooltip("직접 설정용 기본 골드 범위")]
    public int minMoney = 50;
    public int maxMoney = 150;

    [Tooltip("직접 설정용 기본 재료 범위")]
    public int minMaterial = 10;
    public int maxMaterial = 30;

    [Tooltip("직접 설정용 드랍 아이템 목록")]
    public List<ItemDropEntry> dropItems = new List<ItemDropEntry>();

    [HideInInspector]
    public RoomNode node;

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
                break;

            case RoomType.Blacksmith:
                Debug.Log("대장간 (장비 강화)");
                break;

            // ── 회복 ────────────────────────────────────────────────
            case RoomType.RepairShop:
                Debug.Log("수리점 입장 - 전원 체력 일부 회복");
                HealAllParty(0.3f);
                break;

            // ── 보상방 ──────────────────────────────────────────────
            case RoomType.Fountain:
                Debug.Log("분수 - 확정 버프 획득");
                break;

            case RoomType.SageStone:
                Debug.Log("현자의 석판 - 경험치 획득");
                break;

            case RoomType.TrainingRoom:
                Debug.Log("훈련 교관 - 능력치 업 구매");
                break;

            // ── 시작 / 빈 방 ─────────────────────────────────────────
            case RoomType.Start:
                Debug.Log("시작 방");
                break;

            case RoomType.None:
                Debug.Log("빈 방 (아무것도 없는 방)");
                break;

            // ── 기타 방 ──────────────────────────────────────────────
            case RoomType.Event:
                Debug.Log("랜덤 이벤트 방");
                break;

            case RoomType.EliteEnemy:
                Debug.Log("엘리트 적 방");
                if (!node.cleared)
                {
                    BattleManager.instance.StartBattle(this);
                }
                break;

            default:
                Debug.Log($"방 입장: {roomType}");
                break;
        }
    }

    /// <summary>
    /// 이 방에 설정된 확률에 따라 보상(골드, 재료, 드랍 아이템)을 계산합니다.
    /// </summary>
    public (int money, int material, List<ItemData> items) CalculateRewards()
    {
        if (rewardData != null)
        {
            return rewardData.RollRewards();
        }

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
    }

    private void HealAllParty(float percent)
    {
        if (PartyManager.instance == null || PartyManager.instance.partySlots == null) return;

        foreach (Unit unit in PartyManager.instance.partySlots)
        {
            if (unit != null && unit.health > 0)
            {
                int heal = Mathf.RoundToInt(unit.maxHealth * percent);
                unit.Heal(heal);
            }
        }
    }
}