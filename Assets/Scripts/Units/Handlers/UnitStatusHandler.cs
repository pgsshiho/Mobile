using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 상태이상을 턴 카운터로 관리합니다.
/// - 양수 N : N턴 후 자동 해제
/// - -1     : 무한 지속 (아이템/스킬로만 해제)
/// AddStatus(type, turns) 로 부여, TickTurn() 마다 카운트 감소 및 틱 데미지/효과 처리.
/// </summary>
public class UnitStatusHandler
{
    private readonly Unit owner;
    private readonly UnitUIHandler uiHandler;

    // 상태이상별 남은 턴 수 (-1 = 무한, 0 이하 = 없음)
    private readonly Dictionary<StatusType, int> statusTurns = new Dictionary<StatusType, int>();

    // ────────────────────────────────────────────
    // 읽기 프로퍼티 (Unit.cs 에서 참조)
    // ────────────────────────────────────────────
    public bool HasStatus(StatusType type) =>
        statusTurns.TryGetValue(type, out int t) && t != 0;

    public int GetRemainingTurns(StatusType type) =>
        statusTurns.TryGetValue(type, out int t) ? t : 0;

    public UnitStatusHandler(Unit owner, UnitUIHandler uiHandler)
    {
        this.owner = owner;
        this.uiHandler = uiHandler;
    }

    // ────────────────────────────────────────────
    // 부여
    // turns : 지속 턴 수, -1 이면 무한
    // ────────────────────────────────────────────
    public void AddStatus(StatusType type, int turns = 3)
    {
        if (turns == 0) return;

        switch (type)
        {
            // 산화 I이 이미 있으면 II로 전이
            case StatusType.OxidationI:
                if (HasStatus(StatusType.OxidationI))
                {
                    RemoveStatus(StatusType.OxidationI);
                    SetStatus(StatusType.OxidationII, turns);
                }
                else
                {
                    SetStatus(StatusType.OxidationI, turns);
                }
                break;

            // 산화 II는 산화 I을 덮어씀
            case StatusType.OxidationII:
                if (HasStatus(StatusType.OxidationI))
                {
                    InternalRemove(StatusType.OxidationI);
                }
                SetStatus(StatusType.OxidationII, turns);
                break;

            // 화재는 과열을 흡수
            case StatusType.Fire:
                if (HasStatus(StatusType.Overheat))
                {
                    InternalRemove(StatusType.Overheat);
                }
                SetStatus(StatusType.Fire, turns);
                break;

            // 윤활유 유출 및 고갈
            case StatusType.LubricantLeak:
            case StatusType.OilLeak:
                SetStatus(StatusType.LubricantLeak, turns);
                SetStatus(StatusType.OilLeak, turns);
                break;

            case StatusType.OilEmpty:
                InternalRemove(StatusType.LubricantLeak);
                InternalRemove(StatusType.OilLeak);
                SetStatus(StatusType.OilEmpty, turns);
                break;

            // 회로 단선 및 합선
            case StatusType.CircuitryShort:
            case StatusType.ShortCircuit:
                SetStatus(StatusType.CircuitryShort, turns);
                SetStatus(StatusType.ShortCircuit, turns);
                break;

            default:
                SetStatus(type, turns);
                break;
        }
    }

    // ────────────────────────────────────────────
    // 해제
    // ────────────────────────────────────────────
    public void RemoveStatus(StatusType type)
    {
        if (!statusTurns.ContainsKey(type)) return;
        InternalRemove(type);
    }

    private void InternalRemove(StatusType type)
    {
        statusTurns.Remove(type);
        uiHandler?.RemoveStatusIcon(type);

        // 연관 스택 초기화
        if (type == StatusType.CircuitryShort || type == StatusType.ShortCircuit)
        {
            owner.circuitShortStacks = 0;
        }
        else if (type == StatusType.Ghosting)
        {
            owner.ghostingMissPenalty = 0;
        }
    }

    // ────────────────────────────────────────────
    // 내부 세터 (기존 값보다 큰 턴 수만 갱신)
    // ────────────────────────────────────────────
    private void SetStatus(StatusType type, int turns)
    {
        bool isNew = !statusTurns.ContainsKey(type) || statusTurns[type] == 0;

        if (!isNew && statusTurns[type] == -1 && turns != -1) return;
        if (!isNew && turns != -1 && statusTurns[type] >= turns) return;

        statusTurns[type] = turns;

        if (isNew)
        {
            uiHandler?.AddStatusIcon(type);
            Debug.Log($"{owner.Unitname} [{type}] 부여 (지속: {(turns == -1 ? "무한" : turns + "턴")})");
        }
        else
        {
            Debug.Log($"{owner.Unitname} [{type}] 갱신 → {(turns == -1 ? "무한" : turns + "턴")}");
        }
    }

    // ────────────────────────────────────────────
    // 전체 초기화
    // ────────────────────────────────────────────
    public void ClearStates()
    {
        statusTurns.Clear();
        uiHandler?.ClearStatusIcons();
    }

    // ────────────────────────────────────────────
    // 매 턴 처리 (MyTurn에서 호출)
    // ────────────────────────────────────────────
    public void TickTurn()
    {
        OxidationTick();
        OverheatTick();
        FireTick();
        CircuitryShortTick();
        LubricantLeakTick();
        OilEmptyTick();
        MetalFatigueTick();
        BleedingTick();

        TickDownAll();
    }

