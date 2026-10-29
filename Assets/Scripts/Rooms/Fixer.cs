using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class Fixer : MonoBehaviour, IPointerClickHandler
{
    [Header("로봇 목록")]
    public Unit[] Robots;
    public bool cantouchRobot = false;

    [Header("구매 비용 설정")]
    [Tooltip("구매에 필요한 골드")]
    public int requiredGold = 100;
    [Tooltip("구매에 필요한 재료")]
    public int requiredMaterial = 0;

    [Header("선택지 텍스트 설정")]
    [Tooltip("구매 선택지 텍스트")]
    public string buyChoiceText = "고친다";
    [Tooltip("나가기 선택지 텍스트")]
    public string exitChoiceText = "나간다";

    [Header("다이얼로그 키 설정")]
    [Tooltip("로컬라이제이션 테이블의 '돈이 더 필요해' 키")]
    public string noMoneyDialogueKey = "FACTORY_NO_MONEY";

    [Tooltip("로컬라이제이션 테이블의 '파티가 가득 찼어' 키 (4명 초과 시)")]
    public string partyFullDialogueKey = "FACTORY_PARTY_FULL";

    [Header("대화 키 목록")]
    public string[] Dialoguekey;

    [Header("기본 연출 설정")]
    public float duration = 1.0f;
    public float targetScaleMultiplier = 2.5f;
    [Tooltip("3D 월드 오브젝트 포커스 시 카메라와의 거리")]
    public float distanceFromCamera = 5f;
    public bool IsTweening { get; private set; } = false;
    [Header("Fixer 전용 로봇 소환 위치")]
    [Tooltip("수리할 로봇이 배치될 위치")]
    public Transform[] repairSpawnPositions;

    private Camera mainCamera;
    private FocusableObject currentFocusedTarget;

    [Header("로봇 선택 이벤트")]
    public UnitSelectedEvent onRobotSelected = new UnitSelectedEvent();

    public Unit current;

    [Header("Fixer 상호작용 중 숨길 UI")]
    public GameObject[] hideUIObjects;
    public GameObject HideForRobotUI;

    private bool[] originalUIStates;

    TurnOffUIForPrefabs TO;
    [Header("포커스 위치")]
    public Vector2 focusPosition = new Vector2(0f, -250f);
    private void Awake()
    {
        TO = FindAnyObjectByType<TurnOffUIForPrefabs>();
        hideUIObjects = TO.TurnOffs;

        if (hideUIObjects == null)
            return;

        originalUIStates = new bool[hideUIObjects.Length];

        for (int i = 0; i < hideUIObjects.Length; i++)
        {
            if (hideUIObjects[i] != null)
                originalUIStates[i] = hideUIObjects[i].activeSelf;
        }
    }

    // UI 숨기기
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

    // 기존 활성 상태로 복구
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

    public void OnEnable()
    {
        FocusManager.RequestFocusOut += OnRequestFocusOut;
    }

    private void OnRequestFocusOut()
    {
        // 로봇 선택 중일 때는 다이얼로그 닫힘 등으로 인한 외부 FocusOut 요청을 무시합니다.
        if (BattleManager.instance != null && BattleManager.instance.IsSelectingFixerTarget)
            return;

        FocusOut();
    }
    /// <summary>
    /// 현재 파티 로봇을 Fixer 전용 위치에 배치
    /// </summary>
    /// <summary>
    /// 현재 파티원을 Fixer 전용 위치에 배치하거나 소환합니다.
    /// </summary>
    public void PlacePartyAtRepairPositions()
    {
        if (PartyManager.instance == null)
        {
            Debug.LogWarning("[Fixer] PartyManager를 찾을 수 없습니다.");
            return;
        }

        Transform[] spawnPoints = null;
        Camera cam = mainCamera != null ? mainCamera : Camera.main;

        // 1. 설정된 repairSpawnPositions가 현재 카메라 시야 내에 유효한지 검사
        if (repairSpawnPositions != null && repairSpawnPositions.Length > 0 && repairSpawnPositions[0] != null)
        {
            if (cam != null)
            {
                Vector3 vp = cam.WorldToViewportPoint(repairSpawnPositions[0].position);
                // 화면 시야 범위(0~1) 내에 들어오고 카메라 앞쪽(z > 0)인지 확인
                if (vp.z > 0 && vp.x >= -0.2f && vp.x <= 1.2f && vp.y >= -0.2f && vp.y <= 1.2f)
                {
                    spawnPoints = repairSpawnPositions;
                }
                else
                {
                    Debug.LogWarning($"[Fixer] repairSpawnPositions가 카메라 시야 밖({repairSpawnPositions[0].position})에 있습니다. 카메라 정면 파티 위치로 자동 전환합니다.");
                }
            }
            else
            {
                spawnPoints = repairSpawnPositions;
            }
        }

        // 2. repairSpawnPositions가 카메라 밖 엉뚱한 곳에 있거나 설정되지 않은 경우:
        // 카메라가 정면으로 비추는 BattleManager.instance.Party 위치를 우선 사용
        if (spawnPoints == null)
        {
            if (BattleManager.instance != null &&
                BattleManager.instance.Party != null &&
                BattleManager.instance.Party.Length > 0 &&
                BattleManager.instance.Party[0] != null)
            {
                spawnPoints = BattleManager.instance.Party;
            }
            else
            {
                spawnPoints = repairSpawnPositions;
            }
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[Fixer] 수리 전용 소환 위치를 찾을 수 없습니다.");
            return;
        }
        HideForRobotUI.SetActive(true);
        PartyManager.instance.PlacePartyAtPositions(spawnPoints);
        Debug.Log("[Fixer] 수리실 파티원 배치 완료");
    }
    /// <summary>
    /// 수리 선택용으로 소환/배치된 파티 유닛을 회수합니다.
    /// </summary>
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

        Debug.Log("[Fixer] 수리용 파티 유닛 회수 완료");
        HideForRobotUI.SetActive(false);
    }
    public void OnDisable()
    {
        FocusManager.RequestFocusOut -= OnRequestFocusOut;

        if (BattleManager.instance != null && BattleManager.instance.IsSelectingFixerTarget)
        {
            BattleManager.instance.CancelFixerTargetSelection();
        }
        else
        {
            RecallRepairParty();
            RestoreInteractionUI();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject clickedObject = eventData.pointerPress;
        if (clickedObject != null)
        {
            FocusIn(gameObject);
        }
    }

    public void Fix()
    {
        if (BattleManager.instance == null ||
            !BattleManager.instance.BeginFixerSelector(this))
        {
            Debug.LogWarning(
                "[Fixer] 파티원을 소환하거나 선택할 수 없습니다.");
        }
    }
    /// <summary>
    /// 플레이어가 선택한 로봇을 전달받아 Inspector 이벤트를 실행합니다.
    /// </summary>
    public bool OnRobotSelected(Unit target)
    {
        if (target == null)
            return false;

        if (CurrencyManager.instance == null)
        {
            Debug.LogWarning("[Fixer] CurrencyManager가 없습니다.");
            return false;
        }

        bool hasGold =
            CurrencyManager.instance.HasEnough(
                CurrencyType.Gold, requiredGold);

        bool hasMaterial =
            requiredMaterial <= 0 ||
            CurrencyManager.instance.HasEnough(
                CurrencyType.Material, requiredMaterial);

        if (!hasGold || !hasMaterial)
        {
            Debug.Log("[Fixer] 수리 비용이 부족합니다.");

            if (DialogueManager.instance != null)
                DialogueManager.instance.StartDialogue(
                    new string[] { noMoneyDialogueKey });

            return false;
        }

        // 수리 비용 결제
        if (requiredGold > 0)
            CurrencyManager.instance.SpendCurrency(
                CurrencyType.Gold, requiredGold);

        if (requiredMaterial > 0)
            CurrencyManager.instance.SpendCurrency(
                CurrencyType.Material, requiredMaterial);

        // 선택한 로봇 최대체력까지 치유
        current = target;
        current.health = current.maxHealth;

        Notion.Instance.NotionUpLocalizationFormat("FIxerHeal",
            target.name
            );

        // Inspector 이벤트 실행
        onRobotSelected?.Invoke(current);
        Save.CommitSave();
        return true;
    }
    private void FocusIn(GameObject target)
    {
        if (IsTweening || target == null) return;

        FocusableObject focusable = target.GetComponent<FocusableObject>();
        if (focusable == null)
        {
            focusable = target.AddComponent<FocusableObject>();
        }

        if (focusable.IsFocused) return;

        if (currentFocusedTarget != null && currentFocusedTarget != focusable)
        {
            FocusOut();
        }

        currentFocusedTarget = focusable;
        focusable.IsFocused = true;
        IsTweening = true;

        RectTransform rectTransform = target.GetComponent<RectTransform>();
        Sequence seq = DOTween.Sequence();

        if (rectTransform != null) // UI
        {
            rectTransform.SetAsLastSibling();
            seq.Join(rectTransform.DOAnchorPos(focusPosition, duration));
            seq.Join(rectTransform.DOScale(focusable.OriginalScale * targetScaleMultiplier, duration));
        }
        else // 3D 월드 오브젝트
        {
            if (mainCamera == null) mainCamera = Camera.main;
            Vector3 targetWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, distanceFromCamera));
            seq.Join(target.transform.DOMove(targetWorldPos, duration));
            seq.Join(target.transform.DOScale(focusable.OriginalScale * targetScaleMultiplier, duration));
            seq.Join(target.transform.DORotateQuaternion(Quaternion.LookRotation(mainCamera.transform.forward), duration));
        }

        seq.SetEase(Ease.OutCubic)
           .OnComplete(() => IsTweening = false);

        // 다이얼로그 + 선택지(구매한다 / 나간다) 표시
        if (DialogueManager.instance != null)
        {
            string[] keysToUse = (Dialoguekey != null && Dialoguekey.Length > 0)
                ? Dialoguekey
                : new string[] { "Fixer" };

            DialogueManager.instance.StartDialogueWithChoices(
                keysToUse,
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
                    },
                }
            );
        }
    }

    /// <summary>
    /// "구매한다" 선택지에 연결할 UnityEvent를 생성합니다.
    /// </summary>
    private UnityEngine.Events.UnityEvent CreateBuyEvent()
    {
        var evt = new UnityEngine.Events.UnityEvent();
        evt.AddListener(BeginRepairSelection);
        return evt;
    }

    public void BeginRepairSelection()
    {
        if (PartyManager.instance == null ||
            BattleManager.instance == null)
        {
            Debug.LogWarning("[Fixer] 파티 관리자를 찾을 수 없습니다.");
            return;
        }

        if (DialogueManager.instance != null)
            DialogueManager.instance.CloseDialogue();

        HideInteractionUI();

        // 파티원 선택 모드 시작 및 배치
        if (!BattleManager.instance.BeginFixerSelector(this))
        {
            Debug.LogWarning("[Fixer] 수리할 파티원을 선택할 수 없습니다.");
            RestoreInteractionUI();
            return;
        }
    }
    /// <summary>
    /// 구매 조건 확인 (파티 정원 4명 검사 -> 재화 검사 -> Action으로 모든 FactoryAddUnit의 CanClick 활성화)
    /// </summary>
    private void TryPurchase()
    {
        
        // 2. 비용 조건 확인 (CurrencyManager)
        bool hasGold = CurrencyManager.instance == null || CurrencyManager.instance.HasEnough(CurrencyType.Gold, requiredGold);
        bool hasMaterial = CurrencyManager.instance == null || requiredMaterial <= 0 || CurrencyManager.instance.HasEnough(CurrencyType.Material, requiredMaterial);

        if (!hasGold || !hasMaterial)
        {
            // 재화 부족 실패: 다이얼로그 출력 후 종료
            Debug.Log("[RobotFactory] 재화 부족 — 구매 실패");

            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(new string[] { noMoneyDialogueKey });
            }
            return;
        }

        // 3. 재화 결제 차감
        if (CurrencyManager.instance != null)
        {
            if (requiredGold > 0)
                CurrencyManager.instance.SpendCurrency(CurrencyType.Gold, requiredGold);
            if (requiredMaterial > 0)
                CurrencyManager.instance.SpendCurrency(CurrencyType.Material, requiredMaterial);
        }

        Debug.Log($"<color=cyan>[RobotFactory]</color> 구매 완료! (골드 -{requiredGold}, 재료 -{requiredMaterial}) 원하는 로봇을 클릭/선택하여 파티에 추가하세요.");

        // 4. Action을 통해 씬 내의 모든 FactoryAddUnit의 CanClick을 true로 활성화 (로봇 선택 가능)
        FactoryAddUnit.UnlockAll();

        // 5. 다이얼로그 종료
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.CloseDialogue();
        }
    }

    public void FocusOut()
    {
        if (IsTweening || currentFocusedTarget == null) return;

        IsTweening = true;
        FocusableObject focusable = currentFocusedTarget;
        GameObject target = focusable.gameObject;
        RectTransform rectTransform = target.GetComponent<RectTransform>();

        Sequence seq = DOTween.Sequence();

        if (rectTransform != null)
        {
            seq.Join(rectTransform.DOAnchorPos(focusable.OriginalPosition, duration));
            seq.Join(rectTransform.DOScale(focusable.OriginalScale, duration));
            seq.SetEase(Ease.OutCubic)
               .OnComplete(() =>
               {
                   rectTransform.SetSiblingIndex(focusable.OriginalSiblingIndex);
                   focusable.IsFocused = false;
                   currentFocusedTarget = null;
                   IsTweening = false;
               });
        }
        else
        {
            seq.Join(target.transform.DOMove(focusable.OriginalPosition, duration));
            seq.Join(target.transform.DOScale(focusable.OriginalScale, duration));
            seq.Join(target.transform.DORotateQuaternion(focusable.OriginalRotation, duration));
            seq.SetEase(Ease.OutCubic)
               .OnComplete(() =>
               {
                   focusable.IsFocused = false;
                   currentFocusedTarget = null;
                   IsTweening = false;
               });
        }
    }
}
