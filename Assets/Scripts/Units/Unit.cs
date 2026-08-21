using System.Collections.Generic;
using UnityEngine;

public enum StatusType
{
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
    Marked           // 목표지정
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
    public GameObject statusIconPrefab;
    public StatusIconData[] statusIconDatas;

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
    public bool isOxidationI { get => StatusHandler.isOxidationI; set => StatusHandler.isOxidationI = value; }
    public bool isOxidationII { get => StatusHandler.isOxidationII; set => StatusHandler.isOxidationII = value; }
    public bool isOverheat { get => StatusHandler.isOverheat; set => StatusHandler.isOverheat = value; }
    public bool isFire { get => StatusHandler.isFire; set => StatusHandler.isFire = value; }
    public bool isShortCircuit { get => StatusHandler.isShortCircuit; set => StatusHandler.isShortCircuit = value; }
    public bool isFuseBroken { get => StatusHandler.isFuseBroken; set => StatusHandler.isFuseBroken = value; }
    public bool isWeaponPollution { get => StatusHandler.isWeaponPollution; set => StatusHandler.isWeaponPollution = value; }
    public bool isOilLeak { get => StatusHandler.isOilLeak; set => StatusHandler.isOilLeak = value; }
    public bool isOilEmpty { get => StatusHandler.isOilEmpty; set => StatusHandler.isOilEmpty = value; }
    public bool isBroken { get => StatusHandler.isBroken; set => StatusHandler.isBroken = value; }
    public bool isMarked { get => StatusHandler.isMarked; set => StatusHandler.isMarked = value; }
    public int markedTurn { get => StatusHandler.markedTurn; set => StatusHandler.markedTurn = value; }

    public bool isBleeding { get => StatusHandler.isBleeding; set => StatusHandler.isBleeding = value; }
    public int bleedingCount { get => StatusHandler.bleedingCount; set => StatusHandler.bleedingCount = value; }
    public bool isStunned { get => StatusHandler.isStunned; set => StatusHandler.isStunned = value; }
    public int fireCount { get => StatusHandler.fireCount; set => StatusHandler.fireCount = value; }
    public bool isFires { get => StatusHandler.isFires; set => StatusHandler.isFires = value; }

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
                statusIconPrefab,
                statusIconDatas
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
            isStunned = false;

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
            Die();
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

    public void AddStatus(StatusType type, int turn = 0) => StatusHandler.AddStatus(type, turn);
    public void RemoveStatus(StatusType type) => StatusHandler.RemoveStatus(type);
    public void ClearStates() => StatusHandler.ClearStates();
    public void AddMark(int turn) => StatusHandler.AddMark(turn);
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