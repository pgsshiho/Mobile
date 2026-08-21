using System.Collections.Generic;

public class RoomNode
{
    public int id;
    public int floor;

    public ZoneType zoneType;
    public RoomType roomType;

    public bool cleared;

    public List<RoomNode> nextRooms =
        new List<RoomNode>();

    public RoomNode(
        int id,
        int floor,
        ZoneType zoneType,
        RoomType roomType
    )
    {
        this.id = id;
        this.floor = floor;
        this.zoneType = zoneType;
        this.roomType = roomType;
        cleared = false;
    }
}