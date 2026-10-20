using UnityEngine;

/// <summary>아이템 슬롯 UI와 인벤토리 런타임 데이터를 연결하는 얇은 어댑터입니다.</summary>
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
        Debug.Log(
            $"[ItemButton] backpackUI={backpackUI != null}, " +
            $"item={item != null}, " +
            $"itemData={item?.data != null}"
        );

        if (backpackUI == null)
        {
            Debug.LogError("[ItemButton] backpackUI가 연결되지 않았습니다.");
            return;
        }

        if (item == null)
        {
            Debug.LogError("[ItemButton] item이 연결되지 않았습니다.");
            return;
        }

        backpackUI.UsdeItemCheck(item, GetComponent<RectTransform>());
    }
}
