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

    // 분기 연결 (동적 분기 그래프)
    [NonSerialized] public List<RoomNode> nextRooms = new List<RoomNode>();
    [NonSerialized] public RoomNode previousRoom;

    // 4방향 배치 연결
    [NonSerialized] public RoomNode forwardRoom;
    [NonSerialized] public RoomNode backwardRoom;
    [NonSerialized] public RoomNode leftRoom;
    [NonSerialized] public RoomNode rightRoom;

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
        nextRooms = new List<RoomNode>();
    }

    public override string ToString()
    {
        return $"[Node {id} | F{floor} | {zoneType} | {roomType}]";
    }
}