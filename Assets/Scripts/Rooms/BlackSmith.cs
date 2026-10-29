
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class BlackSmithUnitSelectedEvent : UnityEvent<Unit> { }

public class BlackSmith : MonoBehaviour
{
    [Header("강화 비용")]
    public int requiredGold = 100;
    public int requiredMaterial = 0;

    [Header("대화 키")]
    public string Upgrade = "BlacksmithAccept";
    public string Leaving = "BlacksmithLeave";
    public string noMoneyDialogueKey = "BLACKMARKET_NO_MONEY";
    public string[] Dialoguekey;

    [Header("기본 연출 설정")]
    public float duration = 1f;
    public float targetScaleMultiplier = 2.5f;
    public float distanceFromCamera = 5f;
    public Vector2 focusPosition = new Vector2(0f, -250f);

    public bool IsTweening { get; private set; }
    public bool isSelectingRobot = false;

    [Header("로봇 배치 위치")]
    public Transform[] repairSpawnPositions;

    [Header("대장간 UI")]
    public GameObject BlackSmithUI;
    public GameObject HIdeUI;

    [Header("상호작용 중 숨길 UI")]
    public GameObject[] hideUIObjects;

    [Header("로봇 강화")]
    public Unit current;
    public int KindOfItem = 0;
    public BlackSmithUnitSelectedEvent onRobotSelected =
        new BlackSmithUnitSelectedEvent();

    private Camera mainCamera;
    private FocusableObject currentFocusedTarget;
    private bool[] originalUIStates;
    private TurnOffUIForPrefabs uiPrefabManager;

    private void Awake()
    {
        mainCamera = Camera.main;

        uiPrefabManager = FindAnyObjectByType<TurnOffUIForPrefabs>();

        if (uiPrefabManager != null)
            hideUIObjects = uiPrefabManager.TurnOffs;

        CacheInteractionUIStates();
    }

    private void CacheInteractionUIStates()
    {
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

    // ─────────────────────────────────────────
    // UI 숨김 및 복구
    // ─────────────────────────────────────────

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

        int count = Mathf.Min(
            hideUIObjects.Length, originalUIStates.Length);

        for (int i = 0; i < count; i++)
        {
            if (hideUIObjects[i] != null)
                hideUIObjects[i].SetActive(originalUIStates[i]);
        }
    }

    private void OnRequestFocusOut()
    {
        if (isSelectingRobot)
            return;

        if (BattleManager.instance != null &&
            BattleManager.instance.IsSelectingFixerTarget)
            return;

        FocusOut();
    }

    // ─────────────────────────────────────────
    // 파티 로봇 배치
    // ─────────────────────────────────────────

    public void PlacePartyAtBlackSmithPositions()
    {
        if (PartyManager.instance == null)
        {
            Debug.LogWarning(
                "[BlackSmith] PartyManager를 찾을 수 없습니다.");
            return;
        }

        Transform[] spawnPoints = GetAvailableSpawnPositions();

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning(
                "[BlackSmith] 로봇 배치 위치를 찾을 수 없습니다.");
            return;
        }

        PartyManager.instance.PlacePartyAtPositions(spawnPoints);

        Debug.Log("[BlackSmith] 파티 로봇 배치 완료");
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
                cam.WorldToViewportPoint(
                    repairSpawnPositions[0].position);

            bool isVisible =
                viewportPosition.z > 0 &&
                viewportPosition.x >= -0.2f &&
                viewportPosition.x <= 1.2f &&
                viewportPosition.y >= -0.2f &&
                viewportPosition.y <= 1.2f;

            if (isVisible)
                return repairSpawnPositions;

            Debug.LogWarning(
                "[BlackSmith] 배치 위치가 카메라 시야 밖에 있습니다.");
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

    // ─────────────────────────────────────────
    // 대장간 열기 및 닫기
    // ─────────────────────────────────────────

    public void OnPointerClick()
    {
        FocusIn(gameObject);
    }

    public void TurnOntheBlackSmithUI()
    {
        if (BlackSmithUI == null)
        {
            Debug.LogWarning(
                "[BlackSmith] BlackSmithUI가 연결되지 않았습니다.");
            return;
        }

        BlackSmithUI.SetActive(true);

        RectTransform rect =
            BlackSmithUI.GetComponent<RectTransform>();

        if (rect != null)
            rect.SetAsLastSibling();
    }

    public void CloseBlackSmithUI()
    {
        if (BlackSmithUI != null)
            BlackSmithUI.SetActive(false);

        if (HIdeUI != null)
            HIdeUI.SetActive(false);
    }

    public void leave()
    {
        CloseBlackSmithUI();

        if (currentFocusedTarget != null)
            FocusOut();
    }

    // ─────────────────────────────────────────
    // 강화 선택 시작
    // ─────────────────────────────────────────


