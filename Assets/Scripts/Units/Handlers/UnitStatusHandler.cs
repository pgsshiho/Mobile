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
    // 상태이상 틱 처리 중 새로 생긴 상태는 이번 턴에 피해와 턴 감소를 적용하지 않는다.
    private readonly HashSet<StatusType> statusesAppliedThisTurn = new HashSet<StatusType>();
    private bool isRestoringStatuses;
    private bool isProcessingTurnTick;

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

        if (isNew && isProcessingTurnTick)
            statusesAppliedThisTurn.Add(type);

        if (isNew)
        {
            uiHandler?.AddStatusIcon(type);
            if (!isRestoringStatuses)
                uiHandler?.ShowStatusText(GetStatusDisplayName(type), GetStatusTextColor(type));
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

    /// <summary>세이브 시스템에서 상태이상과 남은 턴을 보존하기 위한 읽기 전용 사본입니다.</summary>
    public Dictionary<StatusType, int> GetActiveStatuses()
    {
        return new Dictionary<StatusType, int>(statusTurns);
    }

    /// <summary>로드된 상태이상 목록을 UI 아이콘까지 포함해 복원합니다.</summary>
    public void RestoreStatuses(List<SavedStatusEntry> savedStatuses)
    {
        ClearStates();
        if (savedStatuses == null) return;

        isRestoringStatuses = true;
        try
        {
            foreach (SavedStatusEntry status in savedStatuses)
            {
                if (status == null || status.type == StatusType.None || status.remainingTurns == 0)
                    continue;
                AddStatus(status.type, status.remainingTurns);
            }
        }
        finally
        {
            isRestoringStatuses = false;
        }
    }

    // ────────────────────────────────────────────
    // 매 턴 처리 (MyTurn에서 호출)
    // ────────────────────────────────────────────
    public void TickTurn()
    {
        statusesAppliedThisTurn.Clear();
        isProcessingTurnTick = true;

        try
        {
        // 상태이상 피해는 한 턴에 모두 계산한 뒤 한 번만 적용한다.
        // 따라서 여러 상태이상에 걸려도 피해 숫자가 여러 개로 나뉘지 않는다.
        int totalStatusDamage = 0;
        totalStatusDamage += OxidationTick();
        totalStatusDamage += OverheatTick();
        totalStatusDamage += FireTick();
        totalStatusDamage += CircuitryShortTick();
        totalStatusDamage += LubricantLeakTick();
        totalStatusDamage += OilEmptyTick();
        totalStatusDamage += MetalFatigueTick();
        totalStatusDamage += BleedingTick();

        if (totalStatusDamage > 0)
        {
            owner.TakeDamage(totalStatusDamage, Unit.DamageType.Status);
            Debug.Log($"{owner.Unitname} 상태이상 합산 피해 -{totalStatusDamage}");
        }

        TickDownAll();
        }
        finally
        {
            isProcessingTurnTick = false;
            statusesAppliedThisTurn.Clear();
        }
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
            if (statusesAppliedThisTurn.Contains(type)) continue;

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
    private int OxidationTick()
    {
        if (!CanTick(StatusType.OxidationII)) return 0;

        float pct = owner.oxidationDamagePercent / 100f;
        int baseDmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * pct) + owner.rustDamage);
        int finalDmg = Mathf.Max(1, baseDmg - owner.specialArmor);

        Debug.Log($"{owner.Unitname} 산화II 틱 -{finalDmg} (특수장갑 방어: {owner.specialArmor})");
        return finalDmg;
    }

    // 금속 피로 : 매 턴 하드웨어 마모로 물리 방어력 감소 및 피로 도트
    private int MetalFatigueTick()
    {
        if (!CanTick(StatusType.MetalFatigue)) return 0;

        int wearDmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.02f));
        Debug.Log($"{owner.Unitname} 금속 피로 마모 피해 -{wearDmg}");
        return wearDmg;
    }

    // 과열 : maxHp 4% 도트, 10% 확률 회로단선 / 5% 확률 화재 전이
    private int OverheatTick()
    {
        if (!CanTick(StatusType.Overheat)) return 0;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.04f) - owner.specialArmor);
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

        return dmg;
    }

    // 화재 : maxHp 10% 도트
    private int FireTick()
    {
        if (!CanTick(StatusType.Fire)) return 0;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.10f) - owner.specialArmor);
        Debug.Log($"{owner.Unitname} 화재 틱 -{dmg}");
        return dmg;
    }

    // 회로 단선 (합선) : maxHp 3% 도트, 15% 확률 과열 유발
    private int CircuitryShortTick()
    {
        if (!CanTick(StatusType.CircuitryShort) && !CanTick(StatusType.ShortCircuit)) return 0;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.03f) - owner.specialArmor);
        Debug.Log($"{owner.Unitname} 회로 단선 틱 -{dmg}");

        if (Random.Range(0, 100) < 15 && !HasStatus(StatusType.Overheat) && !HasStatus(StatusType.Fire))
        {
            AddStatus(StatusType.Overheat, 3);
            Debug.Log($"{owner.Unitname} 회로 단선 → 과열 발생!");
        }

        return dmg;
    }

    // 윤활유 유출 : maxHp 3% 도트, 10% 확률 과열 유발
    private int LubricantLeakTick()
    {
        if (!CanTick(StatusType.LubricantLeak) && !CanTick(StatusType.OilLeak)) return 0;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.03f) - owner.specialArmor);
        Debug.Log($"{owner.Unitname} 윤활유 유출 틱 -{dmg}");

        if (Random.Range(0, 100) < 10 && !HasStatus(StatusType.Overheat) && !HasStatus(StatusType.Fire))
        {
            AddStatus(StatusType.Overheat, 3);
            Debug.Log($"{owner.Unitname} 윤활유 유출 → 마찰 과열 발생!");
        }

        return dmg;
    }

    // 윤활유 고갈 : maxHp 2% 도트
    private int OilEmptyTick()
    {
        if (!CanTick(StatusType.OilEmpty)) return 0;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.02f) - owner.specialArmor);
        Debug.Log($"{owner.Unitname} 윤활유 고갈 틱 -{dmg}");
        return dmg;
    }

    // 출혈 : maxHp 5% 도트
    private int BleedingTick()
    {
        if (!CanTick(StatusType.Bleeding)) return 0;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.05f));
        Debug.Log($"{owner.Unitname} 출혈 틱 -{dmg}");
        return dmg;
    }

    private bool CanTick(StatusType type) =>
        HasStatus(type) && !statusesAppliedThisTurn.Contains(type);

    private static string GetStatusDisplayName(StatusType type) => type switch
    {
        StatusType.MetalFatigue => "금속 피로",
        StatusType.LubricantLeak => "윤활유 유출",
        StatusType.CircuitryShort => "회로 단선",
        StatusType.DataFragmentation => "데이터 파편화",
        StatusType.LogicLoop => "논리 오류",
        StatusType.Ghosting => "잔상 현상",
        StatusType.OxidationI => "산화 I",
        StatusType.OxidationII => "산화 II",
        StatusType.Overheat => "과열",
        StatusType.Fire => "화재",
        StatusType.ShortCircuit => "합선",
        StatusType.OilLeak => "윤활유 누유",
        StatusType.OilEmpty => "윤활유 고갈",
        StatusType.FuseBroken => "퓨즈 파손",
        StatusType.WeaponPollution => "무장 오염",
        StatusType.Broken => "파손",
        StatusType.Marked => "목표 지정",
        StatusType.Stun => "기절",
        StatusType.Bleeding => "출혈",
        _ => "상태이상"
    };

    private static Color GetStatusTextColor(StatusType type) => type switch
    {
        StatusType.Fire or StatusType.Overheat => new Color(1f, 0.45f, 0.05f),
        StatusType.Bleeding or StatusType.LubricantLeak or StatusType.OilLeak => new Color(0.95f, 0.15f, 0.15f),
        StatusType.CircuitryShort or StatusType.ShortCircuit => new Color(1f, 0.95f, 0.15f),
        StatusType.OxidationI or StatusType.OxidationII => new Color(0.75f, 0.45f, 0.2f),
        StatusType.Stun => Color.gray,
        _ => new Color(0.85f, 0.35f, 1f)
    };

    public void AddMark(int turns) => AddStatus(StatusType.Marked, turns);
}
