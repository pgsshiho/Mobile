using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class RobotFactory : MonoBehaviour, IPointerClickHandler
{
    public Unit[] Robots;
    public bool cantouchRobot = false;

    [Header("구매 비용 설정")]
    [Tooltip("구매에 필요한 골드")]
    public int requiredGold = 100;
    [Tooltip("구매에 필요한 재료")]
    public int requiredMaterial = 0;

    [Header("구매 실패 다이얼로그 키 (돈 부족)")]
    [Tooltip("로컬라이제이션 테이블의 '돈이 더 필요해' 키")]
    public string noMoneyDialogueKey = "FACTORY_NO_MONEY";

    [Header("기본 연출 설정")]
    public float duration = 1.0f;
    public float targetScaleMultiplier = 2.5f;
    [Tooltip("3D 월드 오브젝트 포커스 시 카메라와의 거리")]
    public float distanceFromCamera = 5f;
    public bool IsTweening { get; private set; } = false;
    public string[] Dialoguekey;

    // 현재 포커스된 로봇 (구매 선택 시 이 유닛을 추가)
    private Unit focusedRobot;

    private Camera mainCamera;
    private FocusableObject currentFocusedTarget;

    public void OnEnable()
    {
        foreach (Unit robot in Robots)
        {
            foreach (Unit party in PartyManager.instance.partySlots)
            {
                if (party == robot)
                {
                    robot.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject clickedObject = eventData.pointerPress;
        if (clickedObject != null)
        {
            // 클릭된 오브젝트에 해당하는 로봇을 찾아 포커스
            Unit clickedRobot = clickedObject.GetComponent<Unit>();
            if (clickedRobot != null)
            {
                focusedRobot = clickedRobot;
            }
            else if (Robots.Length > 0)
            {
                focusedRobot = Robots[0];
            }

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

        // 다이얼로그 + 선택지 표시
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(Dialoguekey);
            DialogueManager.instance.StartDialogueWithChoices(
                new string[] { "NPC_B_01" },
                new DialogueChoice[]
                {
                    new DialogueChoice
                    {
                        text = "구매한다",
                        onSelected = CreateBuyEvent()
                    },
                    new DialogueChoice
                    {
                        text = "나간다"
                        // onSelected 없으면 대화만 닫힘
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
    /// 구매 조건 확인 → 부족하면 "돈이 더 필요해" 다이얼로그, 충분하면 CanClick 갱신 + 파티 추가
    /// </summary>
    private void TryPurchase()
    {
        // 1. 비용 조건 확인 (CurrencyManager)
        bool hasGold     = CurrencyManager.instance == null || CurrencyManager.instance.HasEnough(CurrencyType.Gold, requiredGold);
        bool hasMaterial = CurrencyManager.instance == null || requiredMaterial <= 0 || CurrencyManager.instance.HasEnough(CurrencyType.Material, requiredMaterial);

        if (!hasGold || !hasMaterial)
        {
            // ── 실패: "돈이 더 필요해" 다이얼로그 표시 후 닫기 ──
            Debug.Log("[RobotFactory] 재화 부족 — 구매 실패");

            if (DialogueManager.instance != null)
            {
                // 기존 다이얼로그를 닫고 실패 메시지만 출력 후 자동 종료
                DialogueManager.instance.StartDialogue(new string[] { noMoneyDialogueKey });
            }
            return;
        }

        // ── 성공: 재화 차감 ──
        if (CurrencyManager.instance != null)
        {
            if (requiredGold > 0)
                CurrencyManager.instance.SpendCurrency(CurrencyType.Gold, requiredGold);
            if (requiredMaterial > 0)
                CurrencyManager.instance.SpendCurrency(CurrencyType.Material, requiredMaterial);
        }

        Debug.Log($"[RobotFactory] 구매 성공! (골드 -{requiredGold}, 재료 -{requiredMaterial})");

        // ── 씬 내 모든 FactoryAddUnit의 CanClick = true 갱신 + 파티 추가 ──
        Unit targetUnit = focusedRobot;
        FactoryAddUnit.UnlockAndAdd(targetUnit);

        // 다이얼로그 닫기
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