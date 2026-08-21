using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [Header("Battle")]
    public bool isBattle = false;

    [Header("Main UI")]
    public GameObject battleUI;

    [Header("Turn UI")]
    public TMP_Text turnText;

    [Header("Skill Buttons")]
    public GameObject[] skillButtons;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;
    public AudioClip battleBgm;

    // UI Cache to avoid repeated GetComponent/GetComponentInChildren in loops
    private struct CachedSkillButton
    {
        public GameObject root;
        public TMP_Text label;
        public Image iconImage;
    }

    private CachedSkillButton[] cachedButtons;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        CacheSkillButtons();
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
                cachedButtons[i] = new CachedSkillButton
                {
                    root = btn,
                    label = btn.GetComponentInChildren<TMP_Text>(),
                    iconImage = btn.GetComponent<Image>()
                };
            }
        }
    }

    public void StartBattle(Room room)
    {
        isBattle = true;

        if (battleUI != null)
            battleUI.SetActive(true);

        if (AudioManager.instance != null)
            AudioManager.instance.PlayBattleBgm();

        if (TurnManager.instance != null)
            TurnManager.instance.RegisterRoom(room);

        Debug.Log("전투 시작");
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
            }
            else
            {
                cached.root.SetActive(false);
            }
        }
    }

    public void HidePlayerUI()
    {
        if (cachedButtons == null)
            return;

        for (int i = 0; i < cachedButtons.Length; i++)
        {
            if (cachedButtons[i].root != null)
            {
                cachedButtons[i].root.SetActive(false);
            }
        }
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

        player.selectedSkill = player.skills[index];
        if (player.selectedSkill == null)
            return;

        Debug.Log($"{player.selectedSkill.skillName} 선택");

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
        HidePlayerUI();

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
                RoomManager.instance.ClearCurrentRoom();

            Debug.Log("승리!");
        }
        else
        {
            Debug.Log("패배!");
        }
    }
}