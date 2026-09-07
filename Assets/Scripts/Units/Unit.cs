using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum StatusType
{
    None = -1,          // 상태이상 없음

    // ── 1. 하드웨어적 마모 (물리적 손상) ──────────────────────────
    MetalFatigue,       // 금속 피로: 매 턴 물리 방어력 감소, 피격 시 파손도/피해 증폭
    LubricantLeak,      // 윤활유 유출: 회피율 대폭 하락 및 이동/속도 감소 (적 공격 회피 불가)
    CircuitryShort,     // 회로 단선: 스킬 사용 시 스택 누적, 3스택 이상 시 폭발

    // ── 2. 데이터 / 소프트웨어적 오염 (시스템적 문제) ───────────
    DataFragmentation,  // 데이터 파편화: 행동 시 무작위 오류(행동 취소/턴 낭비) 발생
    LogicLoop,          // 논리 오류: 매 턴 아군을 적으로 오인하거나 명령 반대 수행
    Ghosting,           // 잔상 현상: 적의 공격을 피할 때마다 명중률(정확도) 하락

    // ── 3. 기존 원소 및 상태이상 (하위 호환) ──────────────────────
    OxidationI,         // 산화 I (녹 1단계)
    OxidationII,        // 산화 II (녹 2단계 - 지속 데미지)
    Overheat,           // 과열 (지속 화염 피해)
    Fire,               // 화재 (강력한 화염 피해)
    ShortCircuit,       // 합선 (CircuitryShort 호환)
    OilLeak,            // 윤활유 누유 (LubricantLeak 호환)
    OilEmpty,           // 윤활유 고갈
    FuseBroken,         // 퓨즈 파손
    WeaponPollution,    // 무장 오염
    Broken,             // 파손
    Marked,             // 목표지정 (받는 피해 증가)
    Stun,               // 기절 (행동 불가)
    Bleeding            // 출혈 (지속 물리 피해)
}

public class Unit : MonoBehaviour
{
    public enum DamageType
    {
        Normal,
        Bleed,
        Fire,
        Heal,
        Corrosion,  // 산화 / 녹 데미지
        Electric    // 전기 / 단선 폭발
    }

    [Header("=== 1. 체력 & 생존 (Health & Survival) ===")]
    [Tooltip("최대 체력")]
    public float maxHealth = 100f;              // MaxHp: 최대 체력

    [Tooltip("현재 체력 (0이 되면 비상전력 가동 또는 사망)")]
    public float health = 100f;                 // Hp: 체력

