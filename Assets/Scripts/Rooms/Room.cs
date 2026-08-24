using UnityEngine;

public class Room : MonoBehaviour
{
    public RoomType roomType;
    public ZoneType zoneType;

    public string roomName;

    public Transform[] enemySpawnPoints;

    public Enemy[] enemies;

    RoomNode node;

    public void Setup(RoomNode roomNode)
    {
        node = roomNode;

        roomType = node.roomType;
        zoneType = node.zoneType;

        SpawnByRoomType();

        EnterRoom();
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

            default:
                Debug.Log($"미구현 방 타입: {roomType}");
                break;
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