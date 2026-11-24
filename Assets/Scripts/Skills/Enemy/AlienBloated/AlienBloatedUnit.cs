using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 외계생명체(불어터진) 전용 Enemy 서브클래스.
/// 사망 시 마지막 공격자에게 DeathVengeance 복수 공격을 발동합니다.
/// </summary>
public class AlienBloatedUnit : Enemy
{
    [Header("복수 설정")]
    [HideInInspector]
    public Unit lastAttacker;

    [Tooltip("DeathVengeance 스킬 로직 (ScriptableObject 연결)")]
    public SkillBase vengeanceLogic;

    [Tooltip("DeathVengeance 스킬 데이터 (SkillData 연결)")]
    public SkillData vengeanceSkillData;

    /// <summary>
    /// 마지막 공격자를 추적하기 위해 외부에서 호출합니다.
    /// (Enemy AI 혹은 BattleManager에서 피해를 입힐 때 호출)
    /// </summary>
    public void SetLastAttacker(Unit attacker)
    {
        if (attacker != null)
        {
            lastAttacker = attacker;
        }
    }

    public override void TakeDamage(int damage, Unit.DamageType type = Unit.DamageType.Normal)
    {
        // 피해를 받기 전 현재 공격 출처 추적 (기반 로직 위임)
        base.TakeDamage(damage, type);
    }

    public override void Die()
    {
        base.Die();

        if (lastAttacker != null &&
            lastAttacker.health > 0 &&
            vengeanceLogic != null &&
            vengeanceSkillData != null)
        {
            Debug.Log($"<color=red>[죽음의 복수 발동]</color> {Unitname}이(가) {lastAttacker.Unitname}에게 복수!");
            vengeanceLogic.Use(this, lastAttacker, vengeanceSkillData);
        }
    }
}
