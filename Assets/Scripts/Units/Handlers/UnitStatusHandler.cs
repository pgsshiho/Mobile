using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 상태이상을 턴 카운터 및 틱 데미지로 관리합니다.
/// - 유닛이 직접 지정한 지속 턴 수(turns)와 턴당 고정 데미지(dotDamage)를 반영합니다.
/// - 양수 N : N턴 후 자동 해제
/// - -1     : 무한 지속 (아이템/스킬로만 해제)
/// - 상태이상 재부여 시 기존 턴 수가 있다면 덮어쓰지 않고 턴 수가 합산(Stack)되며,
///   틱당 데미지도 누적 합산되어 더 강력해집니다.
/// </summary>
public class UnitStatusHandler
{
    private readonly Unit owner;
    private readonly UnitUIHandler uiHandler;

    // 상태이상별 남은 턴 수 (-1 = 무한, 0 이하 = 없음)
    private readonly Dictionary<StatusType, int> statusTurns = new Dictionary<StatusType, int>();

    // 상태이상별 턴당 도트 데미지 (유닛/스킬이 직접 지정한 수치 보관)
    private readonly Dictionary<StatusType, int> statusDamages = new Dictionary<StatusType, int>();

    // 상태이상 틱 처리 중 새로 생긴 상태는 이번 턴에 피해와 턴 감소를 적용하지 않는다.
    private readonly HashSet<StatusType> statusesAppliedThisTurn = new HashSet<StatusType>();
    private bool isRestoringStatuses;
    private bool isProcessingTurnTick;

    // ────────────────────────────────────────────
    // 읽기 프로퍼티 (Unit.cs 에서 참조)
    // ────────────────────────────────────────────
    public bool HasStatus(StatusType type) =>
        statusTurns.TryGetValue(type, out int t) && t != 0;

    public bool HasAnyStatus() => statusTurns.Count > 0;

    public int GetRemainingTurns(StatusType type) =>
        statusTurns.TryGetValue(type, out int t) ? t : 0;

    /// <summary>특정 상태이상에 할당된 턴당 도트 피해량을 반환합니다.</summary>
    public int GetStatusDamage(StatusType type) =>
        statusDamages.TryGetValue(type, out int d) ? d : 0;

    /// <summary>현재 유닛이 보유한 모든 상태이상과 턴 수, 데미지 목록을 반환합니다.</summary>
    public Dictionary<StatusType, (int turns, int dotDamage)> GetAllActiveStatuses()
    {
        var result = new Dictionary<StatusType, (int turns, int dotDamage)>();
        foreach (var pair in statusTurns)
        {
            if (pair.Value > 0 || pair.Value == -1)
            {
                int dmg = statusDamages.TryGetValue(pair.Key, out int d) ? d : 0;
                result[pair.Key] = (pair.Value, dmg);
            }
        }
        return result;
    }

    /// <summary>상태이상 딕셔너리를 일괄 적용합니다 (복제/합체용).</summary>
    public void CopyStatusesFrom(Dictionary<StatusType, (int turns, int dotDamage)> statuses)
    {
        if (statuses == null) return;
        foreach (var pair in statuses)
        {
            AddStatus(pair.Key, pair.Value.turns, pair.Value.dotDamage);
        }
    }

    /// <summary>현재 유닛에 적용된 산화(녹)의 턴당 고정 피해량</summary>
    public int CurrentOxidationDamage => GetStatusDamage(StatusType.OxidationII) > 0 ? GetStatusDamage(StatusType.OxidationII) : GetStatusDamage(StatusType.OxidationI);

    /// <summary>현재 유닛에 적용된 윤활유 누유의 턴당 방어력 무시 고정 피해량</summary>
    public int CurrentOilLeakDamage
    {
        get
        {
            int d = GetStatusDamage(StatusType.OilLeak);
            if (d <= 0) d = GetStatusDamage(StatusType.LubricantLeak);
            return d;
        }
    }

    /// <summary>윤활유 누유 누적 스택 (걸릴 때마다 증가)</summary>
    public int OilLeakStacks { get; private set; } = 0;

    public UnitStatusHandler(Unit owner, UnitUIHandler uiHandler)
    {
        this.owner = owner;
        this.uiHandler = uiHandler;
    }

