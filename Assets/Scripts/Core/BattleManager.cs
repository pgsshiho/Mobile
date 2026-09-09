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

        CacheSkillButtons();
        CacheBattleUIEX();
        CacheFormationMoveButtons();

        HideBattleUIEX();
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
    // Target Distance
    // =========================================================

    public bool CanPlayerTargetEnemy(
        PlayerUnit attacker,
        Enemy target,
        SkillData skill)
    {
        if (attacker == null ||
            target == null ||
            skill == null ||
            PartyManager.instance == null)
        {
            return false;
        }

        int partyColumn =
            GetPartyColumn(attacker);

        if (partyColumn < 0 ||
            !enemyColumns.TryGetValue(
                target,
                out int enemyColumn))
        {
            return false;
        }

        int distance =
            partyColumn + enemyColumn + 1;

        return distance <=
               Mathf.Max(1, skill.maxTargetDistance);
    }


    public int GetPartyColumn(PlayerUnit player)
    {
        if (player == null ||
            PartyManager.instance == null ||
            PartyManager.instance.partySlots == null)
        {
            return -1;
        }

        return System.Array.IndexOf(
            PartyManager.instance.partySlots,
            player);
    }


    public bool CanUseSkillAtCurrentColumn(
        PlayerUnit player,
        SkillData skill)
    {
        if (player == null || skill == null)
            return false;

        int partyColumn =
            GetPartyColumn(player);

        if (partyColumn < 0)
            return false;

        int minColumn =
            Mathf.Min(
                skill.minUserColumn,
                skill.maxUserColumn);

        int maxColumn =
            Mathf.Max(
                skill.minUserColumn,
                skill.maxUserColumn);

        return partyColumn >= minColumn &&
               partyColumn <= maxColumn;
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

        int currentColumn =
            GetPartyColumn(mover);

        int targetColumn =
            GetPartyColumn(target);

        if (currentColumn < 0 ||
            targetColumn < 0 ||
            currentColumn == targetColumn ||
            !CanMoveToColumn(
                mover,
                currentColumn,
                targetColumn))
        {
            Debug.Log(
                "[전투] 이 아군의 열까지는 이동할 수 없습니다.");

            return true;
        }

        slots[targetColumn] = mover;
        slots[currentColumn] = target;

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
        if (player == null ||
            PartyManager.instance == null ||
            PartyManager.instance.partySlots == null)
        {
            return false;
        }

        int currentColumn =
            GetPartyColumn(player);

        if (currentColumn < 0)
            return false;

        foreach (Unit unit
                 in PartyManager.instance.partySlots)
        {
            if (unit is PlayerUnit target &&
                target != player &&
                target.health > 0)
            {
                int targetColumn =
                    GetPartyColumn(target);

                if (CanMoveToColumn(
                    player,
                    currentColumn,
                    targetColumn))
                {
                    return true;
                }
            }
        }

        return false;
    }


    private bool CanMoveToColumn(
        PlayerUnit player,
        int currentColumn,
        int targetColumn)
    {
        int distance =
            targetColumn - currentColumn;

        return distance < 0
            ? -distance <= player.maxForwardMoveColumns
            : distance > 0 &&
              distance <= player.maxBackwardMoveColumns;
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
        isBattle = false;

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
                        .GenerateAndOpenReward();
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