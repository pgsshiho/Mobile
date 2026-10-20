
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public class BackpackUIManager : MonoBehaviour, ICurrencyHolder
{
    [Header("재화 UI")]
    public TextMeshProUGUI Money;
    public TextMeshProUGUI Material;

    [Header("아이템 데이터")]
    public ItemData Scrap_Metal_Data;
    public ItemData Emergency_Battery_Data;
    public ItemData Antenna_Data;
    public ItemData Fuse_Data;
    public ItemData Sandpaper_Data;
    public ItemData Rust_Remover_Data;
    public ItemData Coolant_Data;
    public ItemData Fire_Extinguisher_Data;
    public ItemData Brush_Data;
    public ItemData Electrical_Tape_Data;
    public ItemData Waterproof_Tape_Data;

    [Header("아이템 개수 UI")]
    public TextMeshProUGUI Scrap_Metal;
    public TextMeshProUGUI Emergency_Battery;
    public TextMeshProUGUI Antenna;
    public TextMeshProUGUI Fuse;
    public TextMeshProUGUI Sandpaper;
    public TextMeshProUGUI Rust_Remover;
    public TextMeshProUGUI Coolant;
    public TextMeshProUGUI Fire_Extinguisher;
    public TextMeshProUGUI Brush;
    public TextMeshProUGUI Electrical_Tape;
    public TextMeshProUGUI Waterproof_Tape;

    [Header("아이템 최대 소지량 표시")]
    public GameObject Scrap_Metal_Full;
    public GameObject Emergency_Battery_Full;
    public GameObject Antenna_Full;
    public GameObject Fuse_Full;
    public GameObject Sandpaper_Full;
    public GameObject Rust_Remover_Full;
    public GameObject Coolant_Full;
    public GameObject Fire_Extinguisher_Full;
    public GameObject Brush_Full;
    public GameObject Electrical_Tape_FULL;
    public GameObject Waterproof_Tape_Full;

    private ICurrencyHolder currencyHolder;
    private ICurrencyObservable currencyObservable;

    private ItemManager itemManager;
    public ItemRuntime current;
    private readonly HashSet<Button> boundItemButtons = new HashSet<Button>();

    private void Start()
    {
        if (CurrencyManager.instance != null)
        {
            currencyHolder = CurrencyManager.instance;
            currencyObservable = CurrencyManager.instance;
            currencyObservable.OnCurrencyChanged += HandleCurrencyChanged;
        }
        else
        {
            Debug.LogWarning("[BackpackUI] CurrencyManager를 찾을 수 없습니다.");
        }

        itemManager = ItemManager.Instance;

        if (itemManager != null)
        {
            itemManager.OnInventoryChanged += Setting;
        }
        else
        {
            Debug.LogWarning("[BackpackUI] ItemManager를 찾을 수 없습니다.");
        }

        Setting();
        BindItemButtons();
    }

    private void OnDestroy()
    {
        if (currencyObservable != null)
            currencyObservable.OnCurrencyChanged -= HandleCurrencyChanged;

        if (itemManager != null)
            itemManager.OnInventoryChanged -= Setting;
    }

    // 전체 UI 갱신
    public void Setting()
    {
        if (currencyHolder != null)
        {
            if (Money != null)
                Money.text = currencyHolder.GetCurrency(CurrencyType.Gold).ToString();

            if (Material != null)
                Material.text = currencyHolder.GetCurrency(CurrencyType.Material).ToString();
        }

        RefreshItem(Scrap_Metal_Data, Scrap_Metal, Scrap_Metal_Full);
        RefreshItem(Emergency_Battery_Data, Emergency_Battery, Emergency_Battery_Full);
        RefreshItem(Antenna_Data, Antenna, Antenna_Full);
        RefreshItem(Fuse_Data, Fuse, Fuse_Full);
        RefreshItem(Sandpaper_Data, Sandpaper, Sandpaper_Full);
        RefreshItem(Rust_Remover_Data, Rust_Remover, Rust_Remover_Full);
        RefreshItem(Coolant_Data, Coolant, Coolant_Full);
        RefreshItem(Fire_Extinguisher_Data, Fire_Extinguisher, Fire_Extinguisher_Full);
        RefreshItem(Brush_Data, Brush, Brush_Full);
        RefreshItem(Electrical_Tape_Data, Electrical_Tape, Electrical_Tape_FULL);
        RefreshItem(Waterproof_Tape_Data, Waterproof_Tape, Waterproof_Tape_Full);
        BindItemButtons();
    }

    // 개별 아이템 개수 및 최대 소지량 표시 갱신
    private void RefreshItem(
        ItemData data,
        TextMeshProUGUI countText,
        GameObject fullObject)
    {
        int count = 0;

        if (itemManager != null && data != null)
            count = itemManager.GetItemCount(data);

        if (countText != null)
            countText.text = count.ToString();

        if (fullObject != null)
        {
            bool isFull = data != null &&
                          count >= data.maxStackCount;

            fullObject.SetActive(isFull);
        }
    }

    // 재화 변경 이벤트
    private void HandleCurrencyChanged(
        CurrencyType type, int delta, int newValue)
    {
        switch (type)
        {
            case CurrencyType.Gold:
                if (Money != null)
                    Money.text = newValue.ToString();
                break;

            case CurrencyType.Material:
                if (Material != null)
                    Material.text = newValue.ToString();
                break;
        }
    }

    // ICurrencyHolder 구현
    public int GetCurrency(CurrencyType type)
    {
        return currencyHolder != null
            ? currencyHolder.GetCurrency(type)
            : 0;
    }

    public bool HasEnough(CurrencyType type, int amount)
    {
        return currencyHolder != null &&
               currencyHolder.HasEnough(type, amount);
    }

    public void UsdeItemCheck(ItemRuntime item, RectTransform buttonRect)
    {
        Debug.Log(
            $"[아이템 검사] item={item != null}, " +
            $"data={item?.data != null}, " +
            $"count={item?.count}, " +
            $"buttonRect={buttonRect != null}"
        );

        if (item == null || item.data == null || item.count <= 0)
        {
            Debug.LogWarning("[아이템 검사] 아이템이 없거나 개수가 0입니다.");
            return;
        }

        if (buttonRect == null)
        {
            Debug.LogWarning("[아이템 검사] 버튼의 RectTransform이 없습니다.");
            return;
        }

        current = item;
        ShowChoiceMenu(buttonRect);
    }

    public void UsdeItem()
    {
        if (current == null || current.data == null)
            return;

        if (BattleManager.instance == null || !BattleManager.instance.BeginItemTargetSelection(this))
        {
            Debug.LogWarning("[BackpackUI] 아이템을 사용할 전투 파티 위치를 찾을 수 없습니다.");
            return;
        }

        HideItemChoices();
    }

    public void CancelItemUse()
    {
        HideItemChoices();
        BattleManager.instance?.CancelItemTargetSelection();
        current = null;
    }

    /// <summary>BattleManager가 대상 아군을 클릭했을 때 호출합니다.</summary>
    public bool ApplySelectedItemTo(Unit target)
    {
        if (current == null || current.data == null || ItemManager.Instance == null)
            return false;

        bool used = ItemManager.Instance.UseItem(current.data, target);
        if (used)
        {
            current = null;
            Setting();
        }

        return used;
    }

    private void HideItemChoices()
    {
        if (DialogueManager.instance != null)
            DialogueManager.instance.choiceController?.HideChoices();
    }


    private void ShowChoiceMenu(RectTransform sourceButton)
    {
        if (DialogueManager.instance == null)
        {
            Debug.LogError("[BackpackUI] DialogueManager.instance가 null입니다.");
            return;
        }

        DialogueManager.instance.EnsureChoiceController();

        DialogueChoiceController controller =
            DialogueManager.instance.choiceController;

        if (controller == null)
        {
            Debug.LogError("[BackpackUI] choiceController가 연결되지 않았습니다.");
            return;
        }

        // 선택지 UI 위치 설정
        RectTransform choiceRect = controller.choicePanel != null
            ? controller.choicePanel.GetComponent<RectTransform>()
            : controller.GetComponent<RectTransform>();

        if (choiceRect != null && sourceButton != null)
        {
            Canvas.ForceUpdateCanvases();

            Vector3[] corners = new Vector3[4];
            sourceButton.GetWorldCorners(corners);

            choiceRect.position = (corners[2] + corners[3]) * 0.5f;
        }

        // Localization Key로 번역된 선택지 생성
        DialogueChoice useChoice = new DialogueChoice
        {
            text = LocalizationManager.GetLocalizedString("한다"),
            onSelected = new UnityEngine.Events.UnityEvent()
        };

        useChoice.onSelected.AddListener(UsdeItem);

        DialogueChoice cancelChoice = new DialogueChoice
        {
            text = LocalizationManager.GetLocalizedString("안한다"),
            onSelected = new UnityEngine.Events.UnityEvent()
        };

        cancelChoice.onSelected.AddListener(CancelItemUse);

        controller.ShowChoices(
            new[] { useChoice, cancelChoice },
            choice =>
            {
                // 선택한 뒤 UI를 닫고 선택 동작 실행
                controller.HideChoices();
                choice?.onSelected?.Invoke();
            }
        );
    }
    private void BindItemButtons()
    {
        if (itemManager == null || itemManager.inventory == null)
            return;

        // BackPackUI의 형제 아이템 버튼은 모두 ItemData의 이름으로 연결한다.
        // 따라서 Scrap Metal뿐 아니라 같은 목록의 모든 아이템이 동일한 사용 흐름을 가진다.
        foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
        {
            if (button == null || !button.gameObject.scene.IsValid())
                continue;

            ItemRuntime runtime = itemManager.inventory.Find(entry =>
                entry != null && entry.data != null && entry.data.itemName == button.name);
            if (runtime == null)
                continue;

            ItemButton itemButton = button.GetComponent<ItemButton>() ?? button.gameObject.AddComponent<ItemButton>();
            itemButton.Configure(this, runtime);

            // 씬에서 이미 ItemButton.OnClick이 연결된 버튼은 중복 등록하지 않는다.
            if (boundItemButtons.Add(button) && !HasPersistentItemButtonClick(button, itemButton))
                button.onClick.AddListener(itemButton.OnClick);
        }
    }

    private static bool HasPersistentItemButtonClick(Button button, ItemButton itemButton)
    {
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) == itemButton &&
                button.onClick.GetPersistentMethodName(i) == nameof(ItemButton.OnClick))
            {
                return true;
            }
        }

        return false;
    }
}
