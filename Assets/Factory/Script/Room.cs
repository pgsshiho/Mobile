using Unity.VisualScripting;
using UnityEngine;


public class Room : MonoBehaviour
{
    RoomManager.roomType roomType;

    public Room nextRoom;

    public Room prevRoom;

    public string roomName;

    public Enemy[] enemies;

    public void OnEnable()
    {
        switch (roomType)
        {
            case RoomManager.roomType.Enemy:
                foreach (var enemy in enemies)
                {
                    
                    TurnManager.instance.RegisterRoom(RoomManager.instance.currentRoom);
                }
                break;
            case RoomManager.roomType.Boss:
                foreach (var enemy in enemies)
                {
                    TurnManager.instance.RegisterRoom(RoomManager.instance.currentRoom);
                }
                break;
            case RoomManager.roomType.Event:
                break;
            case RoomManager.roomType.Shop:
                break;
        }
    }
}