    [Tooltip("비상전력: 죽음의 문턱(HP 0)에서 1HP로 살아날 확률 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float emergencyPower = 0.2f;         // 비상전력: 죽음의 문턱 생존율

    [Header("=== 2. 통신 & 스트레스 (Communication) ===")]
    [Tooltip("통신도 (-100 ~ 100): 0 이하 도달 시 전력다운 발동, -100 도달 시 자폭 후 비상전력 상태 전환")]
    [Range(-100, 100)]
    public int communication = 100;             // 통신도 (-100 ~ 100)

    [Tooltip("전력다운 상태 여부 (통신도 0 이하 시 활성화)")]
    public bool isPowerDown = false;

    [Tooltip("비상전력 모드 가동 여부")]
    public bool isEmergencyMode = false;

    [Tooltip("자폭 발생 완료 여부")]
    public bool hasSelfDestructed = false;

    [Header("=== 3. 전투 기본 스탯 (Combat Stats) ===")]
    [Tooltip("데미지 (기본 공격력)")]
    public int attackPower = 10;                // 데미지: 기본 공격력

    [Tooltip("장갑 (물리 방어력)")]
    public int defensePower = 5;                // 장갑: 물리 피해 감소

    [Tooltip("특수 장갑 (녹/산화, 도트 등 특수 피해 방어력)")]
    public int specialArmor = 0;                // 특수 장갑: 특수 데미지 경감

    [Tooltip("크리티컬 확률 (%): 발동 시 데미지 2배")]
    [Range(0f, 100f)]
    public float criticalChance = 5f;           // 크리티컬: 치명타 확률

    [Tooltip("정확도 (명중률 0~100%)")]
    [Range(0, 100)]
    public int accuracy = 90;                   // 정확도: 명중률

    [Tooltip("회피율 (0.0 ~ 1.0): 적의 공격을 회피할 확률")]
    [Range(0f, 1f)]
    public float dodgeChance = 0.05f;           // 회피율: 공격 회피 확률

    [Tooltip("속도 (누가 먼저 턴을 가져갈지 결정)")]
    public int speed = 10;                      // 속도: 턴 우선순위

    [Header("=== 4. 에너지 (Energy) ===")]
    [Tooltip("최대 에너지")]
    public int energyMax = 100;                 // 최대 에너지
    [Tooltip("현재 에너지")]
    public int energyCurrent = 100;             // 현재 에너지

    [Header("=== 5. 녹 / 산화 (Rust & Oxidation) ===")]
    [Tooltip("녹 확률 (%): 공격/스킬 시 상대에게 녹(산화)을 부여할 확률")]
    [Range(0, 100)]
    public int rustChance = 20;                 // 녹 확률

    [Tooltip("녹 데미지: 녹(산화) 도트 데미지")]
    public int rustDamage = 5;                  // 녹 데미지

    [Tooltip("산화 저항력 (%)")]
    public int oxidationResist = 0;             // 녹 방어 확률

    [Tooltip("산화 II 지속 데미지 %")]
    public int oxidationDamagePercent = 3;      // 산화II 지속 데미지 (%)

    [Header("=== 6. 상태이상 내부 누적 스택 (Runtime Status Stacks) ===")]
    [Tooltip("회로 단선 스택 (3스택 이상 누적 시 폭발)")]
    public int circuitShortStacks = 0;

    [Tooltip("잔상 현상으로 인한 명중률(정확도) 누적 페널티")]
    public int ghostingMissPenalty = 0;

    [Header("Skill")]
    public List<SkillData> skills = new List<SkillData>();
    public SkillData selectedSkill;

    [Header("Buff & Debuff")]
    public List<BuffRuntime> buffs = new List<BuffRuntime>();
    public List<DebuffRuntime> debuffs = new List<DebuffRuntime>();

    [Header("UI")]
    public GameObject myturnUI;
    public GameObject damageTextPrefab;
    public Transform damageTextSpawnPoint;
    public GameObject HPBar;

    [Header("Upgrade")]
    public int attackLevel = 0;
    public int defenseLevel = 0;
    public int maxAttackLevel = 10;
    public int maxDefenseLevel = 10;
    public bool accuracyupgrade = false;
    public string Unitname = "";

    [Header("Status Icon")]
    public Transform statusIconParent;
    public StatusIconSet statusIconSet;

    // Subsystem Handlers
    private UnitBuffHandler buffHandler;
    private UnitStatusHandler statusHandler;
    private UnitUIHandler uiHandler;
    private Slider healthSlider;

    public SpriteRenderer sp;


    public UnitBuffHandler BuffHandler
    {
        get
        {
            EnsureInitialized();
            return buffHandler;
        }
    }

    public UnitStatusHandler StatusHandler
    {
        get
        {
            EnsureInitialized();
            return statusHandler;
        }
    }

    public UnitUIHandler UIHandler
    {
        get
        {
            EnsureInitialized();
            return uiHandler;
        }
    }

    // ── 상태이상 프로퍼티 (Forwarding) ────────────────────────────
    public bool isMetalFatigue      => StatusHandler.HasStatus(StatusType.MetalFatigue);
    public bool isLubricantLeak     => StatusHandler.HasStatus(StatusType.LubricantLeak) || StatusHandler.HasStatus(StatusType.OilLeak);
    public bool isCircuitryShort    => StatusHandler.HasStatus(StatusType.CircuitryShort) || StatusHandler.HasStatus(StatusType.ShortCircuit);
    public bool isDataFragmentation => StatusHandler.HasStatus(StatusType.DataFragmentation);
    public bool isLogicLoop         => StatusHandler.HasStatus(StatusType.LogicLoop);
    public bool isGhosting          => StatusHandler.HasStatus(StatusType.Ghosting);

    public bool isOxidationI        => StatusHandler.HasStatus(StatusType.OxidationI);
    public bool isOxidationII       => StatusHandler.HasStatus(StatusType.OxidationII);
    public bool isOverheat          => StatusHandler.HasStatus(StatusType.Overheat);
    public bool isFire              => StatusHandler.HasStatus(StatusType.Fire);
    public bool isShortCircuit      => isCircuitryShort;
    public bool isFuseBroken        => StatusHandler.HasStatus(StatusType.FuseBroken);
    public bool isWeaponPollution   => StatusHandler.HasStatus(StatusType.WeaponPollution);
    public bool isOilLeak           => isLubricantLeak;
    public bool isOilEmpty          => StatusHandler.HasStatus(StatusType.OilEmpty);
    public bool isBroken            => StatusHandler.HasStatus(StatusType.Broken);
    public bool isMarked            => StatusHandler.HasStatus(StatusType.Marked);
    public bool isStunned           => StatusHandler.HasStatus(StatusType.Stun);
    public bool isBleeding          => StatusHandler.HasStatus(StatusType.Bleeding);
    public int markedTurn           => StatusHandler.GetRemainingTurns(StatusType.Marked);

    protected virtual void Awake()
    {
        sp = GetComponent<SpriteRenderer>();
        EnsureInitialized();
        CacheHealthBar();
        UpdateHealthBar();
    }

    protected virtual void LateUpdate()
    {
        // 일부 스킬이 health 값을 직접 변경하므로,
        // 매 프레임 UI를 동기화해 모든 피해/회복을 반영한다.
        UpdateHealthBar();
    }

    private void CacheHealthBar()
    {
        if (HPBar != null)
        {
            healthSlider = HPBar.GetComponent<Slider>();
            if (healthSlider == null)
            {
                healthSlider = HPBar.GetComponentInChildren<Slider>(true);
            }
        }

        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>(true);
        }

        if (HPBar == null && healthSlider != null)
        {
            HPBar = healthSlider.gameObject;
        }
    }

    public void UpdateHealthBar()
    {
        if (healthSlider == null)
        {
            CacheHealthBar();
        }

        if (healthSlider == null)
            return;

        healthSlider.minValue = 0f;
        healthSlider.maxValue = Mathf.Max(1f, maxHealth);
        healthSlider.SetValueWithoutNotify(
            Mathf.Clamp(health, 0f, healthSlider.maxValue)
        );
    }

    public void EnsureInitialized()
    {
        if (uiHandler == null)
        {
            uiHandler = new UnitUIHandler(
                this,
                damageTextPrefab,
                damageTextSpawnPoint,
                myturnUI,
                statusIconParent,
                statusIconSet
            );
        }

        if (buffHandler == null)
        {
            buffHandler = new UnitBuffHandler(this, buffs, debuffs);
        }

        if (statusHandler == null)
        {
            statusHandler = new UnitStatusHandler(this, uiHandler);
        }
    }

    // ==========================
    // Turn & Lifecycle
    // ==========================

    public virtual void MyTurn()
    {
        UIHandler.SetTurnUI(true);

        BuffHandler.BuffTurn();
        BuffHandler.DebuffTurn();
        StatusHandler.TickTurn();

        // 1. 기절 검사
        if (isStunned)
        {
            Debug.Log($"{Unitname} 기절로 행동 불가");
            RemoveStatus(StatusType.Stun);

            if (TurnManager.instance != null)
            {
                TurnManager.instance.EndTurn();
            }
            return;
        }

        // 2. 데이터 파편화 검사 (35% 확률로 오류 카드/행동 캔슬)
        if (isDataFragmentation && UnityEngine.Random.Range(0, 100) < 35)
        {
            Debug.LogWarning($"<color=cyan>[데이터 파편화]</color> {Unitname} 시스템 오류로 행동 실패 (턴 스킵)!");
            if (TurnManager.instance != null)
            {
                TurnManager.instance.EndTurn();
            }
            return;
        }

        // 3. 사망 검사
        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 스킬을 사용할 때 호출되어 회로 단선 등의 스택을 누적하고 폭발을 체크합니다.
    /// </summary>
    public virtual void OnSkillUsed(SkillData skill)
    {
        if (isCircuitryShort)
        {
            circuitShortStacks++;
            Debug.LogWarning($"[회로 단선] {Unitname} 스킬 사용으로 배선 과부하! (스택: {circuitShortStacks}/3)");

            if (circuitShortStacks >= 3)
            {
                ExplodeCircuitShort();
            }
        }
    }

    /// <summary>
    /// 회로 단선 3스택 도달 시 과부하 폭발 발생
    /// </summary>
    private void ExplodeCircuitShort()
    {
        Debug.LogError($"<color=red>[회로 단선 폭발]</color> {Unitname} 배선 단선 한계 초과로 내부 대폭발 발생!");
        circuitShortStacks = 0;
        RemoveStatus(StatusType.CircuitryShort);
        RemoveStatus(StatusType.ShortCircuit);

        // 최대 체력의 80%에 달하는 치명적 전기 폭발 피해
        int explosionDmg = Mathf.RoundToInt(maxHealth * 0.8f);
        TakeDamage(explosionDmg, DamageType.Electric);
    }

    public virtual void SelectTarget(Unit target)
    {
    }
    // =========================================================
    // Focus 연출 메서드 (Action 기반 수정)
    // =========================================================
    public virtual void AttackFocus(GameObject Self)
    {
        StartCoroutine(AttackFocusSequence(Self));
    }

    private System.Collections.IEnumerator AttackFocusSequence(GameObject Self)
    {
        // 1. 포커스 인 이벤트 요청
        FocusManager.RequestFocusIn?.Invoke(Self);

        // 2. FocusManager의 연출 시간(기본 1초, 필요에 따라 조정)만큼 대기
        // (IsTweening 값을 검사하고 싶다면 FocusManager의 duration 스펙에 맞춰 대기합니다)
        yield return new WaitForSeconds(1.0f);

        // 3. 공격 연출/동작 수행 시간 추가 (필요 시)
        yield return new WaitForSeconds(0.5f);

        // 4. 포커스 아웃 이벤트 요청
        FocusManager.RequestFocusOut?.Invoke();
    }
    public virtual void TakeDamage(int damage, DamageType type = DamageType.Normal)
    {
        UIHandler.ShowDamageText(damage, type);

        health -= damage;

        if (health <= 0)
        {
            // 체력 0 도달 시: 비상전력 확률로 1HP 생존 (비상전력 모드 가동)
            if (emergencyPower > 0f && UnityEngine.Random.value < emergencyPower)
            {
                health = 1;
                isEmergencyMode = true;
                Debug.Log($"<color=yellow>[비상전력 가동!]</color> {Unitname} 죽음의 문턱에서 1HP로 생존!");
            }
            else
            {
                Die();
            }
        }
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        if(health > 1 && isEmergencyMode)
        {
            isEmergencyMode = false;
            Debug.Log($"<color=yellow>[비상전력 해제]</color> {Unitname} 체력 회복으로 비상전력 모드 종료!");
        }
    }

    /// <summary>
    /// 통신도를 변경합니다 (범위: -100 ~ 100).
    /// - 0 이하 도달: 전력다운 상태 진입 및 무작위 상태이상/효과 획득
    /// - -100 도달: 기체 자폭 발생 후 비상전력 상태(1 HP)로 전환
    /// </summary>
    public void ModifyCommunication(int delta)
    {
        int prev = communication;
        communication = Mathf.Clamp(communication + delta, -100, 100);

        Debug.Log($"[통신도 변화] {Unitname}: {prev} ➔ {communication}");

        // 1. 통신도 0 이하 진입 시: 전력다운 발동
        if (prev > 0 && communication <= 0 && !isPowerDown)
        {
            isPowerDown = true;
            TriggerPowerDownEffects();
        }
        else if (communication > 0)
        {
            isPowerDown = false;
        }

        // 2. 통신도 -100 도달 시: 자폭 발생 ➔ 비상전력 상태로 강제 전환
        if (communication <= -100 && !hasSelfDestructed)
        {
            TriggerSelfDestructionEmergencyPower();
        }
    }

    /// <summary>
    /// 통신도가 0 이하에 도달했을 때 전력다운 상태가 되며 무작위 효과를 획득합니다.
    /// </summary>
    public void TriggerPowerDownEffects()
    {
        Debug.LogWarning($"<color=orange>[전력다운(Power Down) 발생!]</color> {Unitname}의 통신도가 0에 도달하여 전력다운 상태로 진입합니다!");

        int randomEffect = UnityEngine.Random.Range(0, 4);
        switch (randomEffect)
        {
            case 0:
                AddStatus(StatusType.Overheat, 3);
                AddStatus(StatusType.CircuitryShort, 3);
                Debug.Log($"[전력다운 효과] {Unitname} 시스템 과부하로 과열 및 회로 단선 발생!");
                break;
            case 1:
                AddStatus(StatusType.MetalFatigue, 4);
                Debug.Log($"[전력다운 효과] {Unitname} 충격으로 장갑 금속 피로 발생!");
                break;
            case 2:
                AddStatus(StatusType.Ghosting, 3);
                Debug.Log($"[전력다운 효과] {Unitname} 광학 센서 잔상 현상 발생!");
                break;
            case 3:
                AddStatus(StatusType.DataFragmentation, 3);
                Debug.Log($"[전력다운 효과] {Unitname} 메인 메모리 손상으로 데이터 파편화 발생!");
                break;
        }
    }

    /// <summary>
    /// 통신도가 -100에 도달했을 때 기체 자폭으로 인해 비상전력 상태(1 HP)로 전환됩니다.
    /// </summary>
    public void TriggerSelfDestructionEmergencyPower()
    {
        hasSelfDestructed = true;
        isEmergencyMode = true;
        health = 1f;

        Debug.LogError($"<color=red>[자폭 및 비상전력 가동!]</color> {Unitname} 통신도 -100 도달로 기체 자폭 발생! ➔ 비상전력 모드(1 HP)로 긴급 전환!");
    }

    /// <summary>
    /// 통신도가 0 이하(전력다운 상태)일 때 통신 마비로 인한 독자 행동 검사
    /// (100 ~ 1 구간에서는 독자 행동을 하지 않습니다)
    /// </summary>
    public virtual bool CheckCommunicationStress()
    {
        // 100 ~ 1 구간: 정상 조작 가능 (독자 행동 발생하지 않음)
        // 0 이하(전력다운 상태): 통신 마비로 인해 독자 행동 발생
        if (communication <= 0)
        {
            int stressChance = Mathf.Clamp(50 + Mathf.Abs(communication) / 2, 50, 90);
            if (UnityEngine.Random.Range(0, 100) < stressChance)
            {
                Debug.LogWarning($"<color=orange>[통신 마비 / 제어불능]</color> {Unitname} 전력다운 및 통신 마비로 독자 행동 개시! (통신도: {communication})");
                return true;
            }
        }
        return false;
    }

    public void Heal(float amount)
    {
        int finalHealAmount = Mathf.RoundToInt(amount * GetHealMultiplier());
        health += finalHealAmount;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        UIHandler.ShowDamageText(finalHealAmount, DamageType.Heal);
        Debug.Log($"{Unitname} 회복 {finalHealAmount}");
    }

    public virtual void Die()
    {
        Debug.Log($"{Unitname} 사망");

        if (TurnManager.instance != null)
        {
            TurnManager.instance.RemoveUnit(this);
        }

        if (PartyManager.instance != null)
        {
            PartyManager.instance.Remove(this);
        }

        UIHandler.SetTurnUI(false);
        gameObject.SetActive(false);
    }

    // ==========================
    // Buff & Debuff Delegation
    // ==========================

    public void AddBuff(BuffData buff) => BuffHandler.AddBuff(buff);
    public void ClearBuffs() => BuffHandler.ClearBuffs();
    public void AddDebuff(DebuffData debuff) => BuffHandler.AddDebuff(debuff);
    public void ClearDebuffs() => BuffHandler.ClearDebuffs();

    // ==========================
    // Status Delegation
    // ==========================

    public void AddStatus(StatusType type, int turns = 3) => StatusHandler.AddStatus(type, turns);
    public void RemoveStatus(StatusType type) => StatusHandler.RemoveStatus(type);
    public void ClearStates()
    {
        circuitShortStacks = 0;
        ghostingMissPenalty = 0;
        StatusHandler.ClearStates();
    }
    public void AddMark(int turns) => StatusHandler.AddMark(turns);
    public bool HasStatus(StatusType type) => StatusHandler.HasStatus(type);
    public int GetStatusTurns(StatusType type) => StatusHandler.GetRemainingTurns(type);
    public void AddStatusIcon(StatusType type) => UIHandler.AddStatusIcon(type);
    public void RemoveStatusIcon(StatusType type) => UIHandler.RemoveStatusIcon(type);

    // ==========================
    // Stat & Combat Delegation
    // ==========================

    public int GetSpeed() => CombatCalculator.GetSpeed(this);
    public int GetAttackPower() => CombatCalculator.GetAttackPower(this);
    public int GetDefensePower() => CombatCalculator.GetDefensePower(this);
    public float GetCriticalChance() => CombatCalculator.GetCriticalChance(this);
    public float GetHealMultiplier() => CombatCalculator.GetHealMultiplier(this);
    public int GetAccuracy() => CombatCalculator.GetAccuracy(this);

    public int CalculateDamage(Unit target, SkillData skill) => CombatCalculator.CalculateDamage(this, target, skill);
    public bool CheckHit(Unit target, SkillData skill) => CombatCalculator.CheckHit(this, target, skill);

    public IEnumerator WaitSecond(float second)
    {
        yield return new WaitForSeconds(second);
    }
}
