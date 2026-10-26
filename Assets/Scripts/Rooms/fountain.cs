using DG.Tweening;
using TMPro;
using UnityEngine;

public class fountain : MonoBehaviour
{
    [Header("대화 설정")]
    public string[] FindDailogueKey;
    public string FindChoiceText = "FFindChoice";
    public string exitChoiceText = "FExitChoice";

    [Header("강화 관련 키")]
    public string SpeedChoiceText = "SpeedChoice";
    public string AttackChoiceText = "AttackChoice";
    public string maxHpChoiceText = "MaxHpChoice";

    [Header("파티 배치")]
    public Transform[] repairSpawnPositions;
    public Camera mainCamera;

    [Header("상태")]
    public bool isFind = false;
    public Unit current;
    public TextMeshProUGUI Uptext;

    private bool isSelecting = false;

    /// <summary>
    /// 대화창이 닫히더라도 로봇 선택 중에는
    /// 기존 포커스 해제 요청을 무시한다.
    /// </summary>
    private void OnRequestFocusOut()
    {
        if (BattleManager.instance != null &&
            BattleManager.instance.IsSelectingFountainTarget)
        {
            return;
        }
    }

    public void TextUp()
    {
        if (Uptext == null)
        {
            Debug.LogWarning("[Fountain] Uptext가 연결되지 않았습니다.");
            return;
        }

        Uptext.gameObject.SetActive(true);

        Uptext.transform
            .DOBlendableLocalMoveBy(new Vector3(0, 50, 0), 1.0f)
            .SetEase(Ease.OutQuad);

        Uptext.DOFade(1f, 0f);

        Uptext.DOFade(0f, 1.0f)
            .OnComplete(() => Uptext.gameObject.SetActive(false));
    }

    /// <summary>
    /// 로봇 선택을 취소한다.
    /// 분수대는 아직 사용하지 않은 상태로 유지한다.
    /// </summary>
    public void CancelRepairSelection()
    {
        isSelecting = false;
        current = null;

        RecallRepairParty();
    }

    /// <summary>
    /// 분수대와 상호작용한다.
    /// 사용 전이면 선택지를, 사용 후면 재사용 불가 대사를 출력한다.
    /// </summary>
    public void ClickruninBuilding()
    {
        if (isFind)
        {
            AlreadyFind();
            return;
        }

        if (isSelecting)
            return;

        if (DialogueManager.instance == null)
        {
            Debug.LogWarning(
                "[Fountain] DialogueManager를 찾을 수 없습니다."
            );
            return;
        }

        string[] keysToUse =
            FindDailogueKey != null &&
            FindDailogueKey.Length > 0
                ? FindDailogueKey
                : new string[] { "Fountain" };

        DialogueManager.instance.StartDialogueWithChoices(
            keysToUse,
            new DialogueChoice[]
            {
                new DialogueChoice
                {
                    text = LocalizationManager.GetLocalizedString(
                        FindChoiceText
                    ),
                    onSelected = CreateFindEvent()
                },
                new DialogueChoice
                {
                    text = LocalizationManager.GetLocalizedString(
                        exitChoiceText
                    )
                }
            }
        );
    }

    /// <summary>
    /// 로봇 청소 선택지에 실행 이벤트를 연결한다.
    /// </summary>
    private UnityEngine.Events.UnityEvent CreateFindEvent()
    {
        var evt = new UnityEngine.Events.UnityEvent();
        evt.AddListener(BeginRepairSelection);
        return evt;
    }

    /// <summary>
    /// 로봇 청소를 시작한다.
    /// 아직 사용 완료 상태로 처리하지 않는다.
    /// </summary>
    public void BeginRepairSelection()
    {
        if (isFind || isSelecting)
            return;

        if (PartyManager.instance == null)
        {
            Debug.LogWarning(
                "[Fountain] PartyManager를 찾을 수 없습니다."
            );
            return;
        }

        if (BattleManager.instance == null)
        {
            Debug.LogWarning(
                "[Fountain] BattleManager를 찾을 수 없습니다."
            );
            return;
        }

        if (DialogueManager.instance != null)
            DialogueManager.instance.CloseDialogue();

        // 실제 로봇 선택은 BattleManager가 담당한다.
        fountains();
    }

