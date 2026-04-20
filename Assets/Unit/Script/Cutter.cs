using UnityEngine;

public class Cutter : Unit
{
    // Cutter만의 특화된 초기 설정을 Start에서 진행합니다.
    void Start()
    {
        // 1. 기본 정보 설정
        Name = "커터 (Cutter)";
        IsEnemy = false;
        CurrentRank = 1; // 보통 전열 딜러

        // 2. 생존 스탯 설정
        MaxHP = 25f;
        CurrentHP = MaxHP;
        Protection = 0f;      // 회피형 딜러이므로 방어력은 낮게
        Dodge = 10f;          // 기본 회피율 10%
        DeathBlowResist = 0.67f;

        // 3. 공격 스탯 설정
        Speed = 7;            // 빠른 편
        AccuracyMod = 5f;     // 명중 보정
        CritChance = 0.12f;   // 치명타 확률 12%
        DamageMin = 6f;
        DamageMax = 11f;

        // 4. 저항력 설정 (캐릭터 컨셉에 맞춰 조절)
        StunResist = 0.40f;   // 40%
        BlightResist = 0.30f; // 30%
        BleedResist = 0.50f;  // 출혈에는 강함
        DebuffResist = 0.40f;
        MoveResist = 0.40f;

        Debug.Log($"{Name}가 전장에 합류했습니다.");
    }

    // 캐릭터 특유의 특수 능력이나 행동 패턴이 필요하다면 여기에 추가
    void Update()
    {
        // 실시간 로직이 필요한 경우 (예: 스트레스에 따른 이펙트 처리 등)
    }
    public bool CanUseSlash()
    {
        return CurrentRank <= 2;
    }
}