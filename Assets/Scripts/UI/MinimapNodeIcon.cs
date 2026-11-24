using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 미니맵 위의 개별 방 아이콘 오브젝트.
/// MinimapUI가 각 RoomNode마다 하나씩 생성하여 관리합니다.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MinimapNodeIcon : MonoBehaviour
{
    [Header("내부 참조 (Prefab에서 연결)")]
    [SerializeField] private Image backgroundImage;   // 방 배경 이미지
    [SerializeField] private Image iconImage;         // 방 타입 아이콘 이미지
    [SerializeField] private Image currentMarker;     // 현재 위치 하이라이트 (별, 화살표 등)
    [SerializeField] private Image clearedOverlay;    // 클리어된 방 반투명 덮개

    /// <summary>코드에서 동적으로 생성한 Image 컴포넌트를 주입합니다 (폴백 아이콘 전용).</summary>
    public void InjectComponents(Image bg, Image icon, Image marker, Image overlay)
    {
        backgroundImage = bg;
        iconImage       = icon;
        currentMarker   = marker;
        clearedOverlay  = overlay;
    }

    // 이 아이콘이 나타내는 노드
    public RoomNode Node { get; private set; }

    public RectTransform RectTrans { get; private set; }

    private void Awake()
    {
        RectTrans = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 노드 정보와 아이콘 세트를 받아 초기 모양을 세팅합니다.
    /// </summary>
    public void Setup(RoomNode node, MinimapIconSet iconSet, float cellSize)
    {
        Node = node;
        RectTrans.sizeDelta = new Vector2(cellSize, cellSize);

        if (iconSet != null)
        {
            MinimapIconSet.RoomIconEntry entry = iconSet.GetEntry(node.roomType);

            if (backgroundImage != null)
                backgroundImage.color = entry != null ? entry.backgroundColor : iconSet.defaultBackgroundColor;

            if (iconImage != null)
            {
                iconImage.sprite = entry?.icon;
                iconImage.color = entry != null ? entry.iconColor : iconSet.defaultIconColor;
                iconImage.enabled = iconImage.sprite != null;
            }
        }

        // 처음에는 현재 마커, 클리어 오버레이 비활성화
        if (currentMarker != null) currentMarker.enabled = false;
        if (clearedOverlay != null) clearedOverlay.enabled = false;
    }

    /// <summary>이 방이 현재 위치한 방인지에 따라 하이라이트를 갱신합니다.</summary>
    public void SetCurrent(bool isCurrent, Color highlightColor)
    {
        if (currentMarker != null)
        {
            currentMarker.enabled = isCurrent;
            currentMarker.color = highlightColor;
        }

        // 현재 방은 클리어 오버레이 숨김
        if (clearedOverlay != null && isCurrent)
            clearedOverlay.enabled = false;
    }

    /// <summary>방 클리어 여부에 따라 오버레이를 갱신합니다.</summary>
    public void SetCleared(bool cleared, Color overlayColor, bool isCurrent)
    {
        if (clearedOverlay != null)
        {
            clearedOverlay.enabled = cleared && !isCurrent;
            clearedOverlay.color = overlayColor;
        }
    }
}