    // ────────────────────────────────────────────
    // 부여
    // turns     : 지속 턴 수, -1 이면 무한
    // dotDamage : 유닛이 직접 지정하는 턴당 피해량 (0 이하일 경우 기본 권장값 사용)
    // ────────────────────────────────────────────

    /// <summary>
    /// 공격자가 지정한 지속 턴수와 턴당 고정 데미지를 반영하여 산화를 부여합니다.
    /// 중첩 시 턴 수와 데미지가 합산됩니다.
    /// </summary>
    public void AddOxidation(int turns, int damagePerTurn)
    {
        if (turns <= 0) return;
        int dmg = damagePerTurn > 0 ? damagePerTurn : (owner.oxidationDamage > 0 ? owner.oxidationDamage : 5);

        if (HasStatus(StatusType.OxidationI))
        {
            int prevTurns = GetRemainingTurns(StatusType.OxidationI);
            int prevDmg = GetStatusDamage(StatusType.OxidationI);
            RemoveStatus(StatusType.OxidationI);
            SetStatusWithDamage(StatusType.OxidationII, prevTurns + turns, prevDmg + dmg);
        }
        else if (HasStatus(StatusType.OxidationII))
        {
            SetStatusWithDamage(StatusType.OxidationII, turns, dmg);
        }
        else
        {
            SetStatusWithDamage(StatusType.OxidationI, turns, dmg);
        }
    }

    /// <summary>
    /// 지속 턴수와 턴당 고정 데미지를 직접 반영하여 누유를 부여합니다.
    /// 방어력을 무시하는 고정 데미지이며, 한번 더 걸리면 턴수와 데미지가 모두 누적 합산됩니다.
    /// </summary>
    public void AddOilLeak(int turns, int damagePerStack = 5)
    {
        if (turns <= 0) return;
        int dmg = damagePerStack > 0 ? damagePerStack : 5;

        if (!isRestoringStatuses)
        {
            OilLeakStacks++;
        }

        SetStatusWithDamage(StatusType.LubricantLeak, turns, dmg);
        SetStatusWithDamage(StatusType.OilLeak, turns, dmg);
        Debug.Log($"<color=orange>[누유 부여]</color> {owner.Unitname} 누유 적용 (스택: {OilLeakStacks}, 턴당 방어무시 피해: {CurrentOilLeakDamage})");
    }

