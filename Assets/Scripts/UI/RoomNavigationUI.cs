using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 현재 방의 방향별 연결 상태에 따라 화살표 버튼을 자동으로 표시/숨깁니다.
/// Canvas 안에 RoomNavigationUI 오브젝트를 만들고 이 컴포넌트를 붙이세요.
/// </summary>
public class RoomNavigationUI : MonoBehaviour
{
    public static RoomNavigationUI instance;

    [Header("Arrow Buttons (버튼 오브젝트 할당)")]
    [Tooltip("전방 화살표 버튼 (위쪽)")]
    public Button forwardButton;

    [Tooltip("후방 화살표 버튼 (아래쪽)")]
    public Button backwardButton;

    [Tooltip("좌측 화살표 버튼 (왼쪽)")]
    public Button leftButton;

    [Tooltip("우측 화살표 버튼 (오른쪽)")]
    public Button rightButton;

    [Header("Arrow Images (이미지 교체용 - 선택 사항)")]
    [Tooltip("전방 화살표 이미지 (Sprite)")]
    public Sprite forwardArrowSprite;

    [Tooltip("후방 화살표 이미지 (Sprite)")]
    public Sprite backwardArrowSprite;

    [Tooltip("좌측 화살표 이미지 (Sprite)")]
    public Sprite leftArrowSprite;

    [Tooltip("우측 화살표 이미지 (Sprite)")]
    public Sprite rightArrowSprite;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 버튼 클릭 이벤트 연결
        if (forwardButton != null)
            forwardButton.onClick.AddListener(() => RoomManager.instance.MoveForward());

        if (backwardButton != null)
            backwardButton.onClick.AddListener(() => RoomManager.instance.MoveBackward());

        if (leftButton != null)
            leftButton.onClick.AddListener(() => RoomManager.instance.MoveLeft());

        if (rightButton != null)
            rightButton.onClick.AddListener(() => RoomManager.instance.MoveRight());

        // 스프라이트 자동 적용
        ApplySprites();

        // 시작 시 전부 숨기기 (RoomManager.EnterNode()에서 Refresh 호출됨)
        HideAll();
    }

    /// <summary>
    /// 할당된 Arrow Sprite를 각 버튼의 Image 컴포넌트에 자동 적용합니다.
    /// </summary>
    void ApplySprites()
    {
        if (forwardButton != null && forwardArrowSprite != null)
        {
            Image img = forwardButton.GetComponent<Image>();
            if (img != null) img.sprite = forwardArrowSprite;
        }

        if (backwardButton != null && backwardArrowSprite != null)
        {
            Image img = backwardButton.GetComponent<Image>();
            if (img != null) img.sprite = backwardArrowSprite;
        }

        if (leftButton != null && leftArrowSprite != null)
        {
            Image img = leftButton.GetComponent<Image>();
            if (img != null) img.sprite = leftArrowSprite;
        }

        if (rightButton != null && rightArrowSprite != null)
        {
            Image img = rightButton.GetComponent<Image>();
            if (img != null) img.sprite = rightArrowSprite;
        }
    }

    /// <summary>
    /// 현재 RoomNode의 방향별 연결 여부에 따라 화살표 버튼을 표시/숨깁니다.
    /// RoomManager.EnterNode() 에서 자동으로 호출됩니다.
    /// </summary>
    public void Refresh(RoomNode node)
    {
        if (node == null)
        {
            HideAll();
            return;
        }

        SetButton(forwardButton, node.forwardRoom != null);
        SetButton(backwardButton, node.previousRoom != null || node.backwardRoom != null);
        SetButton(leftButton, node.leftRoom != null);
        SetButton(rightButton, node.rightRoom != null);
    }

    /// <summary>전투 중에는 모든 화살표를 숨기고, 전투가 끝나면 다시 표시합니다.</summary>
    public void SetNavigationActive(bool active)
    {
        // active=true면 Refresh로 다시 갱신, false면 전부 숨김
        if (!active)
        {
            HideAll();
        }
        else
        {
            RoomNode current = (RoomManager.instance != null) ? RoomManager.instance.GetCurrentNode() : null;
            Refresh(current);
        }
    }

    void SetButton(Button btn, bool visible)
    {
        if (btn != null)
        {
            btn.gameObject.SetActive(visible);
        }
    }

    void HideAll()
    {
        SetButton(forwardButton, false);
        SetButton(backwardButton, false);
        SetButton(leftButton, false);
        SetButton(rightButton, false);
    }
}
