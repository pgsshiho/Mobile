using UnityEngine;

public class ItemButton : MonoBehaviour
{
    public BackpackUIManager backpackUI;
    public ItemRuntime item;

    public void Configure(BackpackUIManager owner, ItemRuntime runtime)
    {
        backpackUI = owner;
        item = runtime;
    }

    public void OnClick()
    {
        if (backpackUI == null || item == null)
            return;

        backpackUI.UsdeItemCheck(
            item,
            GetComponent<RectTransform>()
        );
    }
}
