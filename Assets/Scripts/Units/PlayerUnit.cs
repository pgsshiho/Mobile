using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerUnit :
    Unit,
    IPointerClickHandler
{
    public Sprite normal;
    public Sprite attack;

    [Header("Formation Movement")]
    [Tooltip("이 유닛이 전열 방향으로 한 번에 이동할 수 있는 최대 칸 수")]
    [Min(0)] public int maxForwardMoveColumns = 1;

    [Tooltip("이 유닛이 후열 방향으로 한 번에 이동할 수 있는 최대 칸 수")]
    [Min(0)] public int maxBackwardMoveColumns = 1;

    protected override void Awake()
    {
        base.Awake();
        EnsurePointerClickSupport();
    }

    private void EnsurePointerClickSupport()
    {
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                collider.size = spriteRenderer.sprite.bounds.size;
            }
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null &&
            mainCamera.GetComponent<Physics2DRaycaster>() == null)
        {
            mainCamera.gameObject.AddComponent<Physics2DRaycaster>();
        }
    }

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

        if (BattleManager.instance != null &&
            !BattleManager.instance.CanUseSkillAtCurrentColumn(this, selectedSkill))
        {
            Debug.Log("[전투] 현재 위치에서는 이 스킬을 사용할 수 없습니다.");
            return;
        }

        if (target is Enemy enemy &&
            (BattleManager.instance == null ||
             !BattleManager.instance.CanPlayerTargetEnemy(
                 this,
                 enemy,
                 selectedSkill
             )))
        {
            Debug.Log("[전투] 현재 위치에서는 해당 적을 공격할 수 없습니다.");
            return;
        }

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
        base.AttackFocus(this.gameObject);
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
            if (BattleManager.instance != null)
            {
                BattleManager.instance.ClearEnemyTargetAvailability();
            }
            StartCoroutine(WaitSecond(1f));
            TurnManager.instance.EndTurn();
        }
        if(attack != null)
            sp.sprite = attack;
        StartCoroutine(base.WaitSecond(0.5f));
        if(normal != null)
            sp.sprite = normal;
    }

    // 다인 공격
    void AttackMultipleEnemies(int count)
    {
        int attacked = 0;
        base.AttackFocus(this.gameObject);
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
        base.AttackFocus(this.gameObject);
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
        base.AttackFocus(this.gameObject);
        if (TurnManager.instance == null || !TurnManager.instance.waitingForTarget)
            return;

        Unit current = TurnManager.instance.currentUnit;
        if (!(current is PlayerUnit) || health <= 0)
            return;

        if (BattleManager.instance != null &&
            BattleManager.instance.TryHandleFormationMoveTarget(
                (PlayerUnit)current,
                this
            ))
            return;

        SkillData skill = current.selectedSkill;
        if (skill == null) return;

        // TargetType.Ally는 아군 1명을 직접 지정하는 스킬이다.
        // 선택한 행동 유닛 자신도 포함하므로 회복/보호 스킬에 모두 사용 가능하다.
        if (skill.targetType == TargetType.Ally)
        {
            current.SelectTarget(this);
        }

    }
}
