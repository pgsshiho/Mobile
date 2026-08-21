using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerUnit :
    Unit,
    IPointerClickHandler
{
    public override void MyTurn()
    {
        base.MyTurn();
    }

    // 스킬 선택
    public void SelectSkill(int index)
    {
        // 범위 체크
        if (index < 0 ||
            index >= skills.Count)
            return;

        selectedSkill =
            skills[index];

        Debug.Log(
            selectedSkill.skillName +
            " 선택"
        );
    }

    // 타겟 선택
    public override void SelectTarget(
        Unit target
    )
    {
        if (selectedSkill == null)
            return;

        switch (selectedSkill.targetType)
        {
            // 단일 적
            case TargetType.SingleEnemy:
                AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                selectedSkill.skillLogic.Use(
                    this,
                    target,
                    selectedSkill
                );
                Debug.Log("Using Skills");
                break;

            // 아군 대상
            case TargetType.Ally:
                AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                selectedSkill.skillLogic.Use(
                    this,
                    target,
                    selectedSkill
                );
                Debug.Log("Using Skills");
                break;

            // 2인 공격
            case TargetType.TwoEnemy:
                AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                AttackMultipleEnemies(2);
                Debug.Log("Using Skills");
                break;

            // 3인 공격
            case TargetType.ThreeEnemy:
                AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                AttackMultipleEnemies(3);
                Debug.Log("Using Skills");
                break;

            // 전체 공격
            case TargetType.AllEnemy:
                AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                AttackAllEnemies();
                Debug.Log("Using Skills");
                break;

            // 자기 자신
            case TargetType.Self:
                AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                selectedSkill.skillLogic.Use(
                    this,
                    this,
                    selectedSkill
                );
                Debug.Log("Using Skills");
                break;
            case TargetType.AllAlly:
                foreach (Unit unit
                    in TurnManager.instance.turnList)
                {
                    if (unit.gameObject.layer ==
                        LayerMask.NameToLayer(
                            "Player"))
                    {
                        AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                        selectedSkill.skillLogic.Use(
                            this,
                            unit,
                            selectedSkill
                        );
                    }
                }
                Debug.Log("Using Skills");
                break;
            case TargetType.DeadAlly:
                foreach (Unit unit
                    in TurnManager.instance.turnList)
                {
                    if (unit.gameObject.layer ==
                        LayerMask.NameToLayer(
                            "Player") &&
                        unit.health <= 0)
                    {
                        AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                        selectedSkill.skillLogic.Use(
                            this,
                            unit,
                            selectedSkill
                        );
                    }
                }
                Debug.Log("Using Skills");
                break;
        }

        BattleManager.instance
            .HidePlayerUI();

        TurnManager.instance
            .waitingForTarget = false;

        TurnManager.instance
            .EndTurn();
    }
    // 다인 공격
    void AttackMultipleEnemies(
        int count
    )
    {
        int attacked = 0;

        foreach (Unit unit
            in TurnManager.instance.turnList)
        {
            if (unit.gameObject.layer ==
                LayerMask.NameToLayer(
                    "Enemy"))
            {
                selectedSkill.skillLogic.Use(
                    this,
                    unit,
                    selectedSkill
                );

                attacked++;

                if (attacked >= count)
                    break;
            }
        }
    }

    // 전체 공격
    void AttackAllEnemies()
    {
        foreach (Unit unit
            in TurnManager.instance.turnList)
        {
            if (unit.gameObject.layer ==
                LayerMask.NameToLayer(
                    "Enemy"))
            {
                selectedSkill.skillLogic.Use(
                    this,
                    unit,
                    selectedSkill
                );
            }
        }
    }

    // 아군 클릭
    public void OnPointerClick(
    PointerEventData eventData
)
    {
        if (!TurnManager.instance.waitingForTarget)
            return;

        Unit current =
            TurnManager.instance.currentUnit;

        if (current == null)
            return;

        SkillData skill =
            current.selectedSkill;

        if (skill == null)
            return;

        if (
            skill.targetType ==
            TargetType.Ally
        )
        {
            current.SelectTarget(this);
        }
    }
}