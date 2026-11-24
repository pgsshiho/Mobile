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
    WeakestDefense,
    LowestEnergy
}

public class Enemy :
    Unit,
    IPointerClickHandler
{
    [Header("AI")]
    public EnemyTargetMode targetMode =
        EnemyTargetMode.Random;

    [Header("Initial Status Effect")]
    public StatusType initialStatus = StatusType.None;
    public int initialStatusTurns = 3;
    public int initialStatusDamage = 0;

    [Header("Pupa Metamorphosis Gimmick")]
    public bool isMetamorphosisPupa = false;
    public int pupaTurnCount = 0;
    public int turnsToMetamorphosis = 4;
    public GameObject butterflyPrefab;

    [Header("AI Delay")]
    public float attackDelay = 2.5f;
    public float endTurnDelay = 2.0f;
    private SpriteRenderer spriteRenderer;
    private Color defaultColor = Color.white;
    private readonly List<Unit> alivePlayersBuffer = new List<Unit>(4);

    // WaitForSeconds 캐싱 (GC 스파이크 방지)
    private WaitForSeconds cachedAttackDelayWait;
    private WaitForSeconds cachedEndTurnDelayWait;
    private float cachedAttackDelayVal = -1f;
    private float cachedEndTurnDelayVal = -1f;

    private WaitForSeconds GetAttackDelayWait()
    {
        if (cachedAttackDelayWait == null || !Mathf.Approximately(cachedAttackDelayVal, attackDelay))
        {
            cachedAttackDelayVal = attackDelay;
            cachedAttackDelayWait = new WaitForSeconds(attackDelay);
        }
        return cachedAttackDelayWait;
    }

    private WaitForSeconds GetEndTurnDelayWait()
    {
        if (cachedEndTurnDelayWait == null || !Mathf.Approximately(cachedEndTurnDelayVal, endTurnDelay))
        {
            cachedEndTurnDelayVal = endTurnDelay;
            cachedEndTurnDelayWait = new WaitForSeconds(endTurnDelay);
        }
        return cachedEndTurnDelayWait;
    }

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }
        EnsurePointerClickSupport();
    }

    protected virtual void Start()
    {
        if (initialStatus != StatusType.None && health > 0)
        {
            AddStatus(initialStatus, initialStatusTurns, initialStatusDamage);
            Debug.Log($"[적 초기 상태] {Unitname}에게 {initialStatus} ({initialStatusTurns}턴) 부여됨");
        }
    }

    public void SetTargetSelectable(bool selectable)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = selectable
                ? defaultColor
                : new Color(0.35f, 0.35f, 0.35f, 0.45f);
        }
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

        // 상태이상으로 사망했거나
        // 기절 / 데이터 파편화 등으로 이미 턴이 종료된 경우
        if (turnEndedBySystem)
            return;

        // 안전장치
        if (health <= 0 ||
            !gameObject.activeInHierarchy)
        {
            return;
        }

        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        yield return GetAttackDelayWait();

        // 대기하는 동안 죽었을 수 있음
        if (health <= 0 ||
            !gameObject.activeInHierarchy)
        {
            yield break;
        }

        if (TurnManager.instance == null ||
            TurnManager.instance.currentUnit != this)
        {
            yield break;
        }

        if (skills == null ||
            skills.Count <= 0)
        {
            yield return GetEndTurnDelayWait();

            if (TurnManager.instance != null &&
                TurnManager.instance.currentUnit == this)
            {
                TurnManager.instance.EndTurn();
            }

            yield break;
        }

        selectedSkill = ChooseSkill();

        if (selectedSkill != null && selectedSkill.skillLogic != null)
        {
            // 적 스킬 발동 시 상단에 스킬 이름 표시
            if (BattleManager.instance != null)
            {
                BattleManager.instance.SKillname(
                    LocalizationManager.GetLocalizedString(selectedSkill.skillName));
            }
            PlaySkillSound();
            OnSkillUsed(selectedSkill);

            switch (selectedSkill.targetType)
            {
                case TargetType.Self:
                    selectedSkill.skillLogic.Use(this, this, selectedSkill);
                    break;

                case TargetType.Ally:
                    // 적이 Ally 스킬(버프/힐 등)을 사용할 때는 적 진영 아군만 대상
                    Unit allyTarget = GetAllyTarget();
                    selectedSkill.skillLogic.Use(this, allyTarget ?? this, selectedSkill);
                    break;

                case TargetType.AllAlly:
                    // 적이 AllAlly 스킬을 사용할 때는 오직 적들(Enemy 진영 전체)에게만 적용
                    ApplyToAllAllies();
                    break;

                case TargetType.DeadAlly:
                    Unit deadAlly = GetDeadEnemyAlly();
                    if (deadAlly != null)
                    {
                        selectedSkill.skillLogic.Use(this, deadAlly, selectedSkill);
                    }
                    break;

                case TargetType.SingleEnemy:
                    Unit playerTarget = GetTarget();
                    if (playerTarget != null)
                    {
                        selectedSkill.skillLogic.Use(this, playerTarget, selectedSkill);
                    }
                    break;

                case TargetType.TwoEnemy:
                    AttackMultiplePlayers(2);
                    break;

                case TargetType.ThreeEnemy:
                    AttackMultiplePlayers(3);
                    break;

                case TargetType.AllEnemy:
                    AttackAllPlayers();
                    break;
            }
        }

        yield return GetEndTurnDelayWait();

        // 공격 도중 사망했을 경우
        if (health <= 0 ||
            !gameObject.activeInHierarchy)
        {
            yield break;
        }

        // 이미 다른 턴으로 넘어갔다면 종료
        if (TurnManager.instance != null &&
            TurnManager.instance.currentUnit == this)
        {
            TurnManager.instance.EndTurn();
        }
    }

    protected virtual SkillData ChooseSkill()
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

        List<SkillData> usableSkills = skills.FindAll(skill =>
            skill != null && !IsSkillDisabledByFuse(skill));

        return usableSkills.Count > 0
            ? usableSkills[Random.Range(0, usableSkills.Count)]
            : null;
    }

    SkillData FindSkill(TargetType type)
    {
        foreach (SkillData skill in skills)
        {
            if (skill != null &&
                skill.targetType == type &&
                !IsSkillDisabledByFuse(skill))
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

            case EnemyTargetMode.LowestEnergy:
                return GetLowestEnergyPlayer();

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

    Unit GetLowestEnergyPlayer()
    {
        Unit result = null;

        if (PartyManager.instance != null && PartyManager.instance.partySlots != null)
        {
            foreach (Unit party in PartyManager.instance.partySlots)
            {
                if (party == null || party.health <= 0)
                    continue;

                if (result == null || party.energyCurrent < result.energyCurrent)
                {
                    result = party;
                }
            }
        }

        return result ?? GetLowestHpPlayer();
    }

    List<Unit> GetAlivePlayers()
    {
        alivePlayersBuffer.Clear();

        foreach (Unit party
            in PartyManager.instance.partySlots)
        {
            if (party != null &&
                party.health > 0)
            {
                alivePlayersBuffer.Add(party);
            }
        }

        return alivePlayersBuffer;
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

    // =========================================================
    // 적 진영(아군) 전용 타겟팅 및 버프/회복 적용 (플레이어 배제)
    // =========================================================

    public static bool IsEnemyUnit(Unit u)
    {
        if (u == null) return false;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        return u is Enemy ||
               u.CompareTag("Enemy") ||
               (enemyLayer != -1 && u.gameObject.layer == enemyLayer);
    }

    public List<Unit> GetAliveAllies()
    {
        List<Unit> allies = new List<Unit>();

        if (TurnManager.instance != null && TurnManager.instance.turnList != null)
        {
            foreach (Unit u in TurnManager.instance.turnList)
            {
                if (u != null && u.health > 0 && IsEnemyUnit(u))
                {
                    allies.Add(u);
                }
            }
        }

        if (allies.Count == 0)
        {
            Enemy[] sceneEnemies = Object.FindObjectsByType<Enemy>();
            foreach (Enemy e in sceneEnemies)
            {
                if (e != null && e.health > 0 && e.gameObject.activeInHierarchy)
                {
                    allies.Add(e);
                }
            }
        }

        if (!allies.Contains(this) && health > 0)
        {
            allies.Add(this);
        }

        return allies;
    }

    public Unit GetAllyTarget()
    {
        List<Unit> allies = GetAliveAllies();
        if (allies.Count == 0) return this;

        // 체력 비율이 가장 낮은 아군 적 우선 선택 (자신 포함)
        Unit lowest = null;
        float lowestRatio = 2f;
        foreach (Unit a in allies)
        {
            if (a == null) continue;
            float ratio = a.maxHealth > 0 ? (float)a.health / a.maxHealth : 1f;
            if (ratio < lowestRatio)
            {
                lowestRatio = ratio;
                lowest = a;
            }
        }
        return lowest ?? this;
    }

    public Unit GetDeadEnemyAlly()
    {
        if (TurnManager.instance != null && TurnManager.instance.turnList != null)
        {
            foreach (Unit u in TurnManager.instance.turnList)
            {
                if (u != null && u.health <= 0 && IsEnemyUnit(u))
                {
                    return u;
                }
            }
        }

        Enemy[] sceneEnemies = Object.FindObjectsByType<Enemy>();
        foreach (Enemy e in sceneEnemies)
        {
            if (e != null && e.health <= 0)
            {
                return e;
            }
        }
        return null;
    }

    /// <summary>
    /// 적이 AllAlly 스킬 사용 시 오직 적들(Enemy 진영 전체)에게만 적용합니다.
    /// </summary>
    void ApplyToAllAllies()
    {
        List<Unit> allies = GetAliveAllies();
        foreach (Unit ally in allies)
        {
            if (ally != null && ally.health > 0 && selectedSkill != null && selectedSkill.skillLogic != null)
            {
                selectedSkill.skillLogic.Use(this, ally, selectedSkill);
            }
        }
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
        if (BattleManager.instance != null && BattleManager.instance.IsBattleIntroPlaying)
            return;

        if (TurnManager.instance == null ||
            !TurnManager.instance.waitingForTarget)
            return;

        Unit current =
            TurnManager.instance.currentUnit;

        if (current == null)
            return;

        if (!(current is PlayerUnit))
            return;

        SkillData skill =
            current.selectedSkill;

        if (skill == null)
            return;

        if (current is PlayerUnit player &&
            (BattleManager.instance == null ||
             !BattleManager.instance.CanPlayerTargetEnemy(
                 player,
                 this,
                 skill
             )))
        {
            return;
        }

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

