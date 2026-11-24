using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 미니맵 UI 전체를 관리합니다.
///
/// ■ 사용법
///   - MinimapUI 컴포넌트를 가진 GameObject를 씬에 배치합니다.
///   - minimapRoot: 미니맵 패널 루트 (기본으로 비활성화).
///   - iconPrefab  : MinimapNodeIcon 컴포넌트를 가진 방 아이콘 프리팹.
///   - iconSet     : MinimapIconSet ScriptableObject.
///   - 버튼 등에서 OpenMinimap() / CloseMinimap() / ToggleMinimap() 을 호출합니다.
///
/// ■ 자동 갱신
///   - RoomManager.instance 가 존재하는 동안 현재 방 변경을 1초마다 폴링하여 갱신합니다.
///   - 성능이 중요하다면 RoomManager.OnRoomEntered 이벤트를 추가해 이벤트 기반으로 전환하세요.
/// </summary>
public class MinimapUI : MonoBehaviour
{
    public static MinimapUI instance;

    // ──────────────────────────────────────────
    // 인스펙터 연결
    // ──────────────────────────────────────────

    [Header("패널 루트 (기본: 비활성화)")]
    [Tooltip("미니맵 전체 패널 오브젝트. 처음에는 꺼져 있어야 합니다.")]
    [SerializeField] private GameObject minimapRoot;

    [Header("아이콘 프리팹 & 아이콘 세트")]
    [Tooltip("MinimapNodeIcon 컴포넌트를 가진 방 아이콘 프리팹")]
    [SerializeField] private MinimapNodeIcon iconPrefab;

    [Tooltip("방 타입별 아이콘/색상을 정의한 ScriptableObject")]
    [SerializeField] private MinimapIconSet iconSet;

    [Header("레이아웃")]
    [Tooltip("방 아이콘 하나의 크기 (픽셀, 모바일 터치 및 가독성 최적화: 기본 56px)")]
    [SerializeField] private float cellSize = 56f;

    [Tooltip("방 사이 간격 (픽셀, 모바일 기준 10px)")]
    [SerializeField] private float cellSpacing = 10f;

    [Tooltip("아이콘들을 배치할 부모 Transform (Content 영역 등)")]
    [SerializeField] private RectTransform iconContainer;

    [Header("애니메이션")]
    [SerializeField] private float animDuration = 0.22f;
    [SerializeField] private float startScale   = 0.8f;

    [Header("닫기 버튼 (선택)")]
    [Tooltip("지정하면 미니맵 열릴 때 활성화, 닫힐 때 비활성화합니다.")]
    [SerializeField] private Button closeButton;

    // ──────────────────────────────────────────
    // 내부 상태
    // ──────────────────────────────────────────

    private readonly Dictionary<RoomNode, MinimapNodeIcon> nodeIcons =
        new Dictionary<RoomNode, MinimapNodeIcon>();

    private RoomNode lastCurrentNode;
    private float    refreshTimer;
    private const float RefreshInterval = 0.5f; // 0.5초마다 갱신 폴링

