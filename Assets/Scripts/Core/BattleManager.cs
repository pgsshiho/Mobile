using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    [Header("Formation Move Button")]
    [Tooltip("누른 뒤 아군을 선택하면 해당 아군의 열로 교대 이동하는 버튼")]
    public Button formationMoveButton;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;
    public AudioClip battleBgm;
    
    // UI Cache to avoid repeated GetComponent/GetComponentInChildren in loops
    private struct CachedSkillButton
    {
        public GameObject root;
        public Button button;
        public TMP_Text label;
        public Image iconImage;
    }

    private CachedSkillButton[] cachedButtons;
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
        CacheFormationMoveButtons();
    }

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
            if (btn != null)
            {
                int skillIndex = i;
                Button button = btn.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(
                        () => SelectSkill(skillIndex)
                    );
                }

                cachedButtons[i] = new CachedSkillButton
                {
                    root = btn,
                    button = button,
                    label = btn.GetComponentInChildren<TMP_Text>(),
                    iconImage = btn.GetComponent<Image>()
                };
            }
        }
    }

    private void CacheFormationMoveButtons()
    {
        if (formationMoveButton != null)
        {
            formationMoveButton.onClick.AddListener(BeginFormationMove);
            formationMoveButton.gameObject.SetActive(false);
        }
    }

    public void StartBattle(Room room)
    {
        isBattle = true;
        if (RoomNavigationUI.instance != null)
        {
            RoomNavigationUI.instance.HideAll();
        }

        // 1. 유닛 위치 정렬 (파티 및 에너미 둘 다 지정된 위치로 배치)
        SetupBattlePositions(room);

        if (battleUI != null)
            battleUI.SetActive(true);

        if (AudioManager.instance != null)
            AudioManager.instance.StopBgm();
            AudioManager.instance.PlayBattleBgm();

        if (TurnManager.instance != null)
            TurnManager.instance.RegisterRoom(room);

        Debug.Log("전투 시작");
    }

    /// <summary>
    /// 배틀 시작 시 Party와 Enemy 유닛들을 인스펙터에 지정된 위치(Party, Enemy)로 배치합니다.
    /// </summary>
    public void SetupBattlePositions(Room room)
    {
        enemyColumns.Clear();

        // 1. 파티 유닛들을 지정된 Party 위치들로 배치
        if (PartyManager.instance != null && Party != null && Party.Length > 0)
        {
            PartyManager.instance.PlacePartyAtPositions(Party);
        }

        // 2. 적 유닛들을 지정된 Enemy 위치들로 배치
        if (room != null && room.enemies != null && room.enemies.Length > 0)
        {
            for (int i = 0; i < room.enemies.Length; i++)
            {
                Enemy enemy = room.enemies[i];
                if (enemy == null) continue;

                Transform targetPoint = null;

                // BattleManager의 Enemy Transform 배열을 최우선으로 적용
                if (Enemy != null && i < Enemy.Length && Enemy[i] != null)
                {
                    targetPoint = Enemy[i];
                }
                // 없으면 방의 기본 스폰 포인트 적용
                else if (room.enemySpawnPoints != null && i < room.enemySpawnPoints.Length && room.enemySpawnPoints[i] != null)
                {
                    targetPoint = room.enemySpawnPoints[i];
                }

                if (targetPoint != null)
                {
                    enemy.transform.position = targetPoint.position;
                    enemy.transform.rotation = targetPoint.rotation;
                }

                enemy.gameObject.SetActive(true);
                enemyColumns[enemy] = i;
            }
        }
    }

    public bool CanPlayerTargetEnemy(
        PlayerUnit attacker,
        Enemy target,
        SkillData skill)
    {
        if (attacker == null || target == null || skill == null ||
            PartyManager.instance == null)
            return false;

        int partyColumn = GetPartyColumn(attacker);

        if (partyColumn < 0 ||
            !enemyColumns.TryGetValue(target, out int enemyColumn))
            return false;

        int distance = partyColumn + enemyColumn + 1;
        return distance <= Mathf.Max(1, skill.maxTargetDistance);
    }

    public int GetPartyColumn(PlayerUnit player)
    {
        if (player == null || PartyManager.instance == null ||
            PartyManager.instance.partySlots == null)
            return -1;

        return System.Array.IndexOf(PartyManager.instance.partySlots, player);
    }

    public bool CanUseSkillAtCurrentColumn(PlayerUnit player, SkillData skill)
    {
        if (player == null || skill == null)
            return false;

        int partyColumn = GetPartyColumn(player);
        if (partyColumn < 0)
            return false;

        int minColumn = Mathf.Min(skill.minUserColumn, skill.maxUserColumn);
        int maxColumn = Mathf.Max(skill.minUserColumn, skill.maxUserColumn);
        return partyColumn >= minColumn && partyColumn <= maxColumn;
    }

    public void RefreshEnemyTargetAvailability(
        PlayerUnit attacker,
        SkillData skill)
    {
        foreach (KeyValuePair<Enemy, int> pair in enemyColumns)
        {
            Enemy enemy = pair.Key;
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.SetTargetSelectable(
                    CanPlayerTargetEnemy(attacker, enemy, skill)
                );
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

    public void StartTurn(Unit unit)
    {
        if (unit == null)
            return;

        if (turnText != null)
            turnText.text = $"{unit.Unitname} TURN";

        if (unit is PlayerUnit player)
        {
            ShowPlayerUI(player);

            if (TurnManager.instance != null)
                TurnManager.instance.waitingForTarget = true;
        }
        else
        {
            HidePlayerUI();

            if (TurnManager.instance != null)
                TurnManager.instance.waitingForTarget = false;
        }
    }

    public void ShowPlayerUI(PlayerUnit player)
    {
        if (player == null || cachedButtons == null)
            return;

        int skillCount = (player.skills != null) ? player.skills.Count : 0;

        for (int i = 0; i < cachedButtons.Length; i++)
        {
            var cached = cachedButtons[i];
            if (cached.root == null)
                continue;

            if (i < skillCount && player.skills[i] != null)
            {
                SkillData skill = player.skills[i];
                cached.root.SetActive(true);

                if (cached.label != null)
                    cached.label.text = skill.skillName;

                if (cached.iconImage != null)
                {
                    cached.iconImage.sprite = skill.icon;
                    cached.iconImage.enabled = skill.icon != null;
                }

                if (cached.button != null)
                {
                    cached.button.interactable = CanUseSkillAtCurrentColumn(
                        player,
                        skill
                    );
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
            for (int i = 0; i < cachedButtons.Length; i++)
            {
                if (cachedButtons[i].root != null)
                {
                    cachedButtons[i].root.SetActive(false);
                }
            }
        }

        isSelectingFormationMove = false;
        if (formationMoveButton != null)
            formationMoveButton.gameObject.SetActive(false);
    }

    private void UpdateFormationMoveButtons(PlayerUnit player)
    {
        if (formationMoveButton != null)
        {
            formationMoveButton.gameObject.SetActive(true);
            formationMoveButton.interactable = CanStartFormationMove(player);
        }
    }

    // Inspector의 단일 열 이동 버튼에 직접 연결해도 되는 공개 메서드다.
    public void BeginFormationMove()
    {
        if (TurnManager.instance == null ||
            !(TurnManager.instance.currentUnit is PlayerUnit player) ||
            !CanStartFormationMove(player))
            return;

        isSelectingFormationMove = true;
        player.selectedSkill = null;
        ClearEnemyTargetAvailability();
        Debug.Log("[전투] 교대할 아군을 선택하세요.");
    }

    public bool TryHandleFormationMoveTarget(
        PlayerUnit mover,
        PlayerUnit target)
    {
        if (!isSelectingFormationMove || mover == null || target == null)
            return false;

        if (!isBattle || TurnManager.instance == null ||
            !TurnManager.instance.waitingForTarget ||
            TurnManager.instance.currentUnit != mover ||
            PartyManager.instance == null ||
            PartyManager.instance.partySlots == null)
            return true;

        Unit[] slots = PartyManager.instance.partySlots;
        int currentColumn = GetPartyColumn(mover);
        int targetColumn = GetPartyColumn(target);

        if (currentColumn < 0 || targetColumn < 0 ||
            currentColumn == targetColumn ||
            !CanMoveToColumn(mover, currentColumn, targetColumn))
        {
            Debug.Log("[전투] 이 아군의 열까지는 이동할 수 없습니다.");
            return true;
        }

        // 선택한 아군과 위치를 바꿔 해당 열로 이동한다.
        slots[targetColumn] = mover;
        slots[currentColumn] = target;

        if (Party != null && Party.Length > 0)
        {
            PartyManager.instance.PlacePartyAtPositions(Party);
        }

        PartyManager.instance.SaveParty();
        Debug.Log($"[전투] {mover.Unitname}: {currentColumn + 1}열 → " +
                  $"{targetColumn + 1}열 이동");

        // 열 이동은 한 턴의 행동으로 처리한다.
        EndPlayerAction();
        return true;
    }

    private bool CanStartFormationMove(PlayerUnit player)
    {
        if (player == null || PartyManager.instance == null ||
            PartyManager.instance.partySlots == null)
            return false;

        int currentColumn = GetPartyColumn(player);
        if (currentColumn < 0)
            return false;

        foreach (Unit unit in PartyManager.instance.partySlots)
        {
            if (unit is PlayerUnit target && target != player &&
                target.health > 0)
            {
                int targetColumn = GetPartyColumn(target);
                if (CanMoveToColumn(player, currentColumn, targetColumn))
                    return true;
            }
        }

        return false;
    }

    private bool CanMoveToColumn(
        PlayerUnit player,
        int currentColumn,
        int targetColumn)
    {
        int distance = targetColumn - currentColumn;
        return distance < 0
            ? -distance <= player.maxForwardMoveColumns
            : distance > 0 && distance <= player.maxBackwardMoveColumns;
    }

    public void SelectSkill(int index)
    {
        if (TurnManager.instance == null)
            return;

        PlayerUnit player = TurnManager.instance.currentUnit as PlayerUnit;
        if (player == null || player.skills == null)
            return;

        if (index < 0 || index >= player.skills.Count)
            return;

        isSelectingFormationMove = false;

        SkillData selectedSkill = player.skills[index];
        if (selectedSkill == null)
            return;

        if (!CanUseSkillAtCurrentColumn(player, selectedSkill))
        {
            Debug.Log($"[전투] {selectedSkill.skillName}은(는) 현재 " +
                      $"{GetPartyColumn(player) + 1}열에서는 사용할 수 없습니다.");
            return;
        }

        player.selectedSkill = selectedSkill;

        Debug.Log($"{player.selectedSkill.skillName} 선택");

        if (player.selectedSkill.targetType == TargetType.Ally)
        {
            ClearEnemyTargetAvailability();
            Debug.Log("[전투] 아군 1명을 선택하세요.");
        }
        else
        {
            RefreshEnemyTargetAvailability(
                player,
                player.selectedSkill
            );
        }

        if (player.selectedSkill.targetType == TargetType.Self)
        {
            player.SelectTarget(player);
        }
    }

    public void PlaySkillSound(SkillData skill)
    {
        if (skill == null || skill.soundEffect == null)
            return;

        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(skill.soundEffect);
        }
        else if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(skill.soundEffect);
        }
    }

    public void EndPlayerAction()
    {
        isSelectingFormationMove = false;
        HidePlayerUI();
        ClearEnemyTargetAvailability();

        if (TurnManager.instance != null)
        {
            TurnManager.instance.waitingForTarget = false;
            TurnManager.instance.EndTurn();
        }
    }

    public void EndBattle(bool win)
    {
        isBattle = false;

        if (battleUI != null)
            battleUI.SetActive(false);

        HidePlayerUI();

        if (TurnManager.instance != null)
            TurnManager.instance.waitingForTarget = false;

        if (win)
        {
            if (RoomManager.instance != null)
            {
                RoomManager.instance.ClearCurrentRoom();

                if (RoomManager.instance.currentRoom != null)
                {
                    RoomManager.instance.currentRoom.GenerateAndOpenReward();
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
}