    /// <summary>
    /// BattleManager의 기존 분수대 로봇 선택 시스템을 시작한다.
    /// </summary>
    public void fountains()
    {
        if (isFind || isSelecting)
            return;

        if (BattleManager.instance == null ||
            !BattleManager.instance.BeginfountainSelector(this))
        {
            Debug.LogWarning(
                "[Fountain] 파티원을 소환하거나 선택할 수 없습니다."
            );
            return;
        }

        isSelecting = true;
    }

    /// <summary>
    /// 선택한 로봇에 무작위 능력치 강화를 적용한다.
    /// 성공한 경우에만 분수대 사용을 완료한다.
    /// </summary>
    public bool OnRobotSelected(Unit target)
    {
        if (isFind || target == null)
            return false;

        int statUpRandom = Random.Range(0, 3);
        string resultText = "";

        switch (statUpRandom)
        {
            case 0:
                target.speed += 2;

                resultText =
                    LocalizationManager.GetLocalizedString("Speed") + " +2";

                Debug.Log($"[Fountain] {target.name}: 속도 +2");
                break;

            case 1:
                target.attackPower += 5;

                resultText =
                    LocalizationManager.GetLocalizedString("Attack") + " +5";

                Debug.Log($"[Fountain] {target.name}: 공격력 +5");
                break;

            case 2:
                target.maxHealth += 5;

                resultText =
                    LocalizationManager.GetLocalizedString("MaxHp") + " +5";

                Debug.Log($"[Fountain] {target.name}: 최대 체력 +5");
                break;
        }

        current = target;

        // 강화 결과 표시
        if (Uptext != null)
        {
            Uptext.text = resultText;
            TextUp();
        }

        // 강화가 적용된 뒤에만 사용 완료 처리
        isFind = true;
        isSelecting = false;

        RecallRepairParty();
        Save.CommitSave();
        Debug.Log("[Fountain] 분수대 사용 완료");
        return true;
    }
    public void PlacePartyAtfountainPositions()
    {
        if (PartyManager.instance == null)
        {
            Debug.LogWarning(
                "[Fountain] PartyManager를 찾을 수 없습니다."
            );
            return;
        }

        Transform[] spawnPoints = null;
        Camera cam = mainCamera != null
            ? mainCamera
            : Camera.main;

        if (repairSpawnPositions != null &&
            repairSpawnPositions.Length > 0 &&
            repairSpawnPositions[0] != null)
        {
            if (cam != null)
            {
                Vector3 vp = cam.WorldToViewportPoint(
                    repairSpawnPositions[0].position
                );

                if (vp.z > 0 &&
                    vp.x >= -0.2f && vp.x <= 1.2f &&
                    vp.y >= -0.2f && vp.y <= 1.2f)
                {
                    spawnPoints = repairSpawnPositions;
                }
            }
            else
            {
                spawnPoints = repairSpawnPositions;
            }
        }

        // 분수대 위치가 유효하지 않으면 전투 파티 위치를 사용한다.
        if (spawnPoints == null &&
            BattleManager.instance != null &&
            BattleManager.instance.Party != null &&
            BattleManager.instance.Party.Length > 0 &&
            BattleManager.instance.Party[0] != null)
        {
            spawnPoints = BattleManager.instance.Party;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning(
                "[Fountain] 파티 배치 위치를 찾을 수 없습니다."
            );
            return;
        }

        PartyManager.instance.PlacePartyAtPositions(spawnPoints);

        Debug.Log("[Fountain] 파티원 배치 완료");
    }

    /// <summary>
    /// 선택 UI에서 사용한 파티 유닛을 회수한다.
    /// </summary>
    public void RecallRepairParty()
    {
        if (PartyManager.instance != null)
            PartyManager.instance.RecallParty();

        PartymenuSpawn[] spawners =
            FindObjectsByType<PartymenuSpawn>();

        foreach (PartymenuSpawn spawner in spawners)
        {
            if (spawner != null)
                spawner.RecallSpawnedUnits();
        }

        Debug.Log("[Fountain] 파티 유닛 회수 완료");
    }

    /// <summary>
    /// 이미 사용한 분수대와 상호작용했을 때 출력한다.
    /// </summary>
    public void AlreadyFind()
    {
        if (DialogueManager.instance == null)
        {
            Debug.LogWarning(
                "[Fountain] DialogueManager를 찾을 수 없습니다."
            );
            return;
        }
        
        DialogueManager.instance.StartDialogue(
            new string[] { "AlreadyFindfountain" }
        );
    }
}