    public void BeginBlackSmithSelection()
    {
        if (PartyManager.instance == null ||
            BattleManager.instance == null)
        {
            Debug.LogWarning(
                "[BlackSmith] 필요한 관리자를 찾을 수 없습니다.");
            return;
        }

        if (isSelectingRobot)
            return;

        if (DialogueManager.instance != null)
            DialogueManager.instance.CloseDialogue();

        if (!BattleManager.instance.BeginBlackSmithSelector(this))
        {
            Debug.LogWarning(
                "[BlackSmith] 로봇 선택을 시작할 수 없습니다.");
            return;
        }

        HideInteractionUI();

        if (BlackSmithUI != null)
            BlackSmithUI.SetActive(false);

        if (HIdeUI != null)
            HIdeUI.SetActive(true);

        isSelectingRobot = true;
    }

    // ─────────────────────────────────────────
    // 강화 버튼 및 비용 처리
    // ─────────────────────────────────────────

    public void buttonUpgrade(int kindUpgrade)
    {
        KindOfItem = kindUpgrade;

        switch (KindOfItem)
        {
            case 0:
                requiredGold = 5000;
                requiredMaterial = 15;
                break;

            case 1:
                requiredGold = 5000;
                requiredMaterial = 20;
                break;

            case 2:
                requiredGold = 5000;
                requiredMaterial = 10;
                break;
            case 3:
                requiredGold = 5000;
                requiredMaterial = 5;
                break;
            default:
                Debug.LogWarning(
                    $"[BlackSmith] 잘못된 강화 종류: {KindOfItem}");
                return;
        }

        TryPurchase();
    }

    private void TryPurchase()
    {
        if (CurrencyManager.instance == null)
        {
            Debug.LogWarning(
                "[BlackSmith] CurrencyManager를 찾을 수 없습니다.");
            return;
        }

        bool hasEnoughGold =
            CurrencyManager.instance.HasEnough(
                CurrencyType.Gold, requiredGold);

        bool hasEnoughMaterial =
            requiredMaterial <= 0 ||
            CurrencyManager.instance.HasEnough(
                CurrencyType.Material, requiredMaterial);

        if (!hasEnoughGold || !hasEnoughMaterial)
        {
            Debug.Log("[BlackSmith] 재화가 부족합니다.");

            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(
                    new string[] { noMoneyDialogueKey });
            }

            return;
        }

