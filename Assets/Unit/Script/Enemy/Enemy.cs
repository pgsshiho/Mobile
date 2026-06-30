using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Enemy :
    Unit,
    IPointerClickHandler
{
    public override void MyTurn()
    {
        base.MyTurn();

        // 랜덤 스킬 선택
        selectedSkill =
            skills[
                Random.Range(
                    0,
                    skills.Count
                )
            ];

        Unit target =
            GetRandomPlayer();

        if (target != null)
        {
            switch (
                selectedSkill.targetType
            )
            {
                case TargetType.SingleEnemy:

                    selectedSkill
                        .skillLogic
                        .Use(
                            this,
                            target,
                            selectedSkill
                        );

                    break;

                case TargetType.AllEnemy:

                    AttackAllPlayers();

                    break;

                case TargetType.TwoEnemy:

                    AttackMultiplePlayers(2);

                    break;

                case TargetType.ThreeEnemy:

                    AttackMultiplePlayers(3);

                    break;

                case TargetType.Self:

                    selectedSkill
                        .skillLogic
                        .Use(
                            this,
                            this,
                            selectedSkill
                        );

                    break;
            }
        }

        StartCoroutine(EndTurnDelay());
    }
    IEnumerator EndTurnDelay()
    {
        yield return new WaitForSeconds(3f);

        TurnManager.instance.EndTurn();
    }

    // 적 클릭
    public void OnPointerClick(
        PointerEventData eventData
    )
    {
        if (!TurnManager.instance
            .waitingForTarget)
            return;

        Unit current =
            TurnManager.instance
            .currentUnit;

        if (current == null)
            return;

        SkillData skill =
            current.selectedSkill;

        if (skill == null)
            return;

        // 적 대상 스킬만 허용
        if (
            skill.targetType ==
            TargetType.SingleEnemy
            ||
            skill.targetType ==
            TargetType.TwoEnemy
            ||
            skill.targetType ==
            TargetType.ThreeEnemy
            ||
            skill.targetType ==
            TargetType.AllEnemy
        )
        {
            current.SelectTarget(this);
        }
    }

    // 랜덤 플레이어
    Unit GetRandomPlayer()
    {
        foreach (Unit party
            in PartyManager.instance
            .partySlots)
        {
            if (
                party != null &&
                party.health > 0
            )
            {
                return party;
            }
        }

        return null;
    }

    // 전체 공격
    void AttackAllPlayers()
    {
        foreach (Unit unit
            in PartyManager.instance
            .partySlots)
        {
            if (
                unit != null &&
                unit.health > 0
            )
            {
                selectedSkill.skillLogic.Use(
                    this,
                    unit,
                    selectedSkill
                );
            }
        }
    }

    // 다인 공격
    void AttackMultiplePlayers(
        int count
    )
    {
        int attacked = 0;

        foreach (Unit unit
            in PartyManager.instance
            .partySlots)
        {
            if (
                unit != null &&
                unit.health > 0
            )
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
}