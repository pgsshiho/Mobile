using UnityEngine;

public class Door : MonoBehaviour
{
    public Room targetRoom;

    public void Next()
    {
      RoomManager.instance.ChangeRoom(targetRoom);
    }
}