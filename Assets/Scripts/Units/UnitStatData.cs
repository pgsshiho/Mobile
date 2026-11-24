using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitStatData", menuName = "Unit/StatData")]
public class UnitStatData : ScriptableObject
{
    [Header("=== 유닛 식별 ===")]
    public string unitName = "";

    [Header("=== 1. 체력 & 생존 (Health & Survival) ===")]
    [Tooltip("기본 최대 체력")]
    public float maxHealth = 100f;

    [Tooltip("비상전력: 죽음의 문턱(HP 0)에서 1HP로 살아날 확률 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float emergencyPower = 0.2f;

    [Header("=== 2. 통신 (Communication) ===")]
    [Tooltip("기본 통신도 (-100 ~ 100)")]
    [Range(-100, 100)]
    public int communication = 100;

    [Header("=== 3. 전투 기본 스탯 (Combat Stats) ===")]
    [Tooltip("기본 데미지 (공격력)")]
    public int attackPower = 10;

    [Tooltip("기본 장갑 (물리 방어력)")]
    public int defensePower = 5;

    [Tooltip("특수 장갑 / 상태이상 저항 (special ammo): 상태이상 부여 확률을 이 수치(%)만큼 감소시킴")]
    public int specialArmor = 0;

    /// <summary>special ammo 별칭 (상태이상 저항 수치, specialArmor와 동일값 참조)</summary>
    public int specialAmmo => specialArmor;

    [Tooltip("기본 크리티컬 확률 (%): 발동 시 데미지 2배")]
    [Range(0f, 100f)]
    public float criticalChance = 5f;

    [Tooltip("기본 정확도 (명중률 0~100%)")]
    public int accuracy = 90;

    [Tooltip("기본 회피율 (0.0 ~ 1.0): 적의 공격을 회피할 확률")]
    [Range(0f, 1f)]
    public float dodgeChance = 0.05f;

    [Tooltip("기본 속도 (턴 우선순위)")]
    public int speed = 10;

    [Header("=== 4. 에너지 (Energy) ===")]
    [Tooltip("기본 최대 에너지")]
    public int energyMax = 10;

    [Header("=== 5. 녹 / 산화 (Rust & Oxidation) ===")]
    [Tooltip("기본 녹 확률 (%): 공격/스킬 시 상대에게 녹(산화)을 부여할 확률")]
    [Range(0, 100)]
    public int rustChance = 20;

    [Tooltip("기본 산화 저항력 (%)")]
    public int oxidationResist = 0;

    [Tooltip("공격 시 상대에게 부여할 기본 산화 지속 턴 수")]
    public int oxidationTurns = 3;

    [Tooltip("공격 시 상대에게 부여할 기본 산화 턴당 고정 데미지")]
    public int oxidationDamage = 5;

    [Header("=== 6. 상태이상 부여 고유 설정 (Inflict Status Settings) ===")]
    [Tooltip("출혈 부여 시 기본 지속 턴수")]
    public int bleedInflictTurns = 3;
    [Tooltip("출혈 부여 시 턴당 기본 고정 데미지 (방어력 무시)")]
    public int bleedInflictDamage = 5;

    [Tooltip("누유/유출 부여 시 기본 지속 턴수")]
    public int oilLeakInflictTurns = 3;
    [Tooltip("누유/유출 부여 시 턴당 기본 고정 데미지 (방어력 무시, 중첩 시 증가)")]
    public int oilLeakInflictDamage = 5;

    [Tooltip("과열 부여 시 기본 지속 턴수")]
    public int overheatInflictTurns = 2;
    [Tooltip("과열 부여 시 턴당 기본 고정 데미지 (방어력 무시)")]
    public int overheatInflictDamage = 4;

    [Tooltip("화재 부여 시 기본 지속 턴수")]
    public int fireInflictTurns = 3;
    [Tooltip("화재 부여 시 턴당 기본 고정 데미지 (방어력 무시)")]
    public int fireInflictDamage = 8;

    [Tooltip("기절 부여 시 기본 지속 턴수")]
    public int stunInflictTurns = 1;

    [Tooltip("표식 부여 시 기본 지속 턴수")]
    public int markInflictTurns = 3;

    [Header("=== 7. 최대 업그레이드 레벨 제한 ===")]
    public int maxAttackLevel = 3;
    public int maxDefenseLevel = 3;
    public int maxAccurancyLevel = 3;
    public int maxMaxHPLevel = 3;

    /// <summary>
    /// 이 스탯 데이터를 대상 Unit 인스턴스에 적용합니다.
    /// </summary>
    public void ApplyTo(Unit unit, bool resetHealthAndEnergy = true)
    {
        if (unit == null) return;

        if (string.IsNullOrEmpty(unit.Unitname) && !string.IsNullOrEmpty(unitName))
            unit.Unitname = unitName;

        unit.maxHealth = maxHealth;
        if (resetHealthAndEnergy || unit.health <= 0f)
            unit.health = maxHealth;

        unit.emergencyPower = emergencyPower;
        unit.communication = communication;

        unit.attackPower = attackPower;
        unit.defensePower = defensePower;
        unit.specialArmor = specialArmor;
        unit.criticalChance = criticalChance;
        unit.accuracy = accuracy;
        unit.dodgeChance = dodgeChance;
        unit.speed = speed;

        unit.energyMax = energyMax;
        if (resetHealthAndEnergy || unit.energyCurrent <= 0)
            unit.energyCurrent = energyMax;

        unit.rustChance = rustChance;
        unit.oxidationResist = oxidationResist;
        unit.oxidationTurns = oxidationTurns;
        unit.oxidationDamage = oxidationDamage;

        // 상태이상 부여 고유 설정
        unit.bleedInflictTurns = bleedInflictTurns;
        unit.bleedInflictDamage = bleedInflictDamage;
        unit.oilLeakInflictTurns = oilLeakInflictTurns;
        unit.oilLeakInflictDamage = oilLeakInflictDamage;
        unit.overheatInflictTurns = overheatInflictTurns;
        unit.overheatInflictDamage = overheatInflictDamage;
        unit.fireInflictTurns = fireInflictTurns;
        unit.fireInflictDamage = fireInflictDamage;
        unit.stunInflictTurns = stunInflictTurns;
        unit.markInflictTurns = markInflictTurns;

        unit.maxAttackLevel = maxAttackLevel;
        unit.maxDefenseLevel = maxDefenseLevel;
        unit.maxAccurancyLevel = maxAccurancyLevel;
        unit.maxMaxHPLevel = maxMaxHPLevel;
    }
}