    // ────────────────────────────────────────────
    // 카운터 감소 & 자동 해제
    // ────────────────────────────────────────────
    private void TickDownAll()
    {
        var types = new List<StatusType>(statusTurns.Keys);
        foreach (var type in types)
        {
            if (!statusTurns.ContainsKey(type)) continue;
            if (statusTurns[type] == -1) continue; // 무한 지속

            statusTurns[type]--;

            if (statusTurns[type] <= 0)
            {
                InternalRemove(type);
                Debug.Log($"{owner.Unitname} [{type}] 상태이상 해제 (턴 종료)");
            }
        }
    }

    // ────────────────────────────────────────────
    // 틱 데미지 & 상태이상 메커니즘
    // ────────────────────────────────────────────

    // 산화 II : maxHp × oxidationDamagePercent% 도트 (특수 장갑으로 경감)
    private void OxidationTick()
    {
        if (!HasStatus(StatusType.OxidationII)) return;

        float pct = owner.oxidationDamagePercent / 100f;
        int baseDmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * pct) + owner.rustDamage);
        int finalDmg = Mathf.Max(1, baseDmg - owner.specialArmor);

        owner.TakeDamage(finalDmg, Unit.DamageType.Corrosion);
        Debug.Log($"{owner.Unitname} 산화II 틱 -{finalDmg} (특수장갑 방어: {owner.specialArmor})");
    }

    // 금속 피로 : 매 턴 하드웨어 마모로 물리 방어력 감소 및 피로 도트
    private void MetalFatigueTick()
    {
        if (!HasStatus(StatusType.MetalFatigue)) return;

        int wearDmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.02f));
        owner.TakeDamage(wearDmg, Unit.DamageType.Normal);
        Debug.Log($"{owner.Unitname} 금속 피로 마모 피해 -{wearDmg}");
    }

    // 과열 : maxHp 4% 도트, 10% 확률 회로단선 / 5% 확률 화재 전이
    private void OverheatTick()
    {
        if (!HasStatus(StatusType.Overheat)) return;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.04f) - owner.specialArmor);
        owner.TakeDamage(dmg, Unit.DamageType.Fire);
        Debug.Log($"{owner.Unitname} 과열 틱 -{dmg}");

        if (Random.Range(0, 100) < 10 && !HasStatus(StatusType.CircuitryShort))
        {
            AddStatus(StatusType.CircuitryShort, 3);
            Debug.Log($"{owner.Unitname} 과열로 인한 회로 단선 발생!");
        }

        if (Random.Range(0, 100) < 5 && !HasStatus(StatusType.Fire))
        {
            InternalRemove(StatusType.Overheat);
            AddStatus(StatusType.Fire, 4);
            Debug.Log($"{owner.Unitname} 과열 → 화재 전이!");
        }
    }

    // 화재 : maxHp 10% 도트
    private void FireTick()
    {
        if (!HasStatus(StatusType.Fire)) return;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.10f) - owner.specialArmor);
        owner.TakeDamage(dmg, Unit.DamageType.Fire);
        Debug.Log($"{owner.Unitname} 화재 틱 -{dmg}");
    }

    // 회로 단선 (합선) : maxHp 3% 도트, 15% 확률 과열 유발
    private void CircuitryShortTick()
    {
        if (!HasStatus(StatusType.CircuitryShort) && !HasStatus(StatusType.ShortCircuit)) return;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.03f) - owner.specialArmor);
        owner.TakeDamage(dmg, Unit.DamageType.Electric);
        Debug.Log($"{owner.Unitname} 회로 단선 틱 -{dmg}");

        if (Random.Range(0, 100) < 15 && !HasStatus(StatusType.Overheat) && !HasStatus(StatusType.Fire))
        {
            AddStatus(StatusType.Overheat, 3);
            Debug.Log($"{owner.Unitname} 회로 단선 → 과열 발생!");
        }
    }

    // 윤활유 유출 : maxHp 3% 도트, 10% 확률 과열 유발
    private void LubricantLeakTick()
    {
        if (!HasStatus(StatusType.LubricantLeak) && !HasStatus(StatusType.OilLeak)) return;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.03f) - owner.specialArmor);
        owner.TakeDamage(dmg, Unit.DamageType.Bleed);
        Debug.Log($"{owner.Unitname} 윤활유 유출 틱 -{dmg}");

        if (Random.Range(0, 100) < 10 && !HasStatus(StatusType.Overheat) && !HasStatus(StatusType.Fire))
        {
            AddStatus(StatusType.Overheat, 3);
            Debug.Log($"{owner.Unitname} 윤활유 유출 → 마찰 과열 발생!");
        }
    }

    // 윤활유 고갈 : maxHp 2% 도트
    private void OilEmptyTick()
    {
        if (!HasStatus(StatusType.OilEmpty)) return;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.02f) - owner.specialArmor);
        owner.TakeDamage(dmg, Unit.DamageType.Bleed);
        Debug.Log($"{owner.Unitname} 윤활유 고갈 틱 -{dmg}");
    }

    // 출혈 : maxHp 5% 도트
    private void BleedingTick()
    {
        if (!HasStatus(StatusType.Bleeding)) return;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.05f));
        owner.TakeDamage(dmg, Unit.DamageType.Bleed);
        Debug.Log($"{owner.Unitname} 출혈 틱 -{dmg}");
    }

    public void AddMark(int turns) => AddStatus(StatusType.Marked, turns);
}
