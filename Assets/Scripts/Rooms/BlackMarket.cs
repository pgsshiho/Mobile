
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[System.Serializable]
public class UnitSelectedEvent : UnityEvent<Unit> { }

public class BlackMarket : MonoBehaviour
{
    [Header("로봇 목록")]
    public Unit[] Robots;
    public bool cantouchRobot = false;
    public bool isSelectingRobot = false;

    [Header("구매 비용")]
    [Tooltip("구매에 필요한 골드")]
    public int requiredGold = 100;

    [Tooltip("구매에 필요한 재료")]
    public int requiredMaterial = 0;

    [Header("선택지 텍스트")]
    public string buyChoiceText = "BlackMarketBuy";
    public string exitChoiceText = "BlackMarketLeave";

    [Header("다이얼로그")]
    public string noMoneyDialogueKey = "BLACKMARKET_NO_MONEY";
    public string[] Dialoguekey;

    [Header("포커스 연출")]
    public float duration = 1f;
    public float targetScaleMultiplier = 2.5f;
    public float distanceFromCamera = 5f;

    public bool IsTweening { get; private set; }

    [Header("로봇 배치 위치")]
    public Transform[] repairSpawnPositions;

    [Header("상점 UI")]
    public GameObject shopUI;
    public GameObject shoperHideUI;

    [Header("로봇 선택 이벤트")]
    public UnitSelectedEvent onRobotSelected = new UnitSelectedEvent();

    public Unit current;

    [Header("상호작용 중 숨길 UI")]
    public GameObject[] hideUIObjects;
    

    [Header("상품 종류")]
    public int KindOfItem = 0;

    private Camera mainCamera;
    private FocusableObject currentFocusedTarget;
    private bool[] originalUIStates;
    private TurnOffUIForPrefabs uiPrefabManager;
    [Header("포커스 위치")]
    public Vector2 focusPosition = new Vector2(0f, -250f);
    private void Awake()
    {
        uiPrefabManager = FindAnyObjectByType<TurnOffUIForPrefabs>();

        if (uiPrefabManager != null)
            hideUIObjects = uiPrefabManager.TurnOffs;

        if (hideUIObjects == null)
            return;

        originalUIStates = new bool[hideUIObjects.Length];

        for (int i = 0; i < hideUIObjects.Length; i++)
        {
            if (hideUIObjects[i] != null)
                originalUIStates[i] = hideUIObjects[i].activeSelf;
        }
    }

    private void OnEnable()
    {
        FocusManager.RequestFocusOut += OnRequestFocusOut;
    }

    private void OnDisable()
    {
        FocusManager.RequestFocusOut -= OnRequestFocusOut;

        if (BattleManager.instance != null &&
            BattleManager.instance.IsSelectingFixerTarget)
        {
            BattleManager.instance.CancelFixerTargetSelection();
        }

        isSelectingRobot = false;
        RestoreInteractionUI();
    }


    // ─────────────────────────────────────────────
    // UI 표시 및 복구
    // ─────────────────────────────────────────────