    // ──────────────────────────────────────────
    // Unity 생명주기
    // ──────────────────────────────────────────

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // 미니맵은 처음에 반드시 꺼진 상태로 시작
        if (minimapRoot != null)
            minimapRoot.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMinimap);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;

        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseMinimap);
    }

    private void Update()
    {
        // 미니맵이 열려 있을 때만 갱신
        if (minimapRoot == null || !minimapRoot.activeSelf)
            return;

        refreshTimer += Time.unscaledDeltaTime;
        if (refreshTimer < RefreshInterval)
            return;

        refreshTimer = 0f;
        RefreshCurrentRoom();
    }

    // ──────────────────────────────────────────
    // 공개 API
    // ──────────────────────────────────────────

    /// <summary>미니맵을 엽니다. RoomManager에서 맵 데이터를 읽어 아이콘을 (재)빌드합니다.</summary>
    public void OpenMinimap()
    {
        if (minimapRoot == null) return;

        // 열 때마다 최신 맵으로 재빌드
        BuildMap();

        minimapRoot.SetActive(true);

        // 팝업 애니메이션
        Transform panel = minimapRoot.transform;
        panel.DOKill();
        panel.localScale = Vector3.one * startScale;
        panel.DOScale(Vector3.one, animDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);

        RefreshCurrentRoom();
    }

    /// <summary>미니맵을 닫습니다.</summary>
    public void CloseMinimap()
    {
        if (minimapRoot == null) return;
        minimapRoot.transform.DOKill();
        minimapRoot.SetActive(false);
    }

    /// <summary>미니맵이 열려 있으면 닫고, 닫혀 있으면 엽니다.</summary>
    public void ToggleMinimap()
    {
        if (minimapRoot == null) return;

        if (minimapRoot.activeSelf)
            CloseMinimap();
        else
            OpenMinimap();
    }

    /// <summary>현재 방이 바뀌었을 때 외부에서 직접 갱신을 요청할 수 있습니다.</summary>
    public void NotifyRoomChanged()
    {
        if (minimapRoot != null && minimapRoot.activeSelf)
            RefreshCurrentRoom();
    }

    // ──────────────────────────────────────────
    // 맵 빌드
    // ──────────────────────────────────────────

    private void BuildMap()
    {
        if (iconContainer == null)
        {
            Debug.LogWarning("[MinimapUI] iconContainer가 연결되지 않았습니다.");
            return;
        }

        if (RoomManager.instance == null || RoomManager.instance.allNodes == null)
        {
            Debug.LogWarning("[MinimapUI] RoomManager 또는 allNodes가 없습니다.");
            return;
        }

        // 기존 아이콘 초기화
        ClearIcons();

        List<RoomNode> nodes = RoomManager.instance.allNodes;
        float step = cellSize + cellSpacing;

        foreach (RoomNode node in nodes)
        {
            if (node == null) continue;

            // 프리팹 없으면 기본 Image 조합으로 생성
            MinimapNodeIcon icon = iconPrefab != null
                ? Instantiate(iconPrefab, iconContainer)
                : CreateFallbackIcon(iconContainer);

            // gridPos 를 픽셀 좌표로 변환
            // Unity UI: x→오른쪽, y→위쪽
            Vector2 anchoredPos = new Vector2(
                node.gridPos.x * step,
                node.gridPos.y * step
            );
            icon.RectTrans.anchoredPosition = anchoredPos;

            icon.Setup(node, iconSet, cellSize);
            nodeIcons[node] = icon;
        }

        // 컨테이너 크기를 내용에 맞게 자동 조정 (ScrollRect와 함께 쓸 때 유용)
        FitContainer(nodes, step);
    }

    private void ClearIcons()
    {
        foreach (MinimapNodeIcon icon in nodeIcons.Values)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
        nodeIcons.Clear();
        lastCurrentNode = null;
    }

    /// <summary>노드들의 그리드 범위에 맞게 iconContainer 크기를 조정합니다.</summary>
    private void FitContainer(List<RoomNode> nodes, float step)
    {
        if (nodes == null || nodes.Count == 0) return;

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (RoomNode node in nodes)
        {
            if (node == null) continue;
            minX = Mathf.Min(minX, node.gridPos.x);
            maxX = Mathf.Max(maxX, node.gridPos.x);
            minY = Mathf.Min(minY, node.gridPos.y);
            maxY = Mathf.Max(maxY, node.gridPos.y);
        }

        float width  = (maxX - minX + 1) * step;
        float height = (maxY - minY + 1) * step;
        iconContainer.sizeDelta = new Vector2(width, height);
    }

    // ──────────────────────────────────────────
    // 현재 방 갱신
    // ──────────────────────────────────────────

    private void RefreshCurrentRoom()
    {
        if (RoomManager.instance == null) return;

        RoomNode currentNode = RoomManager.instance.GetCurrentNode();
        if (currentNode == lastCurrentNode) return; // 변화 없으면 스킵

        // 이전 현재방 마커 해제
        if (lastCurrentNode != null && nodeIcons.TryGetValue(lastCurrentNode, out MinimapNodeIcon prevIcon))
        {
            prevIcon.SetCurrent(false, iconSet != null ? iconSet.currentRoomHighlight : Color.yellow);
            prevIcon.SetCleared(lastCurrentNode.cleared, iconSet != null ? iconSet.clearedRoomOverlay : Color.gray, false);
        }

        // 새 현재방 마커 설정
        if (currentNode != null && nodeIcons.TryGetValue(currentNode, out MinimapNodeIcon curIcon))
        {
            curIcon.SetCurrent(true, iconSet != null ? iconSet.currentRoomHighlight : Color.yellow);
            curIcon.SetCleared(false, Color.clear, true);
        }

        // 전체 클리어 상태 갱신
        RefreshClearedStates(currentNode);

        lastCurrentNode = currentNode;
    }

    private void RefreshClearedStates(RoomNode currentNode)
    {
        Color overlayColor = iconSet != null ? iconSet.clearedRoomOverlay : new Color(1f, 1f, 1f, 0.45f);

        foreach (KeyValuePair<RoomNode, MinimapNodeIcon> pair in nodeIcons)
        {
            if (pair.Key == null || pair.Value == null) continue;
            bool isCurrent = pair.Key == currentNode;
            pair.Value.SetCleared(pair.Key.cleared, overlayColor, isCurrent);
        }
    }

    // ──────────────────────────────────────────
    // 프리팹 없을 때 폴백 아이콘 생성
    // ──────────────────────────────────────────

    private MinimapNodeIcon CreateFallbackIcon(Transform parent)
    {
        // 기본 구조: 루트 → Background(Image) → Icon(Image) + CurrentMarker(Image) + ClearedOverlay(Image)
        GameObject root = new GameObject("MinimapNode", typeof(RectTransform));
        root.transform.SetParent(parent, false);

        MinimapNodeIcon nodeIcon = root.AddComponent<MinimapNodeIcon>();

        // Background
        GameObject bgGO = new GameObject("Background", typeof(Image));
        bgGO.transform.SetParent(root.transform, false);
        Image bg = bgGO.GetComponent<Image>();
        bg.rectTransform.anchorMin = Vector2.zero;
        bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.offsetMin = Vector2.zero;
        bg.rectTransform.offsetMax = Vector2.zero;

        // Icon
        GameObject iconGO = new GameObject("Icon", typeof(Image));
        iconGO.transform.SetParent(root.transform, false);
        Image iconImg = iconGO.GetComponent<Image>();
        iconImg.rectTransform.anchorMin = new Vector2(0.15f, 0.15f);
        iconImg.rectTransform.anchorMax = new Vector2(0.85f, 0.85f);
        iconImg.rectTransform.offsetMin = Vector2.zero;
        iconImg.rectTransform.offsetMax = Vector2.zero;
        iconImg.preserveAspect = true;

        // CurrentMarker (테두리 효과용 반투명 노란색 이미지)
        GameObject markerGO = new GameObject("CurrentMarker", typeof(Image));
        markerGO.transform.SetParent(root.transform, false);
        Image marker = markerGO.GetComponent<Image>();
        marker.color = new Color(1f, 0.9f, 0.2f, 0.6f);
        marker.rectTransform.anchorMin = Vector2.zero;
        marker.rectTransform.anchorMax = Vector2.one;
        marker.rectTransform.offsetMin = new Vector2(-3, -3);
        marker.rectTransform.offsetMax = new Vector2(3, 3);
        marker.enabled = false;

        // ClearedOverlay
        GameObject overlayGO = new GameObject("ClearedOverlay", typeof(Image));
        overlayGO.transform.SetParent(root.transform, false);
        Image overlay = overlayGO.GetComponent<Image>();
        overlay.color = new Color(1f, 1f, 1f, 0.45f);
        overlay.rectTransform.anchorMin = Vector2.zero;
        overlay.rectTransform.anchorMax = Vector2.one;
        overlay.rectTransform.offsetMin = Vector2.zero;
        overlay.rectTransform.offsetMax = Vector2.zero;
        overlay.enabled = false;

        // 리플렉션 없이 직접 필드 설정 (SerializedField는 인스펙터 전용이라 코드에서 직접 주입)
        // MinimapNodeIcon의 Setup에서 처리할 수 있도록 Wrapper 사용
        FallbackIconSetup(nodeIcon, bg, iconImg, marker, overlay);

        return nodeIcon;
    }

    /// <summary>
    /// 폴백 생성 아이콘의 내부 Image 필드를 직접 주입합니다.
    /// MinimapNodeIcon에 public setter 없이 처리하기 위해 별도 초기화 메서드를 활용합니다.
    /// </summary>
    private void FallbackIconSetup(MinimapNodeIcon nodeIcon, Image bg, Image icon, Image marker, Image overlay)
    {
        // 직접 직렬화 필드 접근이 필요하므로 MinimapNodeIcon에 초기화 메서드를 노출합니다.
        nodeIcon.InjectComponents(bg, icon, marker, overlay);
    }
}
