using UnityEngine;


public class  RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    public Room currentRoom;

    private void Awake()
    {
        instance = this;
    }
    public enum roomType
    {
        Enemy,
        Boss,
        Event,
        Shop
    }
    public void ChangeRoom(Room nextRoom)
    {
        currentRoom.gameObject.SetActive(false);

        nextRoom.gameObject.SetActive(true);

        currentRoom = nextRoom;
    }
}