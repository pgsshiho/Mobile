using System.Collections.Generic;
using UnityEngine;
public enum SkillType
{
    // 커터
    Cut, Divide, Overclock, Friction, Ready, PointAttack,
    // 스나이퍼
    WeakPointShot, AimShot, FlareShot, DataCorruption, PiercingShot, LockOn,
    // 수리기사 (Support)
    EmergencyWelding, WrenchThrow, MultiWelding, CommRepair, Recycle, WrenchSwing,
    // 벙커 (Tank)
    Taunt, DefenseStance, PowerBarrier, ForceFix, Charge, EnergyConvert
}
public class UnitBase : MonoBehaviour, ITakeDamage
{
    [Header("Core Stats")]
    [Tooltip("유닛의 최대 체력")]
    public float maxHp;
    public float currentHp;
    [Tooltip("공격력")]
    public float damage;
    [Tooltip("에너지 수치")]
    public float energy;
  
    [Header("Combat Stats")]
    [Range(0, 100)]
    public float evasionRate;      // 회피율
    [Range(0, 100)]
    public float accuracy;         // 정확도 (명중률)
    [Range(0, 100)]
    public float criticalChance;   // 크리티컬 확률 (데미지 2배)
    public float speed;            // 속도 (턴 순서 결정)

    [Header("Defense Stats")]
    public float armor;            // 장갑 (방어력)
    public float specialArmor;     // 특수 장갑 (부식/상태이상 방어)
    [Header("Status Effects")]
    [Range(0, 100)]
    [Tooltip("공격받았을 때 녹이 슬 확률")]
    public float rustChance;       // 녹 확률
    [Tooltip("녹으로 인해 발생하는 지속 데미지")]
    public float rustDamage;       // 녹 데미지
    [Header("Survival Stats")]
    [Range(0, 100)]
    [Tooltip("전력 고갈 시 디버프 무시 확률")]
    public float emergencyPowerStress;

    [Range(0, 100)]
    [Tooltip("죽음의 문턱에서 살아날 확률")]
    public float emergencySurvivalChance;
    public float hitpoisondamage;
    public bool isPoison;
    public ITargetingStrategy targetingStrategy;
    public List<GameObject> enemyList; // 현재 전투 중인 적 리스트
    public virtual void TakeDamage(float damage, float accure, float poison, float poisondamage)
    {
        hitpoisondamage = poisondamage;
        int hitRoll = Random.Range(1, 101);
        accure = accure - evasionRate;
        if (hitRoll > accure)
        {
            Debug.Log($"{gameObject.name}이(가) 공격을 회피했습니다!");
            return;
        }
        currentHp -= damage;
        if (currentHp <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        Destroy(gameObject);
    }
}