        // 선택 모드가 정상적으로 시작된 경우에만 결제한다.
        BeginBlackSmithSelection();

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
            $"[BlackSmith] 강화 선택 시작: 골드 -{requiredGold}, " +
            $"재료 -{requiredMaterial}");
    }

    // ─────────────────────────────────────────
    // 로봇 선택 및 강화 적용
    // ─────────────────────────────────────────

    public bool OnRobotSelected(Unit target)
    {
        if (target == null || !isSelectingRobot)
            return false;

        current = target;

        switch (KindOfItem)
        {
            case 0:
                // 공격력 강화 및 공격 레벨 상승
                if (current.attackLevel >= current.maxAttackLevel)
                {
                    Notion.Instance.NotionUpLocalization(
                        "BlackSmithWillBroken");

                    // 강화 불가이므로 환불
                    CurrencyManager.instance.AddCurrency(
                        CurrencyType.Gold, requiredGold);

                    CurrencyManager.instance.AddCurrency(
                        CurrencyType.Material, requiredMaterial);

                    isSelectingRobot = false;
                    RestoreInteractionUI();

                    if (HIdeUI != null)
                        HIdeUI.SetActive(false);

                    return false;
                }

                current.attackPower += 5;
                current.attackLevel += 1;

                Notion.Instance.NotionUpLocalizationFormat(
                    "BlackSmithAttackUp", current.name);
                break;

            case 1:
                if (current.AccurancyLevel >= current.maxAccurancyLevel)
                {
                    Notion.Instance.NotionUpLocalization(
                        "BlackSmithWillBroken");

                    // 강화 불가이므로 환불
                    CurrencyManager.instance.AddCurrency(
                        CurrencyType.Gold, requiredGold);

                    CurrencyManager.instance.AddCurrency(
                        CurrencyType.Material, requiredMaterial);

                    isSelectingRobot = false;
                    RestoreInteractionUI();

                    if (HIdeUI != null)
                        HIdeUI.SetActive(false);

                    return false;
                }
                current.accuracy += 5;
                current.AccurancyLevel += 1;
                Notion.Instance.NotionUpLocalizationFormat(
                    "BlackSmithAccurancyUp", current.name);
                break;

            case 2:
                if (current.defenseLevel >= current.maxDefenseLevel)
                {
                    Notion.Instance.NotionUpLocalization(
                        "BlackSmithWillBroken");

                    // 강화 불가이므로 환불
                    CurrencyManager.instance.AddCurrency(
                        CurrencyType.Gold, requiredGold);

                    CurrencyManager.instance.AddCurrency(
                        CurrencyType.Material, requiredMaterial);

                    isSelectingRobot = false;
                    RestoreInteractionUI();

                    if (HIdeUI != null)
                        HIdeUI.SetActive(false);

                    return false;
                }
                // 공격력 강화
                current.defensePower += 5;
                current.defenseLevel += 1;
                Notion.Instance.NotionUpLocalizationFormat(
                    "BlackSmithDefenseUp", current.name);
                break;

            case 3:
                if (current.MaxHPLevel >= current.maxMaxHPLevel)
                {
                    Notion.Instance.NotionUpLocalization(
                        "BlackSmithWillBroken");

                    // 강화 불가이므로 환불
                    CurrencyManager.instance.AddCurrency(
                        CurrencyType.Gold, requiredGold);

                    CurrencyManager.instance.AddCurrency(
                        CurrencyType.Material, requiredMaterial);

                    isSelectingRobot = false;
                    RestoreInteractionUI();

                    if (HIdeUI != null)
                        HIdeUI.SetActive(false);

                    return false;
                }
                // 최대 체력 강화
                current.maxHealth += 5;
                current.MaxHPLevel += 1;
                Notion.Instance.NotionUpLocalizationFormat(
                    "BlackSmithMaxHealthUp", current.name);
                break;

            default:
                Debug.LogWarning(
                    $"[BlackSmith] 잘못된 강화 종류: {KindOfItem}");
                return false;
        }

        onRobotSelected?.Invoke(current);

        isSelectingRobot = false;
        RestoreInteractionUI();

        if (HIdeUI != null)
            HIdeUI.SetActive(false);
        
        FocusOut();
        RecallRepairParty();
        Save.CommitSave();

        Debug.Log($"[BlackSmith] {current.Unitname} 강화 완료");

        return true;
    }

    // ─────────────────────────────────────────
    // NPC 포커스 및 대화
    // ─────────────────────────────────────────

    private void FocusIn(GameObject target)
    {
        if (IsTweening || target == null)
            return;

        FocusableObject focusable =
            target.GetComponent<FocusableObject>();

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

        RectTransform rectTransform =
            target.GetComponent<RectTransform>();

        Sequence sequence = DOTween.Sequence();

        if (rectTransform != null)
        {
            rectTransform.SetAsLastSibling();

            sequence.Join(
                rectTransform.DOAnchorPos(focusPosition, duration));

            sequence.Join(
                rectTransform.DOScale(
                    focusable.OriginalScale *
                    targetScaleMultiplier, duration));
        }
        else
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
            {
                Vector3 targetPosition =
                    mainCamera.ViewportToWorldPoint(
                        new Vector3(
                            0.5f, 0.5f, distanceFromCamera));

                sequence.Join(
                    target.transform.DOMove(
                        targetPosition, duration));

                sequence.Join(
                    target.transform.DOScale(
                        focusable.OriginalScale *
                        targetScaleMultiplier, duration));

                sequence.Join(
                    target.transform.DORotateQuaternion(
                        Quaternion.LookRotation(
                            mainCamera.transform.forward), duration));
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
                : new string[] { "BlackSmith" };

        DialogueManager.instance.StartDialogueWithChoices(
            dialogueKeys,
            new DialogueChoice[]
            {
                new DialogueChoice
                {
                    text = LocalizationManager.GetLocalizedString(
                        Upgrade),
                    onSelected = CreateBuyEvent()
                },
                new DialogueChoice
                {
                    text = LocalizationManager.GetLocalizedString(
                        Leaving),
                    onSelected = Leavings()
                }
            });
    }

    private UnityEvent CreateBuyEvent()
    {
        UnityEvent buyEvent = new UnityEvent();
        buyEvent.AddListener(TurnOntheBlackSmithUI);
        return buyEvent;
    }

    private UnityEvent Leavings()
    {
        UnityEvent leavingEvent = new UnityEvent();
        leavingEvent.AddListener(leave);
        return leavingEvent;
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

        Debug.Log("[BlackSmith] 수리용 파티 유닛 회수 완료");
    }
    public void FocusOut()
    {
        if (IsTweening || currentFocusedTarget == null)
            return;

        IsTweening = true;

        FocusableObject focusable = currentFocusedTarget;
        GameObject target = focusable.gameObject;

        RectTransform rectTransform =
            target.GetComponent<RectTransform>();

        Sequence sequence = DOTween.Sequence();

        if (rectTransform != null)
        {
            sequence.Join(
                rectTransform.DOAnchorPos(
                    focusable.OriginalPosition, duration));

            sequence.Join(
                rectTransform.DOScale(
                    focusable.OriginalScale, duration));

            sequence.SetEase(Ease.OutCubic).OnComplete(() =>
            {
                rectTransform.SetSiblingIndex(
                    focusable.OriginalSiblingIndex);

                FinishFocusOut(focusable);
            });
        }
        else
        {
            sequence.Join(
                target.transform.DOMove(
                    focusable.OriginalPosition, duration));

            sequence.Join(
                target.transform.DOScale(
                    focusable.OriginalScale, duration));

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

    public void ToggleFocus(GameObject target)
    {
        if (target == null)
            return;

        FocusableObject focusable =
            target.GetComponent<FocusableObject>();

        if (focusable != null && focusable.IsFocused)
            FocusOut();
        else
            FocusIn(target);
    }
}
