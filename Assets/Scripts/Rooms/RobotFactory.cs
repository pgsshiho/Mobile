using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class RobotFactory : MonoBehaviour, IPointerClickHandler
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
    public string buyChoiceText = "구매한다";
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

    private Camera mainCamera;
    private FocusableObject currentFocusedTarget;

    public void OnEnable()
    {
        RefreshRobotVisibility();
        FocusManager.RequestFocusOut += FocusOut;
    }

    public void OnDisable()
    {
        FocusManager.RequestFocusOut -= FocusOut;
    }

    /// <summary>
    /// 이미 파티에 소속된 로봇은 공장에서 숨김 처리합니다.
    /// </summary>
    public void RefreshRobotVisibility()
    {
        if (Robots == null || PartyManager.instance == null || PartyManager.instance.partySlots == null) return;

        foreach (Unit robot in Robots)
        {
            if (robot == null) continue;

            bool isInParty = false;
            foreach (Unit party in PartyManager.instance.partySlots)
            {
                if (party == robot || (party != null && party.name.Replace("(Clone)", "").Trim() == robot.name.Replace("(Clone)", "").Trim()))
                {
                    isInParty = true;
                    break;
                }
            }

            if (isInParty)
            {
                robot.gameObject.SetActive(false);
            }
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
            seq.Join(rectTransform.DOAnchorPos(Vector2.zero, duration));
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
                : new string[] { "NPC_B_01" };

            DialogueManager.instance.StartDialogueWithChoices(
                keysToUse,
                new DialogueChoice[]
                {
                    new DialogueChoice
                    {
                        text = buyChoiceText,
                        onSelected = CreateBuyEvent()
                    },
                    new DialogueChoice
                    {
                        text = exitChoiceText
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
        evt.AddListener(TryPurchase);
        return evt;
    }

    /// <summary>
    /// 구매 조건 확인 (파티 정원 4명 검사 -> 재화 검사 -> Action으로 모든 FactoryAddUnit의 CanClick 활성화)
    /// </summary>
    private void TryPurchase()
    {
        // 1. 파티 슬롯(4명) 가득 찼는지 사전 검사
        if (PartyManager.instance != null && PartyManager.instance.IsPartyFull())
        {
            Debug.Log("[RobotFactory] 파티 슬롯(4명)이 가득 차서 더 이상 구매할 수 없습니다.");

            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(new string[] { partyFullDialogueKey });
            }
            return;
        }

        // 2. 비용 조건 확인 (CurrencyManager)
        bool hasGold     = CurrencyManager.instance == null || CurrencyManager.instance.HasEnough(CurrencyType.Gold, requiredGold);
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

    private void FocusOut()
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