
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

        RegisterButton(forwardButton, OnForwardClicked);
        RegisterButton(backwardButton, OnBackwardClicked);
        RegisterButton(leftButton, OnLeftClicked);
        RegisterButton(rightButton, OnRightClicked);
    }

    private void Start()
    {
        ApplySprites();
        HideAll();
    }

    private void RegisterButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.AddListener(action);
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
