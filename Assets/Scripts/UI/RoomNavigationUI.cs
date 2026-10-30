
using UnityEngine;
using UnityEngine.UI;

public class RoomNavigationUI : MonoBehaviour
{
    public static RoomNavigationUI instance;

    [Header("Arrow Buttons")]
    public Button forwardButton;
    public Button backwardButton;
    public Button leftButton;
    public Button rightButton;

    [Header("Arrow Images")]
    public Sprite forwardArrowSprite;
    public Sprite backwardArrowSprite;
    public Sprite leftArrowSprite;
    public Sprite rightArrowSprite;

    private Button selectedMoveButton;

    public Transform GetSelectedMoveButton()
    {
        return selectedMoveButton != null
            ? selectedMoveButton.transform
            : null;
    }

    private void Awake()
    {
        instance = this;

        RegisterButton(forwardButton, OnForwardClicked, nameof(RoomManager.MoveForward));
        RegisterButton(backwardButton, OnBackwardClicked, nameof(RoomManager.MoveBackward));
        RegisterButton(leftButton, OnLeftClicked, nameof(RoomManager.MoveLeft));
        RegisterButton(rightButton, OnRightClicked, nameof(RoomManager.MoveRight));
    }

    private void Start()
    {
        ApplySprites();
        HideAll();
    }

    private void RegisterButton(
        Button button,
        UnityEngine.Events.UnityAction action,
        string roomManagerMethod)
    {
        if (button == null)
            return;

        // 씬 인스펙터에 RoomManager 이동 함수가 이미 연결된 경우가 있다.
        // 같은 이동 함수를 런타임에서 한 번 더 연결하면 한 번의 터치가 두 번 이동한다.
        if (HasPersistentRoomManagerMove(button, roomManagerMethod))
        {
            // 선택 버튼 정보는 카메라 포커스 등에 사용하므로 이동 없이 이것만 갱신한다.
            button.onClick.AddListener(() => selectedMoveButton = button);
            return;
        }

        button.onClick.AddListener(action);
    }

    private static bool HasPersistentRoomManagerMove(Button button, string methodName)
    {
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) is RoomManager &&
                button.onClick.GetPersistentMethodName(i) == methodName)
            {
                return true;
            }
        }

        return false;
    }

    private void OnForwardClicked()
    {
        selectedMoveButton = forwardButton;

        if (RoomManager.instance != null)
            RoomManager.instance.MoveForward();
    }

    private void OnBackwardClicked()
    {
        selectedMoveButton = backwardButton;

        if (RoomManager.instance != null)
            RoomManager.instance.MoveBackward();
    }

    private void OnLeftClicked()
    {
        selectedMoveButton = leftButton;

        if (RoomManager.instance != null)
            RoomManager.instance.MoveLeft();
    }

    private void OnRightClicked()
    {
        selectedMoveButton = rightButton;

        if (RoomManager.instance != null)
            RoomManager.instance.MoveRight();
    }

    private void ApplySprites()
    {
        SetButtonSprite(forwardButton, forwardArrowSprite);
        SetButtonSprite(backwardButton, backwardArrowSprite);
        SetButtonSprite(leftButton, leftArrowSprite);
        SetButtonSprite(rightButton, rightArrowSprite);
    }

    private void SetButtonSprite(Button button, Sprite sprite)
    {
        if (button == null || sprite == null)
            return;

        Image image = button.GetComponent<Image>();

        if (image != null)
            image.sprite = sprite;
    }

    public void Refresh(RoomNode node)
    {
        if (node == null)
        {
            HideAll();
            return;
        }

        SetButton(forwardButton, node.forwardRoom != null);
        SetButton(backwardButton, node.backwardRoom != null);
        SetButton(leftButton, node.leftRoom != null);
        SetButton(rightButton, node.rightRoom != null);
    }

    public void SetNavigationActive(bool active)
    {
        if (!active)
        {
            HideAll();
            return;
        }

        Refresh(RoomManager.instance != null
            ? RoomManager.instance.GetCurrentNode()
            : null);
    }

    private void SetButton(Button button, bool visible)
    {
        if (button != null)
            button.gameObject.SetActive(visible);
    }

    public void HideAll()
    {
        SetButton(forwardButton, false);
        SetButton(backwardButton, false);
        SetButton(leftButton, false);
        SetButton(rightButton, false);
    }
}
