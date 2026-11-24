using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 미니맵에서 각 방 타입에 표시할 아이콘 스프라이트 세트를 보관하는 ScriptableObject.
/// Create → MinimapIconSet 에서 에셋을 생성한 뒤 MinimapUI 인스펙터에 연결합니다.
/// </summary>
[CreateAssetMenu(fileName = "MinimapIconSet", menuName = "Minimap/Icon Set")]
public class MinimapIconSet : ScriptableObject
{
    [Serializable]
    public class RoomIconEntry
    {
        [Tooltip("방 타입")]
        public RoomType roomType;

        [Tooltip("방 타입에 해당하는 아이콘 스프라이트")]
        public Sprite icon;

        [Tooltip("아이콘 배경 색상 (기본: 흰색)")]
        public Color backgroundColor = Color.white;

        [Tooltip("아이콘 전경 색상 (기본: 흰색)")]
        public Color iconColor = Color.white;
    }

    [Header("방 타입별 아이콘 설정")]
    [Tooltip("각 방 타입에 맞는 아이콘을 여기에 추가합니다.")]
    public List<RoomIconEntry> entries = new List<RoomIconEntry>();

    [Header("공통 설정")]
    [Tooltip("방 아이콘 미등록 시 사용할 기본 색상")]
    public Color defaultBackgroundColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Tooltip("방 아이콘 미등록 시 사용할 기본 색상")]
    public Color defaultIconColor = Color.white;

    [Tooltip("현재 위치한 방에 적용할 하이라이트 색상")]
    public Color currentRoomHighlight = new Color(1f, 0.9f, 0.2f, 1f);

    [Tooltip("클리어된 방에 적용할 색상 오버레이 (알파값으로 투명도 조절)")]
    public Color clearedRoomOverlay = new Color(1f, 1f, 1f, 0.45f);

    /// <summary>특정 RoomType에 대한 아이콘 설정을 반환합니다. 없으면 null.</summary>
    public RoomIconEntry GetEntry(RoomType roomType)
    {
        foreach (RoomIconEntry entry in entries)
        {
            if (entry.roomType == roomType)
                return entry;
        }
        return null;
    }
}
