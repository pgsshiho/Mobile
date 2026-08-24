using UnityEngine;

public class UnitStatusHandler
{
    private readonly Unit owner;
    private readonly UnitUIHandler uiHandler;

    // Status Effects
    public bool isOxidationI;
    public bool isOxidationII;
    public bool isOverheat;
    public bool isFire;
    public bool isShortCircuit;
    public bool isFuseBroken;
    public bool isWeaponPollution;
    public bool isOilLeak;
    public bool isOilEmpty;
    public bool isBroken;
    public bool isMarked;
    public int markedTurn;

    // Legacy States
    public bool isBleeding;
    public int bleedingCount;
    public bool isStunned;
    public int fireCount;
    public bool isFires;

    public UnitStatusHandler(Unit owner, UnitUIHandler uiHandler)
    {
        this.owner = owner;
        this.uiHandler = uiHandler;
    }

    public void AddStatus(StatusType type, int turn = 0)
    {
        switch (type)
        {
            case StatusType.OxidationI:
                if (isOxidationI)
                {
                    isOxidationI = false;
                    isOxidationII = true;
                    uiHandler?.RemoveStatusIcon(StatusType.OxidationI);
                    uiHandler?.AddStatusIcon(StatusType.OxidationII);
                }
                else
                {
                    isOxidationI = true;
                    uiHandler?.AddStatusIcon(StatusType.OxidationI);
                }
                break;

            case StatusType.OxidationII:
                isOxidationI = false;
                isOxidationII = true;
                uiHandler?.RemoveStatusIcon(StatusType.OxidationI);
                uiHandler?.AddStatusIcon(StatusType.OxidationII);
                break;

            case StatusType.Overheat:
                isOverheat = true;
                uiHandler?.AddStatusIcon(StatusType.Overheat);
                break;

            case StatusType.Fire:
                isOverheat = false;
                isFire = true;
                uiHandler?.RemoveStatusIcon(StatusType.Overheat);
                uiHandler?.AddStatusIcon(StatusType.Fire);
                break;

            case StatusType.ShortCircuit:
                isShortCircuit = true;
                uiHandler?.AddStatusIcon(StatusType.ShortCircuit);
                break;

            case StatusType.FuseBroken:
                isFuseBroken = true;
                uiHandler?.AddStatusIcon(StatusType.FuseBroken);
                break;

            case StatusType.WeaponPollution:
                isWeaponPollution = true;
                uiHandler?.AddStatusIcon(StatusType.WeaponPollution);
                break;

            case StatusType.OilLeak:
                isOilLeak = true;
                uiHandler?.AddStatusIcon(StatusType.OilLeak);
                break;

            case StatusType.OilEmpty:
                isOilLeak = false;
                isOilEmpty = true;
                uiHandler?.RemoveStatusIcon(StatusType.OilLeak);
                uiHandler?.AddStatusIcon(StatusType.OilEmpty);
                break;

            case StatusType.Broken:
                isBroken = true;
                uiHandler?.AddStatusIcon(StatusType.Broken);
                break;

            case StatusType.Marked:
                isMarked = true;
                markedTurn = turn;
                uiHandler?.AddStatusIcon(StatusType.Marked);
                break;
        }
    }

    public void RemoveStatus(StatusType type)
    {
        switch (type)
        {
            case StatusType.OxidationI:
                isOxidationI = false;
                break;

            case StatusType.OxidationII:
                isOxidationII = false;
                break;

            case StatusType.Overheat:
                isOverheat = false;
                break;

            case StatusType.Fire:
                isFire = false;
                break;

            case StatusType.ShortCircuit:
                isShortCircuit = false;
                break;

            case StatusType.FuseBroken:
                isFuseBroken = false;
                break;

            case StatusType.WeaponPollution:
                isWeaponPollution = false;
                break;

            case StatusType.OilLeak:
                isOilLeak = false;
                break;

            case StatusType.OilEmpty:
                isOilEmpty = false;
                break;

            case StatusType.Broken:
                isBroken = false;
                break;

            case StatusType.Marked:
                isMarked = false;
                markedTurn = 0;
                break;
        }

        uiHandler?.RemoveStatusIcon(type);
    }

    public void ClearStates()
    {
        isBleeding = false;
        bleedingCount = 0;

        isStunned = false;

        fireCount = 0;
        isFires = false;

        isOxidationI = false;
        isOxidationII = false;
        isOverheat = false;
        isFire = false;
        isShortCircuit = false;
        isFuseBroken = false;
        isWeaponPollution = false;
        isOilLeak = false;
        isOilEmpty = false;
        isBroken = false;
        isMarked = false;
        markedTurn = 0;

        uiHandler?.ClearStatusIcons();
    }

    public void AddMark(int turn)
    {
        isMarked = true;
        markedTurn = turn;
        uiHandler?.AddStatusIcon(StatusType.Marked);
        Debug.Log($"{owner.Unitname} 표식!");
    }

    public void MarkTurn()
    {
        if (!isMarked) return;

        markedTurn--;
        if (markedTurn <= 0)
        {
            isMarked = false;
            markedTurn = 0;
            uiHandler?.RemoveStatusIcon(StatusType.Marked);
            Debug.Log($"{owner.Unitname} 표식 해제");
        }
    }

    public void Bleeding()
    {
        if (!isBleeding) return;

        float damage = owner.maxHealth * 0.05f;
        int finalDamage = Mathf.RoundToInt(damage);

        owner.TakeDamage(finalDamage, Unit.DamageType.Bleed);
        Debug.Log($"{owner.Unitname} 출혈 피해 {damage}");

        bleedingCount++;
        if (bleedingCount >= 3)
        {
            isBleeding = false;
            bleedingCount = 0;
            Debug.Log($"{owner.Unitname} 출혈 종료");
        }
    }

