using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 현재 방의 방향별 연결 상태에 따라
/// 화살표 버튼을 자동으로 표시/숨깁니다.
/// </summary>
public class RoomNavigationUI : MonoBehaviour
{
    public static RoomNavigationUI instance;

    [Header("Arrow Buttons")]
    [Tooltip("전방 화살표 버튼")]
    public Button forwardButton;

    [Tooltip("후방 화살표 버튼")]
    public Button backwardButton;

    [Tooltip("좌측 화살표 버튼")]
    public Button leftButton;

    [Tooltip("우측 화살표 버튼")]
    public Button rightButton;

    [Header("Arrow Images")]
    [Tooltip("전방 화살표 이미지")]
    public Sprite forwardArrowSprite;

    [Tooltip("후방 화살표 이미지")]
    public Sprite backwardArrowSprite;

    [Tooltip("좌측 화살표 이미지")]
    public Sprite leftArrowSprite;

    [Tooltip("우측 화살표 이미지")]
    public Sprite rightArrowSprite;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // ============================================
        // 스프라이트 적용
        // ============================================

        ApplySprites();

        // 시작 시 숨김
        HideAll();
    }

    // ============================================================
    // Sprite 적용
    // ============================================================

    private void ApplySprites()
    {
        if (forwardButton != null &&
            forwardArrowSprite != null)
        {
            Image img =
                forwardButton.GetComponent<Image>();

            if (img != null)
            {
                img.sprite =
                    forwardArrowSprite;
            }
        }

        if (backwardButton != null &&
            backwardArrowSprite != null)
        {
            Image img =
                backwardButton.GetComponent<Image>();

            if (img != null)
            {
                img.sprite =
                    backwardArrowSprite;
            }
        }

        if (leftButton != null &&
            leftArrowSprite != null)
        {
            Image img =
                leftButton.GetComponent<Image>();

            if (img != null)
            {
                img.sprite =
                    leftArrowSprite;
            }
        }

        if (rightButton != null &&
            rightArrowSprite != null)
        {
            Image img =
                rightButton.GetComponent<Image>();

            if (img != null)
            {
                img.sprite =
                    rightArrowSprite;
            }
        }
    }

    // ============================================================
    // UI 갱신
    // ============================================================

    public void Refresh(RoomNode node)
    {
        if (node == null)
        {
            HideAll();
            return;
        }

        // 전방
        SetButton(
            forwardButton,
            node.forwardRoom != null
        );

        SetButton(
            backwardButton,
            node.backwardRoom != null
        );

        // 좌측
        SetButton(
            leftButton,
            node.leftRoom != null
        );

        // 우측
        SetButton(
            rightButton,
            node.rightRoom != null
        );
    }

    // ============================================================
    // 전투 중 네비게이션 활성/비활성
    // ============================================================

    public void SetNavigationActive(
        bool active)
    {
        if (!active)
        {
            HideAll();
        }
        else
        {
            RoomNode current =
                RoomManager.instance != null
                    ? RoomManager.instance.GetCurrentNode()
                    : null;

            Refresh(current);
        }
    }

    // ============================================================
    // 버튼 활성화
    // ============================================================

    private void SetButton(
        Button btn,
        bool visible)
    {
        if (btn != null)
        {
            btn.gameObject.SetActive(
                visible
            );
        }
    }

    // ============================================================
    // 모든 버튼 숨김
    // ============================================================

    public void HideAll()
    {
        SetButton(
            forwardButton,
            false
        );

        SetButton(
            backwardButton,
            false
        );

        SetButton(
            leftButton,
            false
        );

        SetButton(
            rightButton,
            false
        );
    }
}
