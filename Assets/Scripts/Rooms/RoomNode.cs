using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomNode
{
    public int id;
    public int floor;

    public ZoneType zoneType;
    public RoomType roomType;

    public bool cleared;

    // 분기 및 4방향 연결 (순환 참조 방지를 위해 NonSerialized 처리)
    [NonSerialized] public RoomNode previousRoom;
    [NonSerialized] public RoomNode backwardRoom;
    [NonSerialized] public RoomNode forwardRoom;
    [NonSerialized] public RoomNode leftRoom;
    [NonSerialized] public RoomNode rightRoom;

    [NonSerialized] public List<RoomNode> nextRooms = new List<RoomNode>();

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

    public override string ToString()
    {
        return $"[Node {id}] {roomType} (Floor {floor}, Cleared: {cleared})";
    }
}