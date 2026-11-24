using System.Collections;
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

    // 전투 중 자율 행동/상태이상 처리에서 재사용한다.
    // 매번 새 List를 만들지 않아 GC 할당을 줄인다.
    private readonly List<Unit> aliveEnemies = new List<Unit>(4);
    private readonly List<Unit> aliveAllies = new List<Unit>(4);

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

        List<SkillData> usableSkills = skills.FindAll(skill =>
            skill != null && !IsSkillDisabledByFuse(skill));

        if (usableSkills.Count == 0)
        {
            Debug.LogWarning($"[퓨즈 파손] {Unitname}은(는) 사용할 수 있는 스킬이 없습니다.");
            TurnManager.instance?.EndTurn();
            return;
        }

        selectedSkill = usableSkills[Random.Range(0, usableSkills.Count)];
        Unit autoTarget = GetRandomAliveEnemy();

        Debug.LogWarning($"<color=orange>[독자 행동 발동]</color> {Unitname}이(가) 통신 통제를 벗어나 [{selectedSkill.skillName}]을(를) 즉시 시전합니다!");

        SelectTarget(autoTarget != null ? autoTarget : this);
    }

    private Unit GetRandomAliveEnemy()
    {
        if (TurnManager.instance == null) return null;
        aliveEnemies.Clear();
        foreach (Unit u in TurnManager.instance.turnList)
        {
            if (u != null && u.health > 0 && u.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                aliveEnemies.Add(u);
            }
        }
        return (aliveEnemies.Count > 0)
            ? aliveEnemies[Random.Range(0, aliveEnemies.Count)]
            : null;
    }

    private Unit GetRandomAliveAlly()
    {
        if (TurnManager.instance == null) return null;
        aliveAllies.Clear();
        foreach (Unit u in TurnManager.instance.turnList)
        {
            if (u != null && u.health > 0 && u.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                aliveAllies.Add(u);
            }
        }
        return (aliveAllies.Count > 0)
            ? aliveAllies[Random.Range(0, aliveAllies.Count)]
            : null;
    }

    // 스킬 선택
    public void SelectSkill(int index)
    {
        if (index < 0 || index >= skills.Count)
            return;

        if (IsSkillDisabledByFuse(skills[index]))
        {
            Debug.LogWarning($"[퓨즈 파손] {skills[index].skillName} 스킬은 현재 사용할 수 없습니다.");
            return;
        }

        selectedSkill = skills[index];
        Debug.Log($"{selectedSkill.skillName} 선택");
    }

    // 타겟 선택
    public override void SelectTarget(Unit target)
    {
        if (selectedSkill == null)
            return;

        if (IsSkillDisabledByFuse(selectedSkill))
        {
            Debug.LogWarning($"[퓨즈 파손] {selectedSkill.skillName} 스킬은 현재 사용할 수 없습니다.");
            selectedSkill = null;
            return;
        }

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
                 selectedSkill)))
        {
            Debug.Log("[전투] 현재 위치에서는 해당 적을 공격할 수 없습니다.");
            return;
        }

        // Logic Loop
        if (isLogicLoop &&
            target != null &&
            target.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (Random.Range(0, 100) < 50)
            {
                Unit allyTarget = GetRandomAliveAlly();

                if (allyTarget != null)
                {
                    Debug.LogWarning(
                        $"<color=magenta>[논리 오류 발동]</color> " +
                        $"{Unitname}이(가) 아군 {allyTarget.Unitname}을(를) 적으로 오인하여 공격합니다!"
                    );

                    target = allyTarget;
                }
            }
        }

        if (attack != null)
            sp.sprite = attack;

        // 자기 자신을 포커스
        base.AttackFocus(this.gameObject);

        OnSkillUsed(selectedSkill);

        // 사운드 및 스킬 이름 UI 1회 호출
        if (AudioManager.instance != null && selectedSkill.soundEffect != null)
        {
            AudioManager.instance.PlaySfx(selectedSkill.soundEffect);
        }
        if (BattleManager.instance != null)
        {
            BattleManager.instance.SKillname(LocalizationManager.GetLocalizedString(selectedSkill.skillName));
        }

        switch (selectedSkill.targetType)
        {
            case TargetType.SingleEnemy:
            case TargetType.Ally:
                selectedSkill.skillLogic.Use(this, target, selectedSkill);
                break;

            case TargetType.TwoEnemy:
                AttackMultipleEnemies(2, target);
                break;

            case TargetType.ThreeEnemy:
                AttackMultipleEnemies(3, target);
                break;

            case TargetType.AllEnemy:
                AttackAllEnemies();
                break;

            case TargetType.Self:
                selectedSkill.skillLogic.Use(this, this, selectedSkill);
                break;

            case TargetType.AllAlly:
                ApplyToAllPartyAllies();
                break;

            case TargetType.DeadAlly:
                if (target != null && target.health <= 0)
                {
                    selectedSkill.skillLogic.Use(this, target, selectedSkill);
                }
                break;
        }

        // ==========================
        // 턴 종료
        // ==========================
        BattleManager.instance.HidePlayerUI();

        if (TurnManager.instance != null)
        {
            TurnManager.instance.waitingForTarget = false;

            if (BattleManager.instance != null)
                BattleManager.instance.ClearEnemyTargetAvailability();

            TurnManager.instance.EndTurn();
        }

        // 공격 스프라이트 유지 시간
        StartCoroutine(ResetAttackSprite());
    }
    private IEnumerator ResetAttackSprite()
    {
        yield return WaitHalfSec;

        if (sp != null && normal != null)
            sp.sprite = normal;
    }

    public static bool IsPlayerUnit(Unit u)
    {
        if (u == null) return false;
        int playerLayer = LayerMask.NameToLayer("Player");
        return u is PlayerUnit ||
               u.CompareTag("Player") ||
               (playerLayer != -1 && u.gameObject.layer == playerLayer);
    }

    public static bool IsEnemyTarget(Unit u)
    {
        if (u == null) return false;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        return u is Enemy ||
               u.CompareTag("Enemy") ||
               (enemyLayer != -1 && u.gameObject.layer == enemyLayer);
    }

    // 플레이어 AllAlly 버프/회복: 오직 살아있는 플레이어 아군들에게만 적용 (적 배제)
    void ApplyToAllPartyAllies()
    {
        List<Unit> partyAllies = new List<Unit>();

        if (PartyManager.instance != null && PartyManager.instance.partySlots != null)
        {
            foreach (Unit p in PartyManager.instance.partySlots)
            {
                if (p != null && p.health > 0 && !partyAllies.Contains(p))
                {
                    partyAllies.Add(p);
                }
            }
        }

        if (TurnManager.instance != null && TurnManager.instance.turnList != null)
        {
            foreach (Unit u in TurnManager.instance.turnList)
            {
                if (u != null && u.health > 0 && IsPlayerUnit(u) && !partyAllies.Contains(u))
                {
                    partyAllies.Add(u);
                }
            }
        }

        if (!partyAllies.Contains(this) && health > 0)
        {
            partyAllies.Add(this);
        }

        foreach (Unit ally in partyAllies)
        {
            if (ally != null && ally.health > 0 && selectedSkill != null && selectedSkill.skillLogic != null)
            {
                selectedSkill.skillLogic.Use(this, ally, selectedSkill);
            }
        }
    }

    // 다인 공격: 선택한 대상(primaryTarget)을 1순위로 포함하고 나머지 인원수를 살아있는 적들 중에서 타격
    void AttackMultipleEnemies(int count, Unit primaryTarget = null)
    {
        List<Unit> validEnemies = new List<Unit>();

        if (TurnManager.instance != null && TurnManager.instance.turnList != null)
        {
            foreach (Unit unit in TurnManager.instance.turnList)
            {
                if (unit != null && unit.health > 0 && IsEnemyTarget(unit))
                {
                    validEnemies.Add(unit);
                }
            }
        }

        int attacked = 0;

        // 1. 플레이어가 직접 클릭한 적이 유효하면 먼저 타격
        if (primaryTarget != null && primaryTarget.health > 0 && IsEnemyTarget(primaryTarget))
        {
            selectedSkill.skillLogic.Use(this, primaryTarget, selectedSkill);
            validEnemies.Remove(primaryTarget);
            attacked++;
        }

        // 2. 남은 인원수만큼 다른 살아있는 적 타격
        while (validEnemies.Count > 0 && attacked < count)
        {
            Unit nextEnemy = validEnemies[0];
            validEnemies.RemoveAt(0);

            selectedSkill.skillLogic.Use(this, nextEnemy, selectedSkill);
            attacked++;
        }
    }

    // 전체 공격: 살아있는 모든 적에게 스킬 적용
    void AttackAllEnemies()
    {
        if (TurnManager.instance == null || TurnManager.instance.turnList == null) return;

        foreach (Unit unit in TurnManager.instance.turnList)
        {
            if (unit != null && unit.health > 0 && IsEnemyTarget(unit))
            {
                selectedSkill.skillLogic.Use(this, unit, selectedSkill);
            }
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (BattleManager.instance != null && BattleManager.instance.IsBattleIntroPlaying)
            return;

        if (BattleManager.instance != null)
        {
            // 1. 분수대 로봇 선택
            if (BattleManager.instance.TryHandleFountainTarget(this))
                return;

            // 2. Fixer 로봇 선택
            if (BattleManager.instance.TryHandleFixerTarget(this))
                return;

            // 3. 인벤토리 아이템 대상 선택
            if (BattleManager.instance.TryHandleItemTarget(this))
                return;
        }

        // 4. 기존 전투 타겟 선택 처리
        if (TurnManager.instance == null ||
            !TurnManager.instance.waitingForTarget)
            return;

        base.AttackFocus(gameObject);

        Unit current = TurnManager.instance.currentUnit;

        if (!(current is PlayerUnit) || health <= 0)
            return;

        // 5. 기존 교대 이동 처리
        if (BattleManager.instance != null &&
            BattleManager.instance.TryHandleFormationMoveTarget(
                (PlayerUnit)current,
                this))
            return;

        SkillData skill = current.selectedSkill;

        if (skill == null)
            return;

        // 6. 기존 아군 대상 스킬 처리
        if (skill.targetType == TargetType.Ally)
        {
            current.SelectTarget(this);
        }
    }
}
