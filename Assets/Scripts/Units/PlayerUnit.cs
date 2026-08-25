using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerUnit :
    Unit,
    IPointerClickHandler
{
    public override void MyTurn()
    {
        base.MyTurn();

        // 행동 불능 상태(기절 등)이면 조기 종료
        if (isStunned || health <= 0)
            return;

        // 통신 불량(스트레스) 검사: 통신도가 낮으면 지시를 무시하고 독자 행동
        if (CheckCommunicationStress())
        {
            PerformAutonomousAction();
        }
    }

    /// <summary>
    /// 통신 불량 시 무작위 스킬을 선택하여 임의의 대상에게 발동
    /// </summary>
    public void PerformAutonomousAction()
    {
        if (skills == null || skills.Count == 0)
        {
            Debug.Log($"{Unitname} 혼란으로 대기");
            if (TurnManager.instance != null) TurnManager.instance.EndTurn();
            return;
        }

        selectedSkill = skills[Random.Range(0, skills.Count)];
        Unit autoTarget = GetRandomAliveEnemy();

        Debug.LogWarning($"<color=orange>[독자 행동 발동]</color> {Unitname}이(가) 통신 통제를 벗어나 [{selectedSkill.skillName}]을(를) 즉시 시전합니다!");

        SelectTarget(autoTarget != null ? autoTarget : this);
    }

    private Unit GetRandomAliveEnemy()
    {
        if (TurnManager.instance == null) return null;
        List<Unit> enemies = new List<Unit>();
        foreach (Unit u in TurnManager.instance.turnList)
        {
            if (u != null && u.health > 0 && u.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                enemies.Add(u);
            }
        }
        return (enemies.Count > 0) ? enemies[Random.Range(0, enemies.Count)] : null;
    }

    private Unit GetRandomAliveAlly()
    {
        if (TurnManager.instance == null) return null;
        List<Unit> allies = new List<Unit>();
        foreach (Unit u in TurnManager.instance.turnList)
        {
            if (u != null && u.health > 0 && u.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                allies.Add(u);
            }
        }
        return (allies.Count > 0) ? allies[Random.Range(0, allies.Count)] : null;
    }

    // 스킬 선택
    public void SelectSkill(int index)
    {
        if (index < 0 || index >= skills.Count)
            return;

        selectedSkill = skills[index];
        Debug.Log($"{selectedSkill.skillName} 선택");
    }

    // 타겟 선택
    public override void SelectTarget(Unit target)
    {
        if (selectedSkill == null)
            return;

        // 논리 오류(Logic Loop) 검사: 50% 확률로 아군을 적군으로 오인하여 아군에게 스킬 시전!
        if (isLogicLoop && target != null && target.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (Random.Range(0, 100) < 50)
            {
                Unit allyTarget = GetRandomAliveAlly();
                if (allyTarget != null)
                {
                    Debug.LogWarning($"<color=magenta>[논리 오류 발동]</color> {Unitname}이(가) 아군 {allyTarget.Unitname}을(를) 적으로 오인하여 공격합니다!");
                    target = allyTarget;
                }
            }
        }

        // 스킬 사용 트리거 (회로 단선 등 체크)
        OnSkillUsed(selectedSkill);

        switch (selectedSkill.targetType)
        {
            // 단일 적
            case TargetType.SingleEnemy:
                AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                selectedSkill.skillLogic.Use(this, target, selectedSkill);
                break;

            // 아군 대상
            case TargetType.Ally:
                AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                selectedSkill.skillLogic.Use(this, target, selectedSkill);
                break;

            // 2인 공격
            case TargetType.TwoEnemy:
                AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                AttackMultipleEnemies(2);
                break;

            // 3인 공격
            case TargetType.ThreeEnemy:
                AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                AttackMultipleEnemies(3);
                break;

            // 전체 공격
            case TargetType.AllEnemy:
                AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                AttackAllEnemies();
                break;

            // 자기 자신
            case TargetType.Self:
                AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                selectedSkill.skillLogic.Use(this, this, selectedSkill);
                break;

            case TargetType.AllAlly:
                foreach (Unit unit in TurnManager.instance.turnList)
                {
                    if (unit != null && unit.health > 0 && unit.gameObject.layer == LayerMask.NameToLayer("Player"))
                    {
                        AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                        selectedSkill.skillLogic.Use(this, unit, selectedSkill);
                    }
                }
                break;

            case TargetType.DeadAlly:
                foreach (Unit unit in TurnManager.instance.turnList)
                {
                    if (unit != null && unit.health <= 0 && unit.gameObject.layer == LayerMask.NameToLayer("Player"))
                    {
                        AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
                        selectedSkill.skillLogic.Use(this, unit, selectedSkill);
                    }
                }
                break;
        }

        BattleManager.instance.HidePlayerUI();

        if (TurnManager.instance != null)
        {
            TurnManager.instance.waitingForTarget = false;
            TurnManager.instance.EndTurn();
        }
    }

    // 다인 공격
    void AttackMultipleEnemies(int count)
    {
        int attacked = 0;

        foreach (Unit unit in TurnManager.instance.turnList)
        {
            if (unit != null && unit.health > 0 && unit.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                selectedSkill.skillLogic.Use(this, unit, selectedSkill);
                attacked++;

                if (attacked >= count)
                    break;
            }
        }
    }

    // 전체 공격
    void AttackAllEnemies()
    {
        foreach (Unit unit in TurnManager.instance.turnList)
        {
            if (unit != null && unit.health > 0 && unit.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                selectedSkill.skillLogic.Use(this, unit, selectedSkill);
            }
        }
    }

    // 아군 클릭
    public void OnPointerClick(PointerEventData eventData)
    {
        if (TurnManager.instance == null || !TurnManager.instance.waitingForTarget)
            return;

        Unit current = TurnManager.instance.currentUnit;
        if (current == null) return;

        SkillData skill = current.selectedSkill;
        if (skill == null) return;

        if (skill.targetType == TargetType.Ally)
        {
            current.SelectTarget(this);
        }
    }
}