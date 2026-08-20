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
    private void Awake()
    {
        instance = this;
    }

    public void StartBattle(Room room)
    {
        isBattle = true;

        battleUI.SetActive(true);

        AudioManager.instance.PlayBattleBgm();

        TurnManager.instance.RegisterRoom(room);

        Debug.Log("전투 시작");
    }

    public void StartTurn(Unit unit)
    {
        if (unit == null)
            return;

        turnText.text =
            unit.Unitname + " TURN";

        if (unit.gameObject.layer ==
            LayerMask.NameToLayer("Player"))
        {
            PlayerUnit player =
                unit as PlayerUnit;

            if (player != null)
            {
                ShowPlayerUI(player);

                TurnManager.instance
                    .waitingForTarget = true;
            }
        }
        else
        {
            HidePlayerUI();

            TurnManager.instance
                .waitingForTarget = false;
        }
    }

    public void ShowPlayerUI(PlayerUnit player)
    {
        HidePlayerUI();

        for (int i = 0;
            i < skillButtons.Length;
            i++)
        {
            if (i >= player.skills.Count)
            {
                skillButtons[i]
                    .SetActive(false);

                continue;
            }

            SkillData skill =
                player.skills[i];

            skillButtons[i]
                .SetActive(true);

            TMP_Text text =
                skillButtons[i]
                .GetComponentInChildren<TMP_Text>();

            if (text != null)
            {
                text.text =
                    skill.skillName;
            }

            Image buttonImage =
                skillButtons[i]
                .GetComponent<Image>();

            if (buttonImage != null)
            {
                buttonImage.sprite =
                    skill.icon;

                buttonImage.enabled =
                    skill.icon != null;
            }
        }
    }

    public void HidePlayerUI()
    {
        foreach (GameObject button
            in skillButtons)
        {
            button.SetActive(false);
        }
    }

    public void SelectSkill(int index)
    {
        PlayerUnit player =
            TurnManager.instance.currentUnit
            as PlayerUnit;

        if (player == null)
            return;

        if (index < 0 ||
            index >= player.skills.Count)
            return;

        player.selectedSkill =
            player.skills[index];

        Debug.Log(
            player.selectedSkill.skillName +
            " 선택"
        );

        if (player.selectedSkill.targetType ==
            TargetType.Self)
        {
            player.SelectTarget(player);
        }
    }

    public void PlaySkillSound(SkillData skill)
    {
        if (skill == null)
            return;

        if (skill.soundEffect == null)
            return;

        if (sfxSource == null)
            return;

        sfxSource.PlayOneShot(
            skill.soundEffect
        );
    }

    public void EndPlayerAction()
    {
        HidePlayerUI();

        TurnManager.instance
            .waitingForTarget = false;

        TurnManager.instance.EndTurn();
    }

    public void EndBattle(bool win)
    {
        isBattle = false;

        battleUI.SetActive(false);

        HidePlayerUI();

        TurnManager.instance
            .waitingForTarget = false;

        if (win)
        {
            RoomManager.instance
                .ClearCurrentRoom();

            Debug.Log("승리!");
        }
        else
        {
            Debug.Log("패배!");
        }
    }
}