    public void Fire()
    {
        if (fireCount == 1)
        {
            float damage = owner.maxHealth * 0.04f;
            int finalDamage = Mathf.RoundToInt(damage);
            owner.TakeDamage(finalDamage, Unit.DamageType.Fire);
            Debug.Log($"{owner.Unitname} 과열 피해 {damage}");
            fireCount++;
        }
        else if (fireCount == 2)
        {
            float damage = owner.maxHealth * 0.08f;
            int finalDamage = Mathf.RoundToInt(damage);
            owner.TakeDamage(finalDamage, Unit.DamageType.Fire);
            Debug.Log($"{owner.Unitname} 과열 피해 {damage}");
            fireCount++;
        }
        else if (fireCount >= 3)
        {
            isFires = true;
        }

        if (isFires)
        {
            float damage = owner.maxHealth * 0.2f;
            int finalDamage = Mathf.RoundToInt(damage);
            owner.TakeDamage(finalDamage, Unit.DamageType.Fire);
            Debug.Log($"{owner.Unitname} 화재 피해 {damage}");
        }
    }

    public void TickTurn()
    {
        OxidationTick();
        OverheatTick();
        ShortCircuitTick();
        OilLeakTick();
        OilEmptyTick();
        Bleeding();
        Fire();
        MarkTurn();
    }

    // ── 산화 I/II ─────────────────────────────────────────────────
    // 산화 I: 속도 감소만 (데미지 없음 - 스탯 패널티는 CombatCalculator에서 처리)
    // 산화 II: 속도 감소 + 지속 데미지 (maxHp * oxidationDamagePercent%)
    private void OxidationTick()
    {
        if (isOxidationII)
        {
            float pct = owner.oxidationDamagePercent / 100f;
            int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * pct));
            // 특수 장갑으로 경감
            dmg = Mathf.Max(1, dmg - owner.specialArmor);
            owner.TakeDamage(dmg, Unit.DamageType.Bleed);
            Debug.Log($"{owner.Unitname} 산화II 틱 -{dmg}");
        }
        // 산화 I은 속도 패널티만 (CombatCalculator.GetSpeed에서 반영)
    }

    // ── 과열 I ────────────────────────────────────────────────────
    // 지속 데미지 (maxHp 4%), 확률적 합선/화재 전이
    private void OverheatTick()
    {
        if (!isOverheat) return;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.04f));
        dmg = Mathf.Max(1, dmg - owner.specialArmor);
        owner.TakeDamage(dmg, Unit.DamageType.Fire);
        Debug.Log($"{owner.Unitname} 과열 틱 -{dmg}");

        // 10% 확률로 합선 전이
        if (UnityEngine.Random.Range(0, 100) < 10 && !isShortCircuit)
        {
            owner.AddStatus(StatusType.ShortCircuit);
            Debug.Log($"{owner.Unitname} 과열 → 합선 발생!");
        }

        // 5% 확률로 화재 전이
        if (UnityEngine.Random.Range(0, 100) < 5 && !isFire)
        {
            owner.AddStatus(StatusType.Fire);
            owner.RemoveStatus(StatusType.Overheat);
            Debug.Log($"{owner.Unitname} 과열 → 화재 발전!");
        }
    }

    // ── 합선 ──────────────────────────────────────────────────────
    // 지속 데미지 (maxHp 3%) + 확률적 발열 유발
    private void ShortCircuitTick()
    {
        if (!isShortCircuit) return;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.03f));
        dmg = Mathf.Max(1, dmg - owner.specialArmor);
        owner.TakeDamage(dmg, Unit.DamageType.Fire);
        Debug.Log($"{owner.Unitname} 합선 틱 -{dmg}");

        // 15% 확률로 과열 유발
        if (UnityEngine.Random.Range(0, 100) < 15 && !isOverheat && !isFire)
        {
            owner.AddStatus(StatusType.Overheat);
            Debug.Log($"{owner.Unitname} 합선 → 과열 발생!");
        }
    }

    // ── 윤활유 누유 ───────────────────────────────────────────────
    // 속도 감소 + 지속 데미지 (maxHp 3%) + 확률적 발열
    private void OilLeakTick()
    {
        if (!isOilLeak) return;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.03f));
        dmg = Mathf.Max(1, dmg - owner.specialArmor);
        owner.TakeDamage(dmg, Unit.DamageType.Bleed);
        Debug.Log($"{owner.Unitname} 윤활유 누유 틱 -{dmg}");

        // 10% 확률로 발열
        if (UnityEngine.Random.Range(0, 100) < 10 && !isOverheat && !isFire)
        {
            owner.AddStatus(StatusType.Overheat);
            Debug.Log($"{owner.Unitname} 누유 → 과열 발생!");
        }
    }

    // ── 윤활유 고갈 ───────────────────────────────────────────────
    // 속도 감소 + 지속 데미지 지속 (누유보다 약한 데미지)
    private void OilEmptyTick()
    {
        if (!isOilEmpty) return;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.maxHealth * 0.02f));
        dmg = Mathf.Max(1, dmg - owner.specialArmor);
        owner.TakeDamage(dmg, Unit.DamageType.Bleed);
        Debug.Log($"{owner.Unitname} 윤활유 고갈 틱 -{dmg}");
    }
}
