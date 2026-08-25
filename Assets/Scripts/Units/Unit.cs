using System.Collections.Generic;
using UnityEngine;

public enum StatusType
{
    None = -1,       // 상태이상 없음
    OxidationI,      // 산화 I
    OxidationII,     // 산화 II
    Overheat,        // 과열 I
    Fire,            // 화재
    ShortCircuit,    // 합선
    FuseBroken,      // 퓨즈 파손
    WeaponPollution, // 무장 오염
    OilLeak,         // 윤활유 누유
    OilEmpty,        // 윤활유 고갈
    Broken,          // 파손
    Marked,          // 목표지정
    Stun,            // 기절
    Bleeding         // 출혈
}

public class Unit : MonoBehaviour
{
    public enum DamageType
    {
        Normal,
        Bleed,
        Fire,
        Heal
    }

    [Header("Stat")]
    public int speed;
    public float health;
    public float maxHealth;
    public int attackPower;
    public int defensePower;
    public float criticalChance;

    [Header("Accuracy")]
    public int accuracy = 90;

    [Header("Extended Stats - OverCharge")]
    [Rename("최대 HP")]
    public int energyMax = 100;            // 최대 에너지
    [Rename("현재 HP")]
    public int energyCurrent = 100;        // 현재 에너지
    [Rename("상태이상 데미지 경감")]
    public int specialArmor = 0;           // 특수 장갑 (상태이상 피해 경감)
    [Range(0f, 1f)]
    [Rename("죽음의 일격 방어")]
    public float emergencyPower = 0.1f;    // 비상전력: 체력 0에서 생존 확률
    [Rename("녹 방어확률")]
    public int oxidationResist = 0;        // 녹 방어 확률 (%)
    [Rename("산화 2 지속 데미지 %")]
    public int oxidationDamagePercent = 3; // 산화II 지속 데미지 (%)
    [Range(0f, 1f)]
    [Rename("회피율")]
    public float dodgeChance = 0f;         // 회피율

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

    // Status Properties (Forwarding to StatusHandler for full backward compatibility)
    public bool isOxidationI     => StatusHandler.HasStatus(StatusType.OxidationI);
    public bool isOxidationII    => StatusHandler.HasStatus(StatusType.OxidationII);
    public bool isOverheat       => StatusHandler.HasStatus(StatusType.Overheat);
    public bool isFire           => StatusHandler.HasStatus(StatusType.Fire);
    public bool isShortCircuit   => StatusHandler.HasStatus(StatusType.ShortCircuit);
    public bool isFuseBroken     => StatusHandler.HasStatus(StatusType.FuseBroken);
    public bool isWeaponPollution=> StatusHandler.HasStatus(StatusType.WeaponPollution);
    public bool isOilLeak        => StatusHandler.HasStatus(StatusType.OilLeak);
    public bool isOilEmpty       => StatusHandler.HasStatus(StatusType.OilEmpty);
    public bool isBroken         => StatusHandler.HasStatus(StatusType.Broken);
    public bool isMarked         => StatusHandler.HasStatus(StatusType.Marked);
    public bool isStunned        => StatusHandler.HasStatus(StatusType.Stun);
    public bool isBleeding       => StatusHandler.HasStatus(StatusType.Bleeding);
    public int markedTurn        => StatusHandler.GetRemainingTurns(StatusType.Marked);


    protected virtual void Awake()
    {
        EnsureInitialized();
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

        if (isStunned)
        {
            Debug.Log($"{name} 기절");
            RemoveStatus(StatusType.Stun);

            if (TurnManager.instance != null)
            {
                TurnManager.instance.EndTurn();
            }
            return;
        }


        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void SelectTarget(Unit target)
    {
    }

    public virtual void TakeDamage(int damage, DamageType type = DamageType.Normal)
    {
        UIHandler.ShowDamageText(damage, type);

        health -= damage;

        if (health <= 0)
        {
            // 비상전력: 확률로 1HP 생존
            if (emergencyPower > 0f && UnityEngine.Random.value < emergencyPower)
            {
                health = 1;
                Debug.Log($"{Unitname} 비상전력 발동! 1HP 생존!");
            }
            else
            {
                Die();
            }
        }
    }

    public void Heal(int amount)
    {
        health += amount;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        UIHandler.ShowDamageText(amount, DamageType.Heal);
        Debug.Log($"{Unitname} 회복 {amount}");
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
    public void ClearStates() => StatusHandler.ClearStates();
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
}