using UnityEngine;
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

    private void Awake()
    {
        instance = this;
    }

    // 전투 시작
    public void StartBattle(Room room)
    {

        isBattle = true;

        battleUI.SetActive(true);

        TurnManager.instance.RegisterRoom(room);

        Debug.Log("전투 시작");
    }

    // 턴 시작
    public void StartTurn(Unit unit)
    {
        if (unit == null)
            return;

        // 턴 텍스트
        turnText.text =
            unit.Unitname + " TURN";

        // 플레이어 턴
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
        // 적 턴
        else
        {
            HidePlayerUI();
        }
    }

    // 플레이어 UI 표시
    public void ShowPlayerUI(PlayerUnit player)
    {
        HidePlayerUI();

        for (int i = 0;
            i < skillButtons.Length;
            i++)
        {
            // 스킬 없으면 버튼 숨김
            if (i >= player.skills.Count)
            {
                skillButtons[i]
                    .SetActive(false);

                continue;
            }


            skillButtons[i]
                .SetActive(true);
            
            // 버튼 이름 변경
            TMP_Text text =
                skillButtons[i]
                .GetComponentInChildren<TMP_Text>();

            text.text =
                player.skills[i].skillName;
        }
    }

    // 플레이어 UI 숨김
    public void HidePlayerUI()
    {
        foreach (GameObject button
            in skillButtons)
        {
            button.SetActive(false);
        }
    }

    // 스킬 선택
    public void SelectSkill(int index)
    {
        PlayerUnit player =
            TurnManager.instance.currentUnit
            as PlayerUnit;

        if (player == null)
            return;

        // 범위 초과 방지
        if (index < 0 ||
            index >= player.skills.Count)
            return;

        // 스킬 선택
        player.selectedSkill =
            player.skills[index];

        Debug.Log(
            player.selectedSkill.skillName
            + " 선택"
        );
    }

    // 행동 종료
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

        if (win)
        {
            Debug.Log("승리!");
        }
        else
        {
            Debug.Log("패배!");
        }
    }
}