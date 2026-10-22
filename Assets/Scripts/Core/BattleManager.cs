using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Collections;
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
    private int lastTargetHandledFrame = -1;

    public bool IsSelectingFixerTarget =>
        fixerTargetSelector != null;

    /// <summary>아이템이나 Fixer 등 대상을 선택 중이거나 이번 프레임에 막 선택을 완료했는지 여부입니다.</summary>
    public bool IsSelectingAnyTarget =>
        IsSelectingItemTarget || IsSelectingFixerTarget || (Time.frameCount == lastTargetHandledFrame);
    private struct CachedSkillButton
    {
        public GameObject root;
        public Button button;
        public TMP_Text label;
        public Image iconImage;
    }

    private CachedSkillButton[] cachedButtons;

    private TMP_Text[] battleUIEXTexts;

    private bool isSelectingFormationMove;
    private BackpackUIManager itemTargetSelector;

    /// <summary>인벤토리 아이템의 아군 대상을 고르는 중인지 여부입니다.</summary>
    public bool IsSelectingItemTarget => itemTargetSelector != null;

    // 전투 규칙은 UI/씬 참조 없이 별도 객체에 둔다.
    private readonly BattleTargetingRules targetingRules =
        new BattleTargetingRules();
    private FormationMoveRules formationMoveRules;

    private readonly Dictionary<Enemy, int> enemyColumns =
        new Dictionary<Enemy, int>();


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


    // =========================================================
    // Battle Start
    // =========================================================

    public void StartBattle(Room room)
    {
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
            AudioManager.instance.StopBgm();
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
        enemyColumns.Clear();

        if (PartyManager.instance != null &&
            Party != null &&
            Party.Length > 0)
        {
            PartyManager.instance.PlacePartyAtPositions(Party);
        }

        if (room != null &&
            room.enemies != null &&
            room.enemies.Length > 0)
        {
            for (int i = 0; i < room.enemies.Length; i++)
            {
                Enemy enemy = room.enemies[i];

                if (enemy == null)
                    continue;

                Transform targetPoint = null;

                if (Enemy != null &&
                    i < Enemy.Length &&
                    Enemy[i] != null)
                {
                    targetPoint = Enemy[i];
                }
                else if (room.enemySpawnPoints != null &&
                         i < room.enemySpawnPoints.Length &&
                         room.enemySpawnPoints[i] != null)
                {
                    targetPoint =
                        room.enemySpawnPoints[i];
                }

                if (targetPoint != null)
                {
                    enemy.transform.position =
                        targetPoint.position;

                    enemy.transform.rotation =
                        targetPoint.rotation;
                }

                enemy.gameObject.SetActive(true);

                enemyColumns[enemy] = i;
            }
        }
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
    public bool BeginFixerSelector(Fixer selector)
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

        // 다른 대상 선택 작업과 중복되지 않도록 정리
        itemTargetSelector = null;
        fixerTargetSelector = selector;
        // 파티원 소환 및 배치
        selector.PlacePartyAtRepairPositions();

        Debug.Log("[Fixer] 대상을 선택하세요.");

        return true;
    }
    /// <summary>
    /// 아군 로봇 클릭 시 Fixer 대상 선택을 처리합니다.
    /// PlayerUnit의 클릭 이벤트에서 호출해야 합니다.
    /// </summary>
    public bool TryHandleFixerTarget(Unit target)
    {
        if (fixerTargetSelector == null)
            return false;

        if (target == null ||
            target.health <= 0 ||
            !IsCurrentPartyMember(target))
        {
            // 유효하지 않은 대상 클릭 — 선택 모드 유지 (소비하지 않음)
            return true;
        }

        // 이번 프레임에 선택 처리가 완료되었음을 기록 (스탯창 등이 열리지 않도록 방어)
        lastTargetHandledFrame = Time.frameCount;

        // 선택 상태 먼저 해제 후 처리 (중복 호출 방지)
        Fixer selectedFixer = fixerTargetSelector;
        fixerTargetSelector = null;

        // 같은 프레임에서 스탯창이 켜지지 않도록 즉시 닫기
        RobotClick rc = target.GetComponent<RobotClick>();
        if (rc != null && rc.myUI != null)
        {
            rc.myUI.SetActive(false);
        }

        // 1. 소환된 파티 유닛 회수
        selectedFixer.RecallRepairParty();
        // 2. 숨겼던 UI 복구
        selectedFixer.RestoreInteractionUI();
        // 3. 선택된 로봇 수리 처리
        selectedFixer.OnRobotSelected(target);

        return true;
    }
    public void CancelFixerTargetSelection()
    {
        if (fixerTargetSelector == null)
            return;

        Fixer selectedFixer = fixerTargetSelector;
        fixerTargetSelector = null;

        selectedFixer.RecallRepairParty();
        selectedFixer.RestoreInteractionUI();
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
            partySlots, enemyColumns, attacker, target, skill);
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
                 in enemyColumns)
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
        foreach (Enemy enemy in enemyColumns.Keys)
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

        foreach (Enemy enemy
                 in enemyColumns.Keys)
        {
            if (enemy != null)
            {
                enemy.gameObject
                    .SetActive(false);
            }
        }

        enemyColumns.Clear();
    }


    // =========================================================
    // Rearrange Enemies
    // =========================================================

    public void RearrangeEnemies()
    {
        if (Enemy == null ||
            Enemy.Length == 0)
        {
            return;
        }

        List<Enemy> aliveEnemies =
            new List<Enemy>();

        foreach (KeyValuePair<Enemy, int> pair
                 in enemyColumns)
        {
            Enemy enemy = pair.Key;

            if (enemy != null &&
                enemy.gameObject.activeSelf &&
                enemy.health > 0)
            {
                aliveEnemies.Add(enemy);
            }
        }

        for (int i = 0;
             i < aliveEnemies.Count;
             i++)
        {
            Enemy enemy =
                aliveEnemies[i];

            if (Enemy[i] != null)
            {
                enemy.transform.position =
                    Enemy[i].position;

                enemy.transform.rotation =
                    Enemy[i].rotation;
            }

            enemyColumns[enemy] = i;
        }
    }
}
