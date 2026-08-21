using UnityEngine;

[CreateAssetMenu(menuName = "Map/Room Pool")]
public class RoomPoolData : ScriptableObject
{
    public ZoneType zoneType;
    public RoomType roomType;

    public Room[] roomPrefabs;
}