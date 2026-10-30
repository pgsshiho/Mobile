using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [Header("Positions")]
    public Transform[] Party;
    public Transform[] Enemy;

    [Header("Battle")]
    public bool isBattle = false;

    [Header("Main UI")]
    public GameObject battleUI;

    [Header("Turn UI")]
    public TMP_Text turnText;

    [Header("Skill Buttons")]
    public GameObject[] skillButtons;
    public GameObject BattleButtonsFrame;
    [Header("Skill EX UI")]
    [Tooltip("각 스킬 버튼을 길게 눌렀을 때 표시할 UI")]
    public GameObject[] battleUIEX;

    [Header("Formation Move Button")]
    [Tooltip("누른 뒤 아군을 선택하면 해당 아군의 열로 교대 이동하는 버튼")]
    public Button formationMoveButton;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;
    public AudioClip battleBgm;

    private Fixer fixerTargetSelector;
    private fountain fountainTargetSelector;
    private BackpackUIManager itemTargetSelector;
    private BlackMarket BlackMarketTargetSelector;
    private BlackSmith BlackSmithTargetSelector;
    private int lastTargetHandledFrame = -1;

    public bool IsSelectingFixerTarget =>
        fixerTargetSelector != null;

    public bool IsSelectingFountainTarget =>
        fountainTargetSelector != null;

    public bool IsSelectingItemTarget =>
        itemTargetSelector != null;
    public bool IsSelectingBlackMaket => 
        BlackMarketTargetSelector != null;

    public bool IsSelectingBlackSmith =>
        BlackSmithTargetSelector != null;
    public bool IsSelectingAnyTarget =>
        IsSelectingItemTarget ||
        IsSelectingFixerTarget ||
        IsSelectingFountainTarget ||
        IsSelectingBlackMaket ||
        IsSelectingBlackSmith ||
        Time.frameCount == lastTargetHandledFrame;
    private struct CachedSkillButton
    {
        public GameObject root;
        public Button button;
        public TMP_Text label;
        public Image iconImage;
    }
    [Header("스킬이름")]
    public string Skillname = "";
    public TextMeshProUGUI Skillnames;

    private CachedSkillButton[] cachedButtons;

    private TMP_Text[] battleUIEXTexts;

    private bool isSelectingFormationMove;
    // 전투 규칙은 UI/씬 참조 없이 별도 객체에 둔다.
    private readonly BattleTargetingRules targetingRules =
        new BattleTargetingRules();
    private FormationMoveRules formationMoveRules;

    private readonly BattlePositioningService positioning =
        new BattlePositioningService();


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        formationMoveRules = new FormationMoveRules(targetingRules);

        CacheSkillButtons();
        CacheBattleUIEX();
        CacheFormationMoveButtons();

        HideBattleUIEX();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsSelectingItemTarget)
            {
                itemTargetSelector.CancelItemUse();
            }
            else if (IsSelectingFixerTarget)
            {
                CancelFixerTargetSelection();
            }
            else if (IsSelectingFountainTarget)
            {
                CancelFountainTargetSelection();
            }
        }
    }


    // =========================================================
    // Skill Button Cache
    // =========================================================

    private void CacheSkillButtons()
    {
        if (skillButtons == null)
        {
            cachedButtons = new CachedSkillButton[0];
            return;
        }

        cachedButtons = new CachedSkillButton[skillButtons.Length];

        for (int i = 0; i < skillButtons.Length; i++)
        {
            GameObject btn = skillButtons[i];

            if (btn == null)
                continue;

            Button button = btn.GetComponent<Button>();

            // 중요:
            // 여기서는 Button.onClick으로 SelectSkill을 연결하지 않는다.
            // SkillButtonLongPress가 짧은 클릭/길게 누르기를 직접 처리한다.

            cachedButtons[i] = new CachedSkillButton
            {
                root = btn,
                button = button,
                label = btn.GetComponentInChildren<TMP_Text>(true),
                iconImage = btn.GetComponent<Image>()
            };

            SkillButtonLongPress longPress =
                btn.GetComponent<SkillButtonLongPress>();

            if (longPress == null)
            {
                longPress =
                    btn.AddComponent<SkillButtonLongPress>();
            }

            longPress.Initialize(i);
        }
    }


    // =========================================================
    // Battle EX UI Cache
    // =========================================================

    private void CacheBattleUIEX()
    {
        if (battleUIEX == null)
        {
            battleUIEXTexts = new TMP_Text[0];
            return;
        }

        battleUIEXTexts = new TMP_Text[battleUIEX.Length];

        for (int i = 0; i < battleUIEX.Length; i++)
        {
            if (battleUIEX[i] == null)
                continue;

            battleUIEXTexts[i] =
                battleUIEX[i].GetComponentInChildren<TMP_Text>(true);
        }
    }


    // =========================================================
    // Battle EX UI
    // =========================================================

    public async void ShowBattleUIEX(int index)
    {
        HideBattleUIEX();

        if (battleUIEX == null)
            return;

        if (battleUIEXTexts == null)
            return;

        if (index < 0 || index >= battleUIEX.Length)
            return;

        if (battleUIEX[index] == null)
            return;

        if (TurnManager.instance == null)
            return;

        PlayerUnit player =
            TurnManager.instance.currentUnit as PlayerUnit;

        if (player == null)
            return;

        if (player.skills == null)
            return;

        if (index < 0 || index >= player.skills.Count)
            return;

        SkillData skill = player.skills[index];

        if (skill == null)
            return;

        // EX UI 켜기
        battleUIEX[index].SetActive(true);

        TMP_Text text = battleUIEXTexts[index];

        if (text == null)
        {
            Debug.LogWarning(
                $"[BattleManager] battleUIEX[{index}] 안에 TMP_Text가 없습니다."
            );

            return;
        }

        // -----------------------------------------------------
        // SkillData.description을 Localization Key로 사용
        // -----------------------------------------------------

        if (string.IsNullOrEmpty(skill.description))
        {
            text.text = "";
            return;
        }

        try
        {
            var localizedDescription =
                await LocalizationSettings.StringDatabase
                    .GetLocalizedStringAsync(
                        "En",
                        skill.description
                    )
                    .Task;

            text.text = localizedDescription;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(
                $"[BattleManager] Localization 실패 : {skill.description}\n{e}"
            );

            // Localization 실패 시 원본 문자열 표시
            text.text = skill.description;
        }
    }


    public void HideBattleUIEX()
    {
        if (battleUIEX == null)
            return;

        for (int i = 0; i < battleUIEX.Length; i++)
        {
            if (battleUIEX[i] != null)
            {
                battleUIEX[i].SetActive(false);
            }
        }
    }

    public void SKillname(string name)
    {
        Skillnames.text = name;
    }
    
    public void StartBattle(Room room)
    {
        SKillname("");
        isBattle = true;

        HideBattleUIEX();

        if (RoomNavigationUI.instance != null)
        {
            RoomNavigationUI.instance.HideAll();
        }

        SetupBattlePositions(room);

        if (battleUI != null)
            battleUI.SetActive(true);

        // BGM
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopBattleBgm();
            AudioManager.instance.PlayBattleBgm();
        }

        if (TurnManager.instance != null)
        {
            TurnManager.instance.RegisterRoom(room);
        }

        Debug.Log("전투 시작");
    }


    // =========================================================
    // Battle Position
    // =========================================================

    public void SetupBattlePositions(Room room)
    {
        positioning.Setup(room, PartyManager.instance, Party, Enemy);
    }

    // =========================================================
    // Inventory Item Targeting
    // =========================================================

    /// <summary>
    /// 아이템 선택 후 현재 파티를 전투 슬롯에 배치하고, 아군 클릭을 기다립니다.
    /// 아이템 사용은 턴/스킬 선택과 분리되어 있어 기존 타겟 선택 로직을 덮어쓰지 않습니다.
    /// </summary>
    public bool BeginItemTargetSelection(BackpackUIManager selector)
    {
        if (selector == null || PartyManager.instance == null || PartyManager.instance.partySlots == null)
            return false;

        if (Party == null || Party.Length == 0)
        {
            Debug.LogWarning("[BattleManager] 아이템 대상 선택용 파티 스폰 위치가 설정되어 있지 않습니다.");
            return false;
        }

        PartyManager.instance.PlacePartyAtPositions(Party);
        itemTargetSelector = selector;
        Debug.Log("[아이템] 사용할 아군 로봇을 선택하세요.");
        return true;
    }
    public bool BeginBlackSmithSelector(BlackSmith selector)
    {
        if (selector == null ||
            PartyManager.instance == null ||
            PartyManager.instance.partySlots == null)
        {
            return false;
        }

        if (Party == null || Party.Length == 0)
        {
            Debug.LogWarning(
                "[BlackSmith] 파티 스폰 위치가 설정되어 있지 않습니다.");
            return false;
        }

        // 다른 대상 선택이 진행 중이면 중복 시작하지 않는다.
        if (itemTargetSelector != null ||
            fixerTargetSelector != null ||
            fountainTargetSelector != null)
        {
            Debug.LogWarning(
                "[BlackSmith] 다른 대상 선택이 진행 중입니다.");
            return false;
        }

        BlackSmithTargetSelector = selector;

        // 파티 배치
        selector.PlacePartyAtBlackSmithPositions();

        Debug.Log("[BlackMarket] 장착할 로봇을 선택하세요.");
        return true;
    }
    public bool BeginBlackMarketSelector(BlackMarket selector)
    {
        if (selector == null ||
            PartyManager.instance == null ||
            PartyManager.instance.partySlots == null)
        {
            return false;
        }

        if (Party == null || Party.Length == 0)
        {
            Debug.LogWarning(
                "[BlackMarket] 파티 스폰 위치가 설정되어 있지 않습니다.");
            return false;
        }

        // 다른 대상 선택이 진행 중이면 중복 시작하지 않는다.
        if (itemTargetSelector != null ||
            fixerTargetSelector != null ||
            fountainTargetSelector != null)
        {
            Debug.LogWarning(
                "[BlackMarket] 다른 대상 선택이 진행 중입니다.");
            return false;
        }

        BlackMarketTargetSelector = selector;

        // 파티 배치
        selector.PlacePartyAtBlackMarketPositions();

        Debug.Log("[BlackMarket] 장착할 로봇을 선택하세요.");
        return true;
    }
    public bool BeginfountainSelector(fountain selector)
    {
        if (selector == null ||
            PartyManager.instance == null ||
            PartyManager.instance.partySlots == null)
        {
            return false;
        }

        if (Party == null || Party.Length == 0)
        {
            Debug.LogWarning(
                "[BattleManager] 파티 스폰 위치가 설정되어 있지 않습니다.");
            return false;
        }

        // 다른 대상 선택이 진행 중이면 중복 시작하지 않는다.
        if (itemTargetSelector != null ||
            fixerTargetSelector != null ||
            fountainTargetSelector != null)
        {
            Debug.LogWarning(
                "[BattleManager] 다른 대상 선택이 진행 중입니다.");
            return false;
        }

        fountainTargetSelector = selector;

        // 파티 배치
        selector.PlacePartyAtfountainPositions();

        Debug.Log("[Fountain] 청소할 로봇을 선택하세요.");
        return true;
    }

    public bool BeginFixerSelector(Fixer selector)
    {
        if (itemTargetSelector != null ||
            fixerTargetSelector != null ||
             fountainTargetSelector != null)
        {
            Debug.LogWarning(
                "[BattleManager] 다른 대상 선택이 진행 중입니다."
            );
            return false;
        }
        if (selector == null ||
            PartyManager.instance == null ||
            PartyManager.instance.partySlots == null)
        {
            return false;
        }

        if (Party == null || Party.Length == 0)
        {
            Debug.LogWarning(
                "[BattleManager] 파티 스폰 위치가 설정되어 있지 않습니다.");
            return false;
        }

        // 다른 대상 선택 작업과 중복되지 않도록 정리
        itemTargetSelector = null;
        fixerTargetSelector = selector;

        Debug.Log("[Fixer] 수리할 로봇을 선택하세요.");

        // 파티원 소환 및 배치
        selector.PlacePartyAtRepairPositions();

        return true;
    }
    public bool TryHandleBlackMarketTarget(Unit target)
    {
        if (BlackMarketTargetSelector == null)
            return false;
        Debug.Log("블랙마켓 배틀매니저로 이동");
        // 잘못된 대상이어도 선택 모드는 유지한다.
        if (target == null ||
            target.health <= 0 ||
            !IsCurrentPartyMember(target))
        {
            return true;
        }

        lastTargetHandledFrame = Time.frameCount;

        BlackMarket selectedBlackMarket = BlackMarketTargetSelector;

        // 선택 완료 처리 중 중복 클릭 방지
        RobotClick rc = target.GetComponent<RobotClick>();

        if (rc != null && rc.myUI != null)
            rc.myUI.SetActive(false);

        bool success = selectedBlackMarket.OnRobotSelected(target);

        if (success)
        {
            BlackMarketTargetSelector = null;
            Debug.Log("[BlackMarket] 로봇 선택 완료");
        }

        return true;
    }
    public bool TryHandleBlackSmithTarget(Unit target)
    {
        if (BlackSmithTargetSelector == null)
            return false;
        Debug.Log("대장간 배틀매니저로 이동");
        // 잘못된 대상이어도 선택 모드는 유지한다.
        if (target == null ||
            target.health <= 0 ||
            !IsCurrentPartyMember(target))
        {
            return true;
        }

        lastTargetHandledFrame = Time.frameCount;

        BlackSmith selectedBlackSmith = BlackSmithTargetSelector;

        // 선택 완료 처리 중 중복 클릭 방지
        RobotClick rc = target.GetComponent<RobotClick>();

        if (rc != null && rc.myUI != null)
            rc.myUI.SetActive(false);

        bool success = selectedBlackSmith.OnRobotSelected(target);

        if (success)
        {
            BlackSmithTargetSelector = null;
            Debug.Log("[BlackSmith] 로봇 선택 완료");
        }

        return true;
    }
    public bool TryHandleFountainTarget(Unit target)
    {
        if (fountainTargetSelector == null)
            return false;

        // 잘못된 대상이어도 선택 모드는 유지한다.
        if (target == null ||
            target.health <= 0 ||
            !IsCurrentPartyMember(target))
        {
            return true;
        }

        lastTargetHandledFrame = Time.frameCount;

        fountain selectedFountain = fountainTargetSelector;

        // 선택 완료 처리 중 중복 클릭 방지
        RobotClick rc = target.GetComponent<RobotClick>();

        if (rc != null && rc.myUI != null)
            rc.myUI.SetActive(false);

        bool success = selectedFountain.OnRobotSelected(target);

        if (success)
        {
            fountainTargetSelector = null;
            Debug.Log("[Fountain] 로봇 청소 완료");
        }
        
        return true;
    }

    /// <summary>
    /// 분수대 로봇 선택을 취소한다.
    /// </summary>
    public void CancelFountainTargetSelection()
    {
        if (fountainTargetSelector == null)
            return;

        fountain selectedFountain = fountainTargetSelector;
        fountainTargetSelector = null;

        selectedFountain.CancelRepairSelection();

        Debug.Log("[Fountain] 로봇 선택이 취소되었습니다.");
    }

    public void CancelFixerTargetSelection()
    {
        if (fixerTargetSelector == null)
            return;

        Fixer selectedFixer = fixerTargetSelector;
        fixerTargetSelector = null;

        selectedFixer.RecallRepairParty();
        selectedFixer.RestoreInteractionUI();
        selectedFixer.FocusOut();
        Debug.Log("[Fixer] 수리 대상 선택이 취소되었습니다.");
    }
    /// <summary>PlayerUnit 클릭에서 호출됩니다. 아이템 선택을 처리했으면 true를 반환합니다.</summary>
    public bool TryHandleItemTarget(Unit target)
    {
        if (itemTargetSelector == null)
            return false;

        if (target == null || target.health <= 0 || !IsCurrentPartyMember(target))
            return true;

        lastTargetHandledFrame = Time.frameCount;

        RobotClick rc = target.GetComponent<RobotClick>();
        if (rc != null && rc.myUI != null)
        {
            rc.myUI.SetActive(false);
        }

        if (itemTargetSelector.ApplySelectedItemTo(target))
            itemTargetSelector = null;

        return true;
    }

    public void CancelItemTargetSelection()
    {
        itemTargetSelector = null;
    }

    private bool IsCurrentPartyMember(Unit target)
    {
        if (PartyManager.instance == null || PartyManager.instance.partySlots == null)
            return false;

        foreach (Unit unit in PartyManager.instance.partySlots)
        {
            if (unit == target)
                return true;
        }

        return false;
    }


    // =========================================================
    // Target Distance
    // =========================================================

    public bool CanPlayerTargetEnemy(
        PlayerUnit attacker,
        Enemy target,
        SkillData skill)
    {
        Unit[] partySlots = PartyManager.instance != null
            ? PartyManager.instance.partySlots
            : null;

        return targetingRules.CanPlayerTargetEnemy(
            partySlots, positioning.EnemyColumns, attacker, target, skill);
    }


    public int GetPartyColumn(PlayerUnit player)
    {
        Unit[] partySlots = PartyManager.instance != null
            ? PartyManager.instance.partySlots
            : null;

        return targetingRules.GetPartyColumn(partySlots, player);
    }


    public bool CanUseSkillAtCurrentColumn(
        PlayerUnit player,
        SkillData skill)
    {
        Unit[] partySlots = PartyManager.instance != null
            ? PartyManager.instance.partySlots
            : null;

        return targetingRules.CanUseSkillAtCurrentColumn(
            partySlots, player, skill);
    }


    // =========================================================
    // Enemy Target UI
    // =========================================================

    public void RefreshEnemyTargetAvailability(
        PlayerUnit attacker,
        SkillData skill)
    {
        foreach (KeyValuePair<Enemy, int> pair
                 in positioning.EnemyColumns)
        {
            Enemy enemy = pair.Key;

            if (enemy != null &&
                enemy.gameObject.activeInHierarchy)
            {
                enemy.SetTargetSelectable(
                    CanPlayerTargetEnemy(
                        attacker,
                        enemy,
                        skill));
            }
        }
    }


    public void ClearEnemyTargetAvailability()
    {
        foreach (Enemy enemy in positioning.EnemyColumns.Keys)
        {
            if (enemy != null)
            {
                enemy.SetTargetSelectable(true);
            }
        }
    }


    // =========================================================
    // Turn
    // =========================================================

    public void StartTurn(Unit unit)
    {
        if (unit == null)
            return;

        HideBattleUIEX();

        if (turnText != null)
        {
            turnText.text =
                $"{unit.Unitname} TURN";
        }

        if (unit is PlayerUnit player)
        {
            ShowPlayerUI(player);

            if (TurnManager.instance != null)
            {
                TurnManager.instance.waitingForTarget =
                    true;
            }
        }
        else
        {
            HidePlayerUI();

            if (TurnManager.instance != null)
            {
                TurnManager.instance.waitingForTarget =
                    false;
            }
        }
    }


    // =========================================================
    // Player UI
    // =========================================================

    public void ShowPlayerUI(PlayerUnit player)
    {
        if (player == null ||
            cachedButtons == null)
        {
            return;
        }

        HideBattleUIEX();

        int skillCount =
            player.skills != null
                ? player.skills.Count
                : 0;

        for (int i = 0;
             i < cachedButtons.Length;
             i++)
        {
            var cached =
                cachedButtons[i];

            if (cached.root == null)
                continue;

            if (i < skillCount &&
                player.skills[i] != null)
            {
                SkillData skill =
                    player.skills[i];

                cached.root.SetActive(true);

                // 스킬 이름
                if (cached.label != null)
                {
                    cached.label.text =
                        skill.skillName;
                }

                // 스킬 아이콘
                if (cached.iconImage != null)
                {
                    cached.iconImage.sprite =
                        skill.icon;

                    cached.iconImage.enabled =
                        skill.icon != null;
                }

                // 현재 열에서 사용 가능한지
                if (cached.button != null)
                {
                    cached.button.interactable =
                        CanUseSkillAtCurrentColumn(
                            player,
                            skill);
                }
            }
            else
            {
                cached.root.SetActive(false);
            }
        }

        UpdateFormationMoveButtons(player);
    }


    public void HidePlayerUI()
    {
        if (cachedButtons != null)
        {
            for (int i = 0;
                 i < cachedButtons.Length;
                 i++)
            {
                if (cachedButtons[i].root != null)
                {
                    cachedButtons[i]
                        .root
                        .SetActive(false);
                }
            }
        }

        HideBattleUIEX();

        isSelectingFormationMove = false;

        if (formationMoveButton != null)
        {
            formationMoveButton.gameObject
                .SetActive(false);
        }
    }


    // =========================================================
    // Formation Move
    // =========================================================

    private void CacheFormationMoveButtons()
    {
        if (formationMoveButton != null)
        {
            formationMoveButton.onClick
                .AddListener(BeginFormationMove);

            formationMoveButton.gameObject
                .SetActive(false);
        }
    }


    private void UpdateFormationMoveButtons(
        PlayerUnit player)
    {
        if (formationMoveButton != null)
        {
            formationMoveButton.gameObject
                .SetActive(true);

            formationMoveButton.interactable =
                CanStartFormationMove(player);
        }
    }


    public void BeginFormationMove()
    {
        if (TurnManager.instance == null)
            return;

        if (!(TurnManager.instance.currentUnit
              is PlayerUnit player))
        {
            return;
        }

        if (!CanStartFormationMove(player))
            return;

        isSelectingFormationMove = true;

        player.selectedSkill = null;

        ClearEnemyTargetAvailability();

        HideBattleUIEX();

        Debug.Log(
            "[전투] 교대할 아군을 선택하세요.");
    }
    /// <summary>
    /// 아군 로봇 클릭 시 Fixer 대상 선택을 처리합니다.
    /// </summary>
    public bool TryHandleFixerTarget(Unit target)
    {
        if (fixerTargetSelector == null)
            return false;

        // 쓰러졌거나 파티원이 아닌 경우 선택을 유지한다.
        if (target == null ||
            target.health <= 0 ||
            !IsCurrentPartyMember(target))
        {
            return true;
        }

        // 같은 프레임의 중복 클릭 처리 방지
        lastTargetHandledFrame = Time.frameCount;

        // 중복 처리를 방지하기 위해 선택 상태를 먼저 해제한다.
        Fixer selectedFixer = fixerTargetSelector;
        fixerTargetSelector = null;

        // 로봇의 스탯 UI가 열려 있다면 닫는다.
        RobotClick rc = target.GetComponent<RobotClick>();

        if (rc != null && rc.myUI != null)
            rc.myUI.SetActive(false);

        // 기존 Fixer 수리 기능 실행
        selectedFixer.OnRobotSelected(target);

        // 파티원 회수 및 상호작용 상태 복원
        selectedFixer.RecallRepairParty();
        selectedFixer.RestoreInteractionUI();
        selectedFixer.FocusOut();

        Debug.Log("[Fixer] 수리 대상 선택 처리 완료");

        return true;
    }

    public bool TryHandleFormationMoveTarget(
        PlayerUnit mover,
        PlayerUnit target)
    {
        if (!isSelectingFormationMove ||
            mover == null ||
            target == null)
        {
            return false;
        }

        if (!isBattle ||
            TurnManager.instance == null ||
            !TurnManager.instance.waitingForTarget ||
            TurnManager.instance.currentUnit != mover ||
            PartyManager.instance == null ||
            PartyManager.instance.partySlots == null)
        {
            return true;
        }

        Unit[] slots =
            PartyManager.instance.partySlots;

        if (!formationMoveRules.TrySwap(
            slots, mover, target,
            out int currentColumn, out int targetColumn))
        {
            Debug.Log(
                "[전투] 이 아군의 열까지는 이동할 수 없습니다.");

            return true;
        }

        if (Party != null &&
            Party.Length > 0)
        {
            PartyManager.instance
                .PlacePartyAtPositions(Party);
        }

        PartyManager.instance.SaveParty();

        Debug.Log(
            $"[전투] {mover.Unitname}: " +
            $"{currentColumn + 1}열 → " +
            $"{targetColumn + 1}열 이동");

        EndPlayerAction();

        return true;
    }


    private bool CanStartFormationMove(
        PlayerUnit player)
    {
        return player != null &&
               PartyManager.instance != null &&
               formationMoveRules != null &&
               formationMoveRules.CanStartMove(
                   PartyManager.instance.partySlots,
                   player);
    }


    // =========================================================
    // Skill Select
    // =========================================================

    public void SelectSkill(int index)
    {
        
        if (TurnManager.instance == null)
            return;

        PlayerUnit player =
            TurnManager.instance.currentUnit
            as PlayerUnit;

        if (player == null ||
            player.skills == null)
        {
            return;
        }

        if (index < 0 ||
            index >= player.skills.Count)
        {
            return;
        }

        isSelectingFormationMove = false;

        SkillData selectedSkill =
            player.skills[index];
        SKillname(LocalizationManager.GetLocalizedString(selectedSkill.skillName));
        if (selectedSkill == null)
            return;

        if (!CanUseSkillAtCurrentColumn(
            player,
            selectedSkill))
        {
            Debug.Log(
                $"[전투] {selectedSkill.skillName}은(는) " +
                $"현재 {GetPartyColumn(player) + 1}열에서는 사용할 수 없습니다.");

            return;
        }

        player.selectedSkill =
            selectedSkill;

        HideBattleUIEX();

        Debug.Log(
            $"{player.selectedSkill.skillName} 선택");

        if (player.selectedSkill.targetType
            == TargetType.Ally)
        {
            ClearEnemyTargetAvailability();

            Debug.Log(
                "[전투] 아군 1명을 선택하세요.");
        }
        else
        {
            RefreshEnemyTargetAvailability(
                player,
                player.selectedSkill);
        }

        if (player.selectedSkill.targetType
            == TargetType.Self)
        {
            player.SelectTarget(player);
        }
    }


    // =========================================================
    // Skill Sound
    // =========================================================

    public void PlaySkillSound(
        SkillData skill)
    {
        if (skill == null ||
            skill.soundEffect == null)
        {
            return;
        }

        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(
                skill.soundEffect);
        }
        else if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(
                skill.soundEffect);
        }
    }


    // =========================================================
    // Player Action End
    // =========================================================

    public void EndPlayerAction()
    {
        isSelectingFormationMove = false;

        HidePlayerUI();

        ClearEnemyTargetAvailability();

        if (TurnManager.instance != null)
        {
            TurnManager.instance.waitingForTarget =
                false;

            TurnManager.instance.EndTurn();
        }
    }


    // =========================================================
    // Battle End
    // =========================================================

    public void EndBattle(bool win)
    {
        CancelFixerTargetSelection();
        isBattle = false;
        CancelItemTargetSelection();

        HideBattleUIEX();

        DisableBattleUnits();

        if (battleUI != null)
            battleUI.SetActive(false);

        HidePlayerUI();

        if (TurnManager.instance != null)
        {
            TurnManager.instance.waitingForTarget =
                false;
        }

        if (win)
        {
            if (RoomManager.instance != null)
            {
                RoomManager.instance
                    .ClearCurrentRoom();

                if (RoomManager.instance.currentRoom != null)
                {
                    RoomManager.instance
                        .currentRoom
                        .GenerateAndOpenBattleReward();
                }
                else if (Reward.Instance != null)
                {
                    Reward.Instance.RewardOpen();
                }
            }
            else if (Reward.Instance != null)
            {
                Reward.Instance.RewardOpen();
            }

            Debug.Log("승리!");
        }
        else
        {
            AudioManager.instance.StopBattleBgm();
            SceneChanger.BG("Mainmenu");
            Save.DeleteSaveSlot(Save.activeSlot);
            Debug.Log("패배!");
        }
    }


    private void DisableBattleUnits()
    {
        if (PartyManager.instance != null &&
            PartyManager.instance.partySlots != null)
        {
            foreach (Unit unit
                     in PartyManager.instance.partySlots)
            {
                if (unit != null)
                {
                    unit.gameObject
                        .SetActive(false);
                }
            }
        }

        positioning.DisableEnemies();
    }


    // =========================================================
    // Rearrange Enemies
    // =========================================================

    public void RearrangeEnemies()
    {
        positioning.Rearrange(Enemy);
    }
}
