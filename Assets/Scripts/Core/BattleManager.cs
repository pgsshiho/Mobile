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

    [Header("Game Over")]
    [Tooltip("아군이 전멸했을 때 활성화할 GameOver UI 또는 오브젝트")]
    public GameObject gameOverObject;


    [Header("Turn Button Colors")]
    [Tooltip("내 턴이 아닐 때 버튼들에 적용할 약간 어두운 검은색 (비활성 표현)")]
    public Color notMyTurnColor = new Color(0.32f, 0.32f, 0.32f, 0.9f);
    [Tooltip("내 턴일 때 정상 사용 가능한 버튼 색상")]
    public Color normalTurnColor = Color.white;
    [Tooltip("내 턴이지만 현재 거리/열 제한으로 사용 불가능한 스킬 색상")]
    public Color disabledSkillColor = new Color(0.48f, 0.48f, 0.48f, 0.9f);

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
        public Image[] allImages;
        public TMP_Text[] allTexts;
    }

    private CachedSkillButton[] cachedButtons;

    private TMP_Text[] battleUIEXTexts;

    private Image[] formationMoveImages;
    private TMP_Text[] formationMoveTexts;

    private bool isSelectingFormationMove;

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
                iconImage = btn.GetComponent<Image>(),
                allImages = btn.GetComponentsInChildren<Image>(true),
                allTexts = btn.GetComponentsInChildren<TMP_Text>(true)
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


    private void SetButtonVisualState(CachedSkillButton btn, bool interactable, Color color)
    {
        if (btn.button != null)
        {
            btn.button.interactable = interactable;
        }

        if (btn.allImages != null)
        {
            for (int i = 0; i < btn.allImages.Length; i++)
            {
                if (btn.allImages[i] != null)
                {
                    btn.allImages[i].color = color;
                }
            }
        }

        if (btn.allTexts != null)
        {
            Color txtColor = new Color(color.r, color.g, color.b, color.a);
            for (int i = 0; i < btn.allTexts.Length; i++)
            {
                if (btn.allTexts[i] != null)
                {
                    btn.allTexts[i].color = txtColor;
                }
            }
        }
    }

    private void SetFormationMoveButtonVisual(bool interactable, Color color)
    {
        if (formationMoveButton == null) return;

        formationMoveButton.interactable = interactable;

        if (formationMoveImages != null)
        {
            for (int i = 0; i < formationMoveImages.Length; i++)
            {
                if (formationMoveImages[i] != null)
                {
                    formationMoveImages[i].color = color;
                }
            }
        }

        if (formationMoveTexts != null)
        {
            Color txtColor = new Color(color.r, color.g, color.b, color.a);
            for (int i = 0; i < formationMoveTexts.Length; i++)
            {
                if (formationMoveTexts[i] != null)
                {
                    formationMoveTexts[i].color = txtColor;
                }
            }
        }
    }

    private void EnsureButtonsCached()
    {
        if (cachedButtons == null || cachedButtons.Length == 0)
        {
            CacheSkillButtons();
        }

        if (formationMoveImages == null || formationMoveImages.Length == 0)
        {
            CacheFormationMoveButtons();
        }
    }

    // =========================================================
    // Player UI
    // =========================================================

    public void ShowPlayerUI(PlayerUnit player)
    {
        EnsureButtonsCached();

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

                // 현재 열에서 사용 가능한지 확인 후 시각 상태(밝은 색상 / 딤드) 적용
                bool canUse = CanUseSkillAtCurrentColumn(player, skill);
                Color targetColor = canUse ? normalTurnColor : disabledSkillColor;
                SetButtonVisualState(cached, canUse, targetColor);
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
        EnsureButtonsCached();

        HideBattleUIEX();

        isSelectingFormationMove = false;

        // 전투 진행 중: 내 턴이 아닐 때는 버튼들을 끄지 않고 약간 검은색으로 변경하여 사용할 수 없음을 표현
        if (isBattle && cachedButtons != null)
        {
            bool anyActive = false;
            for (int i = 0;
                 i < cachedButtons.Length;
                 i++)
            {
                if (cachedButtons[i].root != null && cachedButtons[i].root.activeSelf)
                {
                    anyActive = true;
                    SetButtonVisualState(cachedButtons[i], false, notMyTurnColor);
                }
            }

            // 아직 아무 버튼도 켜지지 않은 상태(전투 시작 직후 적 턴인 경우)라면 기본 4개 버튼을 켜서 검은색으로 표시
            if (!anyActive)
            {
                int defaultCount = Mathf.Min(4, cachedButtons.Length);
                for (int i = 0; i < defaultCount; i++)
                {
                    if (cachedButtons[i].root != null)
                    {
                        cachedButtons[i].root.SetActive(true);
                        SetButtonVisualState(cachedButtons[i], false, notMyTurnColor);
                    }
                }
            }

            if (formationMoveButton != null)
            {
                formationMoveButton.gameObject.SetActive(true);
                SetFormationMoveButtonVisual(false, notMyTurnColor);
            }
        }
        else if (!isBattle)
        {
            // 전투가 완전히 종료되었을 때는 버튼을 완전히 비활성화
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

            if (formationMoveButton != null)
            {
                formationMoveButton.gameObject
                    .SetActive(false);
            }
        }
    }


    // =========================================================
    // Formation Move
    // =========================================================

    private void CacheFormationMoveButtons()
    {
        if (formationMoveButton != null)
        {
            formationMoveImages = formationMoveButton.GetComponentsInChildren<Image>(true);
            formationMoveTexts = formationMoveButton.GetComponentsInChildren<TMP_Text>(true);

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

            bool canMove = CanStartFormationMove(player);
            Color targetColor = canMove ? normalTurnColor : disabledSkillColor;
            SetFormationMoveButtonVisual(canMove, targetColor);
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
        isBattle = false;

        HideBattleUIEX();

        DisableBattleUnits();

        if (battleUI != null)
            battleUI.SetActive(false);
        
        HidePlayerUI();
        if(AudioManager.instance != null)
        {
            AudioManager.instance.StopBgm();
        }
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
            Save.EndCurrentRun();
            StartCoroutine(ActivateGameOverRoutine());
        }
    }

    private IEnumerator ActivateGameOverRoutine()
    {
        // 마지막 유닛의 사망 연출(0.8초)이 자연스럽게 마무리되도록 잠시 대기
        yield return new WaitForSeconds(0.6f);

        if (gameOverObject != null)
        {
            gameOverObject.SetActive(true);
            Debug.Log($"<color=red>[BattleManager]</color> 지정된 게임오버 오브젝트 '{gameOverObject.name}' 활성화 완료!");
            yield break;
        }

        // 씬 내에서 "GameOver", "GameOverUI", "Game Over" 등의 이름을 가진 오브젝트 자동 탐색
        GameObject found = GameObject.Find("GameOver");
        if (found == null) found = GameObject.Find("GameOverUI");
        if (found == null) found = GameObject.Find("GameOverCanvas");
        if (found == null) found = GameObject.Find("Game Over");

        if (found == null)
        {
            // 비활성화된 오브젝트까지 포함하여 탐색
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in allObjects)
            {
                if (go.hideFlags == HideFlags.None &&
                    (go.name.Equals("GameOver", System.StringComparison.OrdinalIgnoreCase) ||
                     go.name.Equals("GameOverUI", System.StringComparison.OrdinalIgnoreCase) ||
                     go.name.Equals("GameOverCanvas", System.StringComparison.OrdinalIgnoreCase) ||
                     go.name.Equals("Game Over", System.StringComparison.OrdinalIgnoreCase)))
                {
                    if (go.scene.IsValid() && go.scene.isLoaded)
                    {
                        found = go;
                        break;
                    }
                }
            }
        }

        if (found != null)
        {
            found.SetActive(true);
            gameOverObject = found;
            Debug.Log($"<color=red>[BattleManager]</color> 자동 탐색된 게임오버 오브젝트 '{found.name}' 활성화 완료!");
        }
        else
        {
            Debug.LogWarning("[BattleManager] 활성화할 GameOver 오브젝트를 찾을 수 없습니다. 인스펙터의 BattleManager -> GameOver Object 슬롯에 할당하거나 씬에 'GameOver' 오브젝트를 배치해주세요.");
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

    // =========================================================
    // Rearrange Party (아군 사망 시 앞으로 당겨 재정렬)
    // =========================================================

    public void RearrangeParty()
    {
        if (Party == null ||
            Party.Length == 0)
        {
            return;
        }

        if (PartyManager.instance == null ||
            PartyManager.instance.partySlots == null)
        {
            return;
        }

        // 빈 슬롯 정리 및 앞으로 당김
        PartyManager.instance.ShiftPartyForward();

        for (int i = 0; i < PartyManager.instance.partySlots.Length; i++)
        {
            Unit unit = PartyManager.instance.partySlots[i];

            if (unit != null &&
                unit.gameObject.activeInHierarchy &&
                unit.health > 0)
            {
                if (i < Party.Length && Party[i] != null)
                {
                    unit.transform.position = Party[i].position;
                    unit.transform.rotation = Party[i].rotation;
                }
            }
        }
    }
}

