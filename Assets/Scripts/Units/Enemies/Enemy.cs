using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum EnemyTargetMode
{
    Random,
    LowestHp,
    HighestHp,
    Fastest,
    WeakestDefense
}

public class Enemy :
    Unit,
    IPointerClickHandler
{
    [Header("AI")]
    public EnemyTargetMode targetMode =
        EnemyTargetMode.Random;

    [Header("AI Delay")]
    public float attackDelay = 1.5f;
    public float endTurnDelay = 1.5f;

    public override void MyTurn()
    {
        base.MyTurn();

        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(attackDelay);

        if (skills == null ||
            skills.Count <= 0)
        {
            yield return new WaitForSeconds(endTurnDelay);

            TurnManager.instance.EndTurn();
            yield break;
        }

        selectedSkill =
            ChooseSkill();

        Unit target =
            GetTarget();

        if (target != null &&
            selectedSkill != null)
        {
            PlaySkillSound();
            OnSkillUsed(selectedSkill);

            switch (selectedSkill.targetType)
            {
                case TargetType.SingleEnemy:

                    selectedSkill.skillLogic.Use(
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

                    selectedSkill.skillLogic.Use(
                        this,
                        this,
                        selectedSkill
                    );

                    break;
            }
        }

        yield return new WaitForSeconds(endTurnDelay);

        TurnManager.instance.EndTurn();
    }

    SkillData ChooseSkill()
    {
        if (health <= maxHealth * 0.3f)
        {
            SkillData selfSkill =
                FindSkill(TargetType.Self);

            if (selfSkill != null)
                return selfSkill;
        }

        int alivePlayerCount =
            CountAlivePlayers();

        if (alivePlayerCount >= 3)
        {
            SkillData allSkill =
                FindSkill(TargetType.AllEnemy);

            if (allSkill != null)
                return allSkill;
        }

        return skills[
            Random.Range(0, skills.Count)
        ];
    }

    SkillData FindSkill(TargetType type)
    {
        foreach (SkillData skill in skills)
        {
            if (skill != null &&
                skill.targetType == type)
            {
                return skill;
            }
        }

        return null;
    }

    Unit GetTarget()
    {
        switch (targetMode)
        {
            case EnemyTargetMode.LowestHp:
                return GetLowestHpPlayer();

            case EnemyTargetMode.HighestHp:
                return GetHighestHpPlayer();

            case EnemyTargetMode.Fastest:
                return GetFastestPlayer();

            case EnemyTargetMode.WeakestDefense:
                return GetWeakestDefensePlayer();

            default:
                return GetRandomPlayer();
        }
    }

    Unit GetRandomPlayer()
    {
        List<Unit> alivePlayers =
            GetAlivePlayers();

        if (alivePlayers.Count <= 0)
            return null;

        return alivePlayers[
            Random.Range(0, alivePlayers.Count)
        ];
    }

    Unit GetLowestHpPlayer()
    {
        Unit result = null;

        foreach (Unit party
            in PartyManager.instance.partySlots)
        {
            if (party == null ||
                party.health <= 0)
                continue;

            if (result == null ||
                party.health < result.health)
            {
                result = party;
            }
        }

        return result;
    }

    Unit GetHighestHpPlayer()
    {
        Unit result = null;

        foreach (Unit party
            in PartyManager.instance.partySlots)
        {
            if (party == null ||
                party.health <= 0)
                continue;

            if (result == null ||
                party.health > result.health)
            {
                result = party;
            }
        }

        return result;
    }

    Unit GetFastestPlayer()
    {
        Unit result = null;

        foreach (Unit party
            in PartyManager.instance.partySlots)
        {
            if (party == null ||
                party.health <= 0)
                continue;

            if (result == null ||
                party.GetSpeed() >
                result.GetSpeed())
            {
                result = party;
            }
        }

        return result;
    }

    Unit GetWeakestDefensePlayer()
    {
        Unit result = null;

        foreach (Unit party
            in PartyManager.instance.partySlots)
        {
            if (party == null ||
                party.health <= 0)
                continue;

            if (result == null ||
                party.GetDefensePower() <
                result.GetDefensePower())
            {
                result = party;
            }
        }

        return result;
    }

    List<Unit> GetAlivePlayers()
    {
        List<Unit> alivePlayers =
            new List<Unit>();

        foreach (Unit party
            in PartyManager.instance.partySlots)
        {
            if (party != null &&
                party.health > 0)
            {
                alivePlayers.Add(party);
            }
        }

        return alivePlayers;
    }

    int CountAlivePlayers()
    {
        int count = 0;

        foreach (Unit party
            in PartyManager.instance.partySlots)
        {
            if (party != null &&
                party.health > 0)
            {
                count++;
            }
        }

        return count;
    }

    void AttackAllPlayers()
    {
        foreach (Unit unit
            in PartyManager.instance.partySlots)
        {
            if (unit != null &&
                unit.health > 0)
            {
                selectedSkill.skillLogic.Use(
                    this,
                    unit,
                    selectedSkill
                );
            }
        }
    }

    void AttackMultiplePlayers(int count)
    {
        int attacked = 0;

        List<Unit> alivePlayers =
            GetAlivePlayers();

        while (alivePlayers.Count > 0 &&
            attacked < count)
        {
            int index =
                Random.Range(0, alivePlayers.Count);

            Unit target =
                alivePlayers[index];

            selectedSkill.skillLogic.Use(
                this,
                target,
                selectedSkill
            );

            alivePlayers.RemoveAt(index);

            attacked++;
        }
    }

    void PlaySkillSound()
    {
        if (AudioManager.instance == null)
            return;

        if (selectedSkill == null)
            return;

        AudioManager.instance
            .PlaySfx(selectedSkill.soundEffect);
    }

    public void OnPointerClick(
        PointerEventData eventData
    )
    {
        if (!TurnManager.instance
            .waitingForTarget)
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
}