    /// <summary>
    /// 유닛이 직접 몇 턴부터 몇 데미지부여까지 정하여 상태이상을 부여합니다.
    /// </summary>
    public void AddStatus(StatusType type, int turns = 3, int dotDamage = 0)
    {
        if (turns == 0) return;

        switch (type)
        {
            // 산화 I이 이미 있으면 II로 전이 (턴수 및 데미지 합산)
            case StatusType.OxidationI:
                int oxDmg = dotDamage > 0 ? dotDamage : (owner.oxidationDamage > 0 ? owner.oxidationDamage : 5);
                if (HasStatus(StatusType.OxidationI))
                {
                    int prevTurns = GetRemainingTurns(StatusType.OxidationI);
                    int prevDmg = GetStatusDamage(StatusType.OxidationI);
                    RemoveStatus(StatusType.OxidationI);
                    SetStatusWithDamage(StatusType.OxidationII, prevTurns + turns, prevDmg + oxDmg);
                }
                else
                {
                    SetStatusWithDamage(StatusType.OxidationI, turns, oxDmg);
                }
                break;

            // 산화 II는 산화 I을 흡수하며 턴수 및 데미지 합산
            case StatusType.OxidationII:
                int oxDmg2 = dotDamage > 0 ? dotDamage : (owner.oxidationDamage > 0 ? owner.oxidationDamage : 5);
                if (HasStatus(StatusType.OxidationI))
                {
                    int prevTurns = GetRemainingTurns(StatusType.OxidationI);
                    int prevDmg = GetStatusDamage(StatusType.OxidationI);
                    InternalRemove(StatusType.OxidationI);
                    SetStatusWithDamage(StatusType.OxidationII, prevTurns + turns, prevDmg + oxDmg2);
                }
                else
                {
                    SetStatusWithDamage(StatusType.OxidationII, turns, oxDmg2);
                }
                break;

            // 화재는 과열을 흡수하며 턴수 및 데미지 합산
            case StatusType.Fire:
                int fireDmg = dotDamage > 0 ? dotDamage : GetDefaultStatusDamage(StatusType.Fire);
                if (HasStatus(StatusType.Overheat))
                {
                    int overheatTurns = GetRemainingTurns(StatusType.Overheat);
                    int overheatDmg = GetStatusDamage(StatusType.Overheat);
                    InternalRemove(StatusType.Overheat);
                    SetStatusWithDamage(StatusType.Fire, overheatTurns + turns, overheatDmg + fireDmg);
                }
                else
                {
                    SetStatusWithDamage(StatusType.Fire, turns, fireDmg);
                }
                break;

            // 윤활유 유출 및 누유 (방어력 무시 고정 피해, 중첩 시 데미지 합산)
            case StatusType.LubricantLeak:
            case StatusType.OilLeak:
                int leakDmg = dotDamage > 0 ? dotDamage : 5;
                if (!isRestoringStatuses)
                {
                    OilLeakStacks++;
                }
                SetStatusWithDamage(StatusType.LubricantLeak, turns, leakDmg);
                SetStatusWithDamage(StatusType.OilLeak, turns, leakDmg);
                Debug.Log($"<color=orange>[누유 중첩]</color> {owner.Unitname} 누유 적용 (스택: {OilLeakStacks}, 턴당 피해: {CurrentOilLeakDamage})");
                break;

            case StatusType.OilEmpty:
                InternalRemove(StatusType.LubricantLeak);
                InternalRemove(StatusType.OilLeak);
                SetStatusWithDamage(StatusType.OilEmpty, turns, dotDamage);
                break;

            // 회로 단선 및 합선
            case StatusType.CircuitryShort:
            case StatusType.ShortCircuit:
                SetStatusWithDamage(StatusType.CircuitryShort, turns, dotDamage);
                SetStatusWithDamage(StatusType.ShortCircuit, turns, dotDamage);
                break;

            default:
                SetStatusWithDamage(type, turns, dotDamage);
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
        statusDamages.Remove(type);
        uiHandler?.RemoveStatusIcon(type);

        // 연관 스택 및 데미지 초기화
        if (type == StatusType.CircuitryShort || type == StatusType.ShortCircuit)
        {
            owner.circuitShortStacks = 0;
        }
        else if (type == StatusType.Ghosting)
        {
            owner.ghostingMissPenalty = 0;
        }
        else if (type == StatusType.FuseBroken)
        {
            owner.ClearFuseBrokenSkillLock();
        }
        else if (type == StatusType.LubricantLeak || type == StatusType.OilLeak)
        {
            if (!HasStatus(StatusType.LubricantLeak) && !HasStatus(StatusType.OilLeak))
            {
                OilLeakStacks = 0;
                statusDamages.Remove(StatusType.LubricantLeak);
                statusDamages.Remove(StatusType.OilLeak);
            }
        }
    }

    // ────────────────────────────────────────────
    // 내부 세터 (턴 수 합산 및 유닛 지정 데미지 누적 합산)
    // ────────────────────────────────────────────
    private void SetStatusWithDamage(StatusType type, int turns, int dotDamage)
    {
        bool isNew = !statusTurns.ContainsKey(type) || statusTurns[type] <= 0;

        if (isRestoringStatuses)
        {
            // 저장 데이터 로드 복원 시에는 저장된 턴/데미지 값 그대로 복원
            statusTurns[type] = turns;
            if (dotDamage > 0)
                statusDamages[type] = dotDamage;
        }
        else
        {
            int appliedDamage = dotDamage > 0 ? dotDamage : GetDefaultStatusDamage(type);

            if (isNew)
            {
                statusTurns[type] = turns;
                if (appliedDamage > 0)
                    statusDamages[type] = appliedDamage;
            }
            else
            {
                // 이미 턴 수가 있는 경우: 덮어씌우지 않고 합산!
                if (statusTurns[type] == -1 || turns == -1)
                {
                    statusTurns[type] = -1; // 무한 지속
                }
                else
                {
                    statusTurns[type] += turns; // 턴 수 합산
                }

                // 한번 더 걸리면 데미지가 더 높아지도록 누적 합산
                if (appliedDamage > 0)
                {
                    if (statusDamages.ContainsKey(type))
                        statusDamages[type] += appliedDamage;
                    else
                        statusDamages[type] = appliedDamage;
                }
            }
        }

        if (isNew && isProcessingTurnTick)
            statusesAppliedThisTurn.Add(type);

        int currentDmg = GetStatusDamage(type);
        if (isNew)
        {
            if (type == StatusType.FuseBroken)
                owner.LockRandomSkillForFuseBroken();

            uiHandler?.AddStatusIcon(type);
            if (!isRestoringStatuses)
                uiHandler?.ShowStatusText(GetStatusDisplayName(type), GetStatusTextColor(type));
            Debug.Log($"{owner.Unitname} [{type}] 부여 완료 (지속: {(turns == -1 ? "무한" : turns + "턴")}, 턴당 피해: {currentDmg})");
        }
        else
        {
            Debug.Log($"{owner.Unitname} [{type}] 턴 및 데미지 합산 갱신 → 총 {(statusTurns[type] == -1 ? "무한" : statusTurns[type] + "턴")}, 누적 턴당 피해: {currentDmg}");
        }
    }

    /// <summary>데미지가 지정되지 않았을 때 사용하는 기본 틱당 데미지</summary>
    private int GetDefaultStatusDamage(StatusType type) => type switch
    {
        StatusType.Bleeding => 5,
        StatusType.OilLeak or StatusType.LubricantLeak => 5,
        StatusType.Overheat => Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.04f)),
        StatusType.Fire => Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.10f)),
        StatusType.OxidationI or StatusType.OxidationII => owner.oxidationDamage > 0 ? owner.oxidationDamage : 5,
        StatusType.CircuitryShort or StatusType.ShortCircuit => Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.03f)),
        StatusType.MetalFatigue => Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.02f)),
        _ => 0
    };

    // ────────────────────────────────────────────
    // 전체 초기화
    // ────────────────────────────────────────────
    public void ClearStates()
    {
        statusTurns.Clear();
        statusDamages.Clear();
        OilLeakStacks = 0;
        owner.ClearFuseBrokenSkillLock();
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
                AddStatus(status.type, status.remainingTurns, status.dotDamage);
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

    // 산화 II : 공격자/유닛이 직접 지정한 턴당 고정 데미지 (방어력 무시)
    private int OxidationTick()
    {
        if (!CanTick(StatusType.OxidationII)) return 0;

        int rawDmg = GetStatusDamage(StatusType.OxidationII);
        if (rawDmg <= 0) rawDmg = owner.oxidationDamage > 0 ? owner.oxidationDamage : 5;

        int finalDmg = Mathf.Max(1, rawDmg);
        Debug.Log($"{owner.Unitname} 산화II 틱 -{finalDmg} (방어력 무시 고정 피해: {rawDmg})");
        return finalDmg;
    }

    // 금속 피로 : 매 턴 하드웨어 마모로 물리 방어력 감소 및 피로 도트
    private int MetalFatigueTick()
    {
        if (!CanTick(StatusType.MetalFatigue)) return 0;

        int dmg = GetStatusDamage(StatusType.MetalFatigue);
        if (dmg <= 0) dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.02f));
        Debug.Log($"{owner.Unitname} 금속 피로 마모 피해 -{dmg}");
        return dmg;
    }

    // 과열 : 유닛이 직접 지정한 턴당 데미지 (방어력 무시), 10% 확률 회로단선 / 5% 확률 화재 전이
    private int OverheatTick()
    {
        if (!CanTick(StatusType.Overheat)) return 0;

        int rawDmg = GetStatusDamage(StatusType.Overheat);
        if (rawDmg <= 0) rawDmg = Mathf.RoundToInt(owner.maxHealth * 0.04f);

        int dmg = Mathf.Max(1, rawDmg);
        Debug.Log($"{owner.Unitname} 과열 틱 -{dmg} (방어력 무시 고정 피해: {rawDmg})");

        if (Random.Range(0, 100) < 10 && !HasStatus(StatusType.CircuitryShort))
        {
            AddStatus(StatusType.CircuitryShort, 3);
            Debug.Log($"{owner.Unitname} 과열로 인한 회로 단선 발생!");
        }

        if (Random.Range(0, 100) < 5 && !HasStatus(StatusType.Fire))
        {
            int fireDmg = GetStatusDamage(StatusType.Fire);
            if (fireDmg <= 0) fireDmg = rawDmg * 2;
            InternalRemove(StatusType.Overheat);
            AddStatus(StatusType.Fire, 4, fireDmg);
            Debug.Log($"{owner.Unitname} 과열 → 화재 전이!");
        }

        return dmg;
    }

    // 화재 : 유닛이 직접 지정한 턴당 데미지 (방어력 무시)
    private int FireTick()
    {
        if (!CanTick(StatusType.Fire)) return 0;

        int rawDmg = GetStatusDamage(StatusType.Fire);
        if (rawDmg <= 0) rawDmg = Mathf.RoundToInt(owner.maxHealth * 0.10f);

        int dmg = Mathf.Max(1, rawDmg);
        Debug.Log($"{owner.Unitname} 화재 틱 -{dmg} (방어력 무시 고정 피해: {rawDmg})");
        return dmg;
    }

    // 회로 단선 (합선) : 유닛이 지정한 데미지, 15% 확률 과열 유발
    private int CircuitryShortTick()
    {
        if (!CanTick(StatusType.CircuitryShort) && !CanTick(StatusType.ShortCircuit)) return 0;

        int rawDmg = GetStatusDamage(StatusType.CircuitryShort);
        if (rawDmg <= 0) rawDmg = GetStatusDamage(StatusType.ShortCircuit);
        if (rawDmg <= 0) rawDmg = Mathf.RoundToInt(owner.maxHealth * 0.03f);

        int dmg = Mathf.Max(1, rawDmg);
        Debug.Log($"{owner.Unitname} 회로 단선 틱 -{dmg} (방어력 무시 고정 피해)");

        if (Random.Range(0, 100) < 15 && !HasStatus(StatusType.Overheat) && !HasStatus(StatusType.Fire))
        {
            AddStatus(StatusType.Overheat, 3);
            Debug.Log($"{owner.Unitname} 회로 단선 → 과열 발생!");
        }

        return dmg;
    }

    // 윤활유 유출 / 누유 : 출혈처럼 방어력 무시의 일정 고정 데미지 (중첩 시 데미지 증가)
    private int LubricantLeakTick()
    {
        if (!CanTick(StatusType.LubricantLeak) && !CanTick(StatusType.OilLeak)) return 0;

        // % 데미지가 아닌 방어력 무시의 일정 고정 데미지 (한 번 더 걸리면 데미지 누적 증가)
        int dmg = CurrentOilLeakDamage > 0 ? CurrentOilLeakDamage : 5;
        Debug.Log($"{owner.Unitname} 누유 틱 피해 -{dmg} (스택: {OilLeakStacks}, 방어력 무시 고정 피해)");

        if (Random.Range(0, 100) < 10 && !HasStatus(StatusType.Overheat) && !HasStatus(StatusType.Fire))
        {
            AddStatus(StatusType.Overheat, 3);
            Debug.Log($"{owner.Unitname} 윤활유 누유 → 마찰 과열 발생!");
        }

        return dmg;
    }

    // 윤활유 고갈 : maxHp 2% 도트
    private int OilEmptyTick()
    {
        if (!CanTick(StatusType.OilEmpty)) return 0;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.02f));
        Debug.Log($"{owner.Unitname} 윤활유 고갈 틱 -{dmg} (방어력 무시 피해)");

        if (Random.Range(0, 100) < 5 &&
            !HasStatus(StatusType.Overheat) &&
            !HasStatus(StatusType.Fire))
        {
            AddStatus(StatusType.Overheat, 3);
            Debug.Log($"{owner.Unitname} 윤활유 고갈로 과열 발생! (5%)");
        }

        return dmg;
    }

    // 출혈 : 유닛이 직접 지정한 방어력 무시 고정 도트 데미지 (중첩 시 증가)
    private int BleedingTick()
    {
        if (!CanTick(StatusType.Bleeding)) return 0;

        int dmg = GetStatusDamage(StatusType.Bleeding);
        if (dmg <= 0) dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.05f));

        Debug.Log($"{owner.Unitname} 출혈 틱 -{dmg} (방어력 무시 고정 피해)");
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
