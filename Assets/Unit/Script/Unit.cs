using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class Unit
{
    [Header("기본 정보")]
    public string Name;
    public bool IsEnemy;
    public int CurrentRank; // 1~4열 위치

    [Header("생존 스탯")]
    public float MaxHP;
    public float CurrentHP;
    public float Protection;    // PROT (데미지 감쇄 %)
    public float Dodge;         // 회피율
    public float DeathBlowResist = 0.67f; // 죽음의 일격 저항 (아군 전용)

    [Header("공격 스탯")]
    public int Speed;           // SPD (턴 우선순위)
    public float AccuracyMod;   // ACC (명중 보정)
    public float CritChance;    // CRIT (치명타 확률)
    public float DamageMin;     // 최소 공격력
    public float DamageMax;     // 최대 공격력

    [Header("상태 이상 저항 (Resistances)")]
    public float StunResist;    // 기절 저항
    public float BlightResist;  // 중독 저항
    public float BleedResist;   // 출혈 저항
    public float DebuffResist;  // 약화 저항
    public float MoveResist;    // 이동 저항
    public float DiseaseResist; // 질병 저항

    [Header("심리 요소 (아군 전용)")]
    public float Stress;
    public float VirtueChance = 0.25f; // 영웅적 기상 확률 (기본 25%)
    public bool IsAfflicted;           // 고통 상태 여부
    public bool IsVirtuous;            // 영웅적 기상 상태 여부

    [Header("실시간 상태 관리")]
    public bool IsDead;
    public bool IsInDeathDoor;         // 죽음의 문턱 상태 여부
    public int TempStunResistBuff;     // 기절 회복 후 일시적 저항 증가치

    // 지속 데미지 및 버프 리스트
    public List<OverTimeEffect> ActiveEffects = new List<OverTimeEffect>();
    public void TakeDamage(float amount, bool isCrit, Unit attacker)
    {
        if (IsDead) return;

        // 1. 방어력(Protection) 계산
        // PROT가 20%라면 데미지는 80%만 들어옴
        float reducedDamage = amount * (1f - (Protection / 100f));

        // 최소 데미지 1 보장
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(reducedDamage));

        CurrentHP -= finalDamage;
        Debug.Log($"{Name}이(가) {finalDamage}의 피해를 입었습니다. (치명타: {isCrit})");

        // 2. 체력 상태 체크
        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            CheckDeathStatus();
        }
    }

    private void CheckDeathStatus()
    {
        if (!IsEnemy) // 아군일 경우
        {
            if (!IsInDeathDoor)
            {
                IsInDeathDoor = true;
                Debug.Log($"{Name}이 죽음의 문턱에 들어섰습니다!");
                // 여기서 스트레스 증가 등 추가 로직 실행
            }
            else
            {
                // 죽음의 문턱 상태에서 데미지를 받으면 사망 판정 로직으로 이동
                // (실제 사망 판정은 별도의 DeathBlow 저항 굴림 후 결정)
            }
        }
        else // 적군일 경우
        {
            Die();
        }
    }

    public void TakeDotDamage(float amount, OverTimeEffect.EffectType dotType)
    {
        // 도트 데미지는 보통 방어력(PROT)을 무시합니다.
        CurrentHP -= amount;
        Debug.Log($"{Name}이 {dotType}으로 인해 {amount}의 피해를 입었습니다.");

        if (CurrentHP <= 0) CheckDeathStatus();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        CurrentHP += amount;
        if (CurrentHP > MaxHP) CurrentHP = MaxHP;

        // 치료받으면 죽음의 문턱 해제
        if (IsInDeathDoor && CurrentHP > 0)
        {
            IsInDeathDoor = false;
            Debug.Log($"{Name}이 죽음의 문턱에서 벗어났습니다.");
        }
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log($"{Name}이(가) 사망했습니다.");
    }
}

[System.Serializable]
public class OverTimeEffect
{
    public enum EffectType { Blight, Bleed, Buff, Debuff }
    public EffectType Type;
    public float Amount;      // 턴당 데미지 혹은 스탯 증감량
    public int Duration;      // 남은 턴수
    public string StatTarget; // (버프/디버프일 경우) 영향을 주는 스탯 이름
}