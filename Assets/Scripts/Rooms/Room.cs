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
            roomType == RoomType.Boss)
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
            case RoomType.Enemy:
            case RoomType.Boss:
                if (!node.cleared)
                {
                    BattleManager.instance
                        .StartBattle(this);
                }
                break;

            case RoomType.Shop:
                Debug.Log("상점 방");
                break;

            case RoomType.Event:
                Debug.Log("이벤트 방");
                break;

            case RoomType.Reward:
                Debug.Log("보상 방");
                break;
        }
    }
}