    private void HideInteractionUI()
    {
        if (hideUIObjects == null)
            return;

        foreach (GameObject obj in hideUIObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    public void RestoreInteractionUI()
    {
        if (hideUIObjects == null || originalUIStates == null)
            return;

        for (int i = 0; i < hideUIObjects.Length; i++)
        {
            if (hideUIObjects[i] != null)
                hideUIObjects[i].SetActive(originalUIStates[i]);
        }
    }

    private void OnRequestFocusOut()
    {
        // 다른 시스템의 포커스 해제 요청으로 선택이 끊기지 않게 한다.
        if (isSelectingRobot)
            return;

        if (BattleManager.instance != null &&
            BattleManager.instance.IsSelectingFixerTarget)
            return;

        FocusOut();
    }

    // ─────────────────────────────────────────────
    // 파티 로봇 배치 및 회수
    // ─────────────────────────────────────────────

    public void PlacePartyAtBlackMarketPositions()
    {
        if (PartyManager.instance == null)
        {
            Debug.LogWarning("[BlackMarket] PartyManager를 찾을 수 없습니다.");
            return;
        }

        Transform[] spawnPoints = GetAvailableSpawnPositions();

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[BlackMarket] 로봇 배치 위치를 찾을 수 없습니다.");
            return;
        }

        PartyManager.instance.PlacePartyAtPositions(spawnPoints);
        Debug.Log("[BlackMarket] 파티 로봇 배치 완료");
    }

    private Transform[] GetAvailableSpawnPositions()
    {
        Camera cam = mainCamera != null ? mainCamera : Camera.main;

        if (repairSpawnPositions != null &&
            repairSpawnPositions.Length > 0 &&
            repairSpawnPositions[0] != null)
        {
            if (cam == null)
                return repairSpawnPositions;

            Vector3 viewportPosition =
                cam.WorldToViewportPoint(repairSpawnPositions[0].position);

            bool isVisible =
                viewportPosition.z > 0 &&
                viewportPosition.x >= -0.2f &&
                viewportPosition.x <= 1.2f &&
                viewportPosition.y >= -0.2f &&
                viewportPosition.y <= 1.2f;

            if (isVisible)
                return repairSpawnPositions;

            Debug.LogWarning(
                "[BlackMarket] 배치 위치가 카메라 시야 밖에 있어 기본 파티 위치를 사용합니다.");
        }

        if (BattleManager.instance != null &&
            BattleManager.instance.Party != null &&
            BattleManager.instance.Party.Length > 0 &&
            BattleManager.instance.Party[0] != null)
        {
            return BattleManager.instance.Party;
        }

        return repairSpawnPositions;
    }
    public void RecallRepairParty()
    {
        if (PartyManager.instance != null)
        {
            PartyManager.instance.RecallParty();
        }

        PartymenuSpawn[] spawners =
            FindObjectsByType<PartymenuSpawn>();

        foreach (PartymenuSpawn spawner in spawners)
        {
            if (spawner != null)
                spawner.RecallSpawnedUnits();
        }

        Debug.Log("[BlackMarket] 수리용 파티 유닛 회수 완료");
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
        Debug.Log("[BlackMarket] 상점 UI 켜기");

        if (shopUI != null)
        {
            Debug.Log("[BlackMarket] shopUI 활성화");

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
        if (!TryGetItemPrice(out int goldCost))
        {
            Debug.LogWarning($"[BlackMarket] 잘못된 상품 종류: {KindOfItem}");
            return;
        }

        if (CurrencyManager.instance == null)
        {
            Debug.LogWarning("[BlackMarket] CurrencyManager를 찾을 수 없습니다.");
            return;
        }

        requiredGold = goldCost;

        bool hasEnoughGold =
            CurrencyManager.instance.HasEnough(CurrencyType.Gold, requiredGold);

        bool hasEnoughMaterial =
            requiredMaterial <= 0 ||
            CurrencyManager.instance.HasEnough(
                CurrencyType.Material, requiredMaterial);

        if (!hasEnoughGold || !hasEnoughMaterial)
        {
            Debug.Log("[BlackMarket] 재화가 부족합니다.");

            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(
                    new string[] { noMoneyDialogueKey });
            }

            return;
        }

        // 선택 모드를 시작할 수 있을 때만 결제한다.
        BeginRepairSelection();

        if (!isSelectingRobot)
            return;

        if (requiredGold > 0)
        {
            CurrencyManager.instance.SpendCurrency(
                CurrencyType.Gold, requiredGold);
        }

        if (requiredMaterial > 0)
        {
            CurrencyManager.instance.SpendCurrency(
                CurrencyType.Material, requiredMaterial);
        }

        Debug.Log(
            $"[BlackMarket] 구매 완료: 골드 -{requiredGold}, 재료 -{requiredMaterial}");
    }

    private bool TryGetItemPrice(out int price)
    {
        switch (KindOfItem)
        {
            case 0:
                price = 10000;
                return true;

            case 1:
                price = 7000;
                return true;

            case 2:
                price = 5000;
                return true;

            default:
                price = 0;
                return false;
        }
    }

    // ─────────────────────────────────────────────
    // 로봇 강화 선택
    // ─────────────────────────────────────────────

    public void BeginBlackMarketSelector()
    {
        if (BattleManager.instance == null ||
            !BattleManager.instance.BeginBlackMarketSelector(this))
        {
            Debug.LogWarning("[BlackMarket] 로봇 선택을 시작할 수 없습니다.");
        }
    }

    public void BeginRepairSelection()
    {
        if (PartyManager.instance == null ||
            BattleManager.instance == null)
        {
            Debug.LogWarning("[BlackMarket] 필요한 관리자를 찾을 수 없습니다.");
            return;
        }

        if (DialogueManager.instance != null)
            DialogueManager.instance.CloseDialogue();

        HideInteractionUI();

        if (!BattleManager.instance.BeginBlackMarketSelector(this))
        {
            Debug.LogWarning("[BlackMarket] 로봇 선택을 시작할 수 없습니다.");
            RestoreInteractionUI();
            return;
        }
        shopUI.gameObject.SetActive(false);
        shoperHideUI.gameObject.SetActive(true);
        isSelectingRobot = true;
    }
    public bool OnRobotSelected(Unit target)
    {
        if (target == null)
            return false;

        current = target;

        switch (KindOfItem)
        {
            case 0:
                current.accuracy += 5;
                Notion.Instance.NotionUpLocalization("BlackMarketAccuracyUp");
                break;

            case 1:
                current.maxHealth += 10;
                Notion.Instance.NotionUpLocalization("BlackMarketMaxHealthUp");
                break;

            case 2:
                current.attackPower += 5;
                Notion.Instance.NotionUpLocalization("BlackMarketAttackPowerUp");
                break;

            default:
                Debug.LogWarning($"[BlackMarket] 잘못된 상품 종류: {KindOfItem}");
                return false;
        }

        onRobotSelected?.Invoke(current);

        isSelectingRobot = false;
        RestoreInteractionUI();
        FocusOut();
        RecallRepairParty();
        Save.CommitSave();

        Debug.Log($"[BlackMarket] {current.Unitname} 강화 완료");
        return true;
    }

    // ─────────────────────────────────────────────
    // NPC 포커스 및 대화
    // ─────────────────────────────────────────────

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
                : new string[] { "BlackMarket" };

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
                    text = LocalizationManager.GetLocalizedString(exitChoiceText)
                }
            });
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

        if (shoperHideUI != null)
            shoperHideUI.SetActive(false);
    }
}
