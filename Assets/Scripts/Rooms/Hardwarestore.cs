using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
public class Hardwarestore : MonoBehaviour
{
    [Header("아이템 및 가격")]
    public int[] requiredGolds;
    public ItemData[] Items;
    public TextMeshProUGUI[] Prices;
    public int[] ItemsLeft;
    public TextMeshProUGUI[] ItemCount;
    public GameObject[] SoldOut;
    public TextMeshProUGUI currentGold;
    public TextMeshProUGUI currentMatetial;
    [Header("선택지 텍스트")]
    public string buyChoiceText = "Hardwarestorebuy";
    public string exitChoiceText = "Hardwarestoreexit";
    public string noItem = "noitem";
    public string FullBackpack = "FullBackpack";
    [Header("다이얼로그")]
    public string noMoneyDialogueKey = "Hardwarestore_no_money";
    public string[] Dialoguekey;

    [Header("포커스 연출")]
    public float duration = 1f;
    public float targetScaleMultiplier = 2.5f;
    public float distanceFromCamera = 5f;
    private ICurrencyHolder currencyHolder;
    private ICurrencyObservable currencyObservable;

    [Header("포커스 위치")]
    public Vector2 focusPosition = new Vector2(0f, -200f);

    public bool IsTweening { get; private set; }

    [Header("상점 UI")]
    public GameObject shopUI;

    [Header("상품 종류")]
    public int KindOfItem = 0;

    private Camera mainCamera;
    private FocusableObject currentFocusedTarget;
    public int need_Gold;

    public void Setting()
    {
        for(int i = 0; i < requiredGolds.Length; i++)
        {

            if (ItemsLeft[i] > 0)
            {
                ItemCount[i].text = ItemsLeft[i].ToString();
                SoldOut[i].SetActive(false);
                
            }
            if(ItemsLeft[i] <= 0)
            {
                SoldOut[i].SetActive(true);
                ItemCount[i].text = ItemsLeft[i].ToString();
                ItemCount[i].color = Color.red;
            }
            Prices[i].text = requiredGolds[i].ToString() + "G";
        }
        currencyHolder = CurrencyManager.instance;
        currencyObservable = CurrencyManager.instance;

        if (currentGold != null)
            currentGold.text = currencyHolder.GetCurrency(CurrencyType.Gold).ToString();

        if (currentMatetial != null)
            currentMatetial.text = currencyHolder.GetCurrency(CurrencyType.Material).ToString();
    }
    private void OnEnable()
    {
        Setting();
        FocusManager.RequestFocusOut += OnRequestFocusOut;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && shopUI.activeSelf)
        {
            CloseShop();
        }
    }
    public void CloseShop()
    {
        shopUI.SetActive(false);
    }

    private void OnDisable()
    {
        FocusManager.RequestFocusOut -= OnRequestFocusOut;

        if (BattleManager.instance != null &&
            BattleManager.instance.IsSelectingFixerTarget)
        {
            BattleManager.instance.CancelFixerTargetSelection();
        }
    }
    private void OnRequestFocusOut()
    {

        if (BattleManager.instance != null &&
            BattleManager.instance.IsSelectingFixerTarget)
            return;

        FocusOut();
    }
    // ─────────────────────────────────────────────
    // 상점 및 구매
    // ─────────────────────────────────────────────

    public void OnPointerClick()
    {
        FocusIn(gameObject);
    }

    public void TurnOntheShopUI()
    {
        Debug.Log("[Hardwarestore] 상점 UI 켜기");
        Setting();
        if (shopUI != null)
        {
            Debug.Log("[Hardwarestore] shopUI 활성화");

            shopUI.SetActive(true);

            RectTransform rect = shopUI.GetComponent<RectTransform>();

            if (rect != null)
                rect.SetAsLastSibling();
        }
    }
    public void ItemBuy(int itemKind)
    {
        KindOfItem = itemKind;
        TryPurchase();
    }

    private void TryPurchase()
    {
        if (CurrencyManager.instance == null)
        {
            Debug.LogWarning("[BlackMarket] CurrencyManager를 찾을 수 없습니다.");
            return;
        }

        need_Gold = requiredGolds[KindOfItem];
        
        bool hasEnoughGold =
            CurrencyManager.instance.HasEnough(CurrencyType.Gold, need_Gold);

        if (!hasEnoughGold)
        {
            Debug.Log("[Hardwarestore] 재화가 부족합니다.");

            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(
                    new string[] { noMoneyDialogueKey });
            }
            Setting();
            return;
        }
        if (ItemsLeft[KindOfItem] <= 0)
        {
            Debug.Log("[Hardwarestore] 재고가 없습니다");
            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(
                    new string[] { noItem });
            }
            Setting();
            return;
        }
        if (Items[KindOfItem].currentCount >= Items[KindOfItem].maxStackCount) {
            Debug.Log("[Hardwarestore] 가방이 가득 찼습니다");
            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(
                    new string[] { FullBackpack });
            }
            Setting();
            return;
        }
        if (need_Gold > 0)
        {
            CurrencyManager.instance.SpendCurrency(
                CurrencyType.Gold, need_Gold);
        }
        ItemData item = Items[KindOfItem];
        ItemManager.Instance.AddItem(item);
        ItemsLeft[KindOfItem] -= 1;
        Setting();
        Save.CommitSave();
    }
    private void FocusIn(GameObject target)
    {
        if (IsTweening || target == null)
            return;

        FocusableObject focusable = target.GetComponent<FocusableObject>();

        if (focusable == null)
            focusable = target.AddComponent<FocusableObject>();

        if (focusable.IsFocused)
            return;

        if (currentFocusedTarget != null &&
            currentFocusedTarget != focusable)
        {
            FocusOut();

            if (IsTweening)
                return;
        }

        currentFocusedTarget = focusable;
        focusable.IsFocused = true;
        IsTweening = true;

        RectTransform rectTransform = target.GetComponent<RectTransform>();
        Sequence sequence = DOTween.Sequence();

        if (rectTransform != null)
        {
            rectTransform.SetAsLastSibling();

            sequence.Join(
                rectTransform.DOAnchorPos(focusPosition, duration));

            sequence.Join(
                rectTransform.DOScale(
                    focusable.OriginalScale * targetScaleMultiplier, duration));
        }
        else
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
            {
                Vector3 targetPosition = mainCamera.ViewportToWorldPoint(
                    new Vector3(0.5f, 0.5f, distanceFromCamera));

                sequence.Join(target.transform.DOMove(targetPosition, duration));
                sequence.Join(target.transform.DOScale(
                    focusable.OriginalScale * targetScaleMultiplier, duration));

                sequence.Join(target.transform.DORotateQuaternion(
                    Quaternion.LookRotation(mainCamera.transform.forward), duration));
            }
        }

        sequence
            .SetEase(Ease.OutCubic)
            .OnComplete(() => IsTweening = false);

        ShowDialogueChoices();
    }

    private void ShowDialogueChoices()
    {
        if (DialogueManager.instance == null)
            return;

        string[] dialogueKeys =
            Dialoguekey != null && Dialoguekey.Length > 0
                ? Dialoguekey
                : new string[] { "Hardwarestore" };

        DialogueManager.instance.StartDialogueWithChoices(
            dialogueKeys,
            new DialogueChoice[]
            {
                new DialogueChoice
                {
                    text = LocalizationManager.GetLocalizedString(buyChoiceText),
                    onSelected = CreateBuyEvent()
                },
                new DialogueChoice
                {
                    text = LocalizationManager.GetLocalizedString(exitChoiceText),
                    onSelected = FocusOutEvent()
                }
            });
    }
    private UnityEvent FocusOutEvent()
    {
        UnityEvent FocusOutEvent = new UnityEvent();
        FocusOutEvent.AddListener(FocusOut);
        return FocusOutEvent;
    }
    private UnityEvent CreateBuyEvent()
    {
        UnityEvent buyEvent = new UnityEvent();
        buyEvent.AddListener(TurnOntheShopUI);
        return buyEvent;
    }

    public void FocusOut()
    {
        if (IsTweening || currentFocusedTarget == null)
            return;

        IsTweening = true;

        FocusableObject focusable = currentFocusedTarget;
        GameObject target = focusable.gameObject;
        RectTransform rectTransform = target.GetComponent<RectTransform>();

        Sequence sequence = DOTween.Sequence();

        if (rectTransform != null)
        {
            sequence.Join(
                rectTransform.DOAnchorPos(focusable.OriginalPosition, duration));

            sequence.Join(
                rectTransform.DOScale(focusable.OriginalScale, duration));

            sequence.SetEase(Ease.OutCubic).OnComplete(() =>
            {
                rectTransform.SetSiblingIndex(focusable.OriginalSiblingIndex);
                FinishFocusOut(focusable);
            });
        }
        else
        {
            sequence.Join(
                target.transform.DOMove(focusable.OriginalPosition, duration));

            sequence.Join(
                target.transform.DOScale(focusable.OriginalScale, duration));

            sequence.Join(
                target.transform.DORotateQuaternion(
                    focusable.OriginalRotation, duration));

            sequence.SetEase(Ease.OutCubic).OnComplete(() =>
            {
                FinishFocusOut(focusable);
            });
        }
    }

    private void FinishFocusOut(FocusableObject focusable)
    {
        focusable.IsFocused = false;
        currentFocusedTarget = null;
        IsTweening = false;
    }
    public void CloseShopUI()
    {
        if (shopUI != null)
            shopUI.SetActive(false);
    }
}
