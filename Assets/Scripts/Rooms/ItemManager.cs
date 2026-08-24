using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    [Header("Currency")]
    public int money;

    [Header("Inventory")]
    public List<ItemRuntime> inventory = new List<ItemRuntime>();

    private void Awake()
    {
        Instance = this;
    }

    // ============================================================
    // 인벤토리 관리
    // ============================================================

    /// <summary>아이템 획득 (수량 +1)</summary>
    public void AddItem(ItemData item)
    {
        if (item == null) return;

        ItemRuntime runtime = inventory.Find(r => r.data == item);
        if (runtime != null)
        {
            runtime.count++;
        }
        else
        {
            inventory.Add(new ItemRuntime
            {
                data = item,
                count = 1,
                usedCount = 0
            });
        }

        Debug.Log($"아이템 획득: {item.itemName}");
    }

    /// <summary>아이템 보유 수량 조회</summary>
    public int GetItemCount(ItemData item)
    {
        ItemRuntime runtime = inventory.Find(r => r.data == item);
        return runtime?.count ?? 0;
    }

    // ============================================================
    // 아이템 사용 (전투 중)
    // ============================================================

    /// <summary>전투 중 아이템 사용. 턴을 소모하므로 호출 후 EndTurn() 처리 필요.</summary>
    public bool UseItem(ItemData item, Unit target)
    {
        if (item == null || target == null) return false;

        ItemRuntime runtime = inventory.Find(r => r.data == item);
        if (runtime == null || runtime.count <= 0)
        {
            Debug.Log($"{item.itemName}: 보유 없음");
            return false;
        }

        // 사용 한도 체크
        if (item.maxUseCount > 0 && runtime.usedCount >= item.maxUseCount)
        {
            Debug.Log($"{item.itemName}: 사용 한도({item.maxUseCount}회) 초과! 연료 고갈");
            return false;
        }

        bool success = ApplyItemEffect(item, target, runtime);
        if (success)
        {
            runtime.count--;
            runtime.usedCount++;
            Debug.Log($"{item.itemName} 사용 완료 → {target.Unitname}");
        }

        return success;
    }

    private bool ApplyItemEffect(ItemData item, Unit target, ItemRuntime runtime)
    {
        switch (item.effectType)
        {
            // ── 고철덩어리: HP 회복 ──────────────────────────────
            case ItemEffectType.HealHp:
                int healed = Mathf.RoundToInt(item.healAmount * target.GetHealMultiplier());
                target.Heal(healed);
                Debug.Log($"{target.Unitname} HP +{healed}");
                return true;

            // ── 비상배터리: 에너지 회복 ─────────────────────────
            case ItemEffectType.RecoverEnergy:
                target.energyCurrent = Mathf.Min(
                    target.energyCurrent + item.energyAmount,
                    target.energyMax
                );
                Debug.Log($"{target.Unitname} 에너지 +{item.energyAmount}");
                return true;

            // ── 안테나: 통신기 복구 (미래 확장용) ───────────────
            case ItemEffectType.RecoverAntenna:
                Debug.Log($"{target.Unitname} 통신기 복구 완료");
                // TODO: 통신기 상태 시스템 구현 시 연동
                return true;

            // ── 퓨즈: 퓨즈 파손 해제 ────────────────────────────
            case ItemEffectType.RecoverFuse:
                if (!target.isFuseBroken)
                {
                    Debug.Log($"{target.Unitname}은 퓨즈 파손 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.FuseBroken);
                Debug.Log($"{target.Unitname} 퓨즈 복구 완료");
                return true;

            // ── 사포: 산화 해제 + HP 2 감소 ─────────────────────
            case ItemEffectType.SandpaperOxidation:
                if (!target.isOxidationI && !target.isOxidationII)
                {
                    Debug.Log($"{target.Unitname}은 산화 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.OxidationI);
                target.RemoveStatus(StatusType.OxidationII);
                target.TakeDamage(item.sandpaperHpPenalty, Unit.DamageType.Normal);
                Debug.Log($"{target.Unitname} 산화 제거 (사포) - HP -{item.sandpaperHpPenalty}");
                return true;

            // ── 녹 제거제: 산화 해제 ─────────────────────────────
            case ItemEffectType.RemoveOxidation:
                if (!target.isOxidationI && !target.isOxidationII)
                {
                    Debug.Log($"{target.Unitname}은 산화 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.OxidationI);
                target.RemoveStatus(StatusType.OxidationII);
                Debug.Log($"{target.Unitname} 산화 제거 (녹 제거제)");
                return true;

            // ── 냉각제: 과열 해제 ────────────────────────────────
            case ItemEffectType.CoolDown:
                if (!target.isOverheat)
                {
                    Debug.Log($"{target.Unitname}은 과열 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.Overheat);
                Debug.Log($"{target.Unitname} 과열 해제 (냉각제)");
                return true;

            // ── 소화기: 화재 → 과열 전환 ────────────────────────
            case ItemEffectType.ExtinguishFire:
                if (!target.isFire)
                {
                    Debug.Log($"{target.Unitname}은 화재 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.Fire);
                // 합선이 없을 때만 스프링쿨러 효과 (과열로 전환)
                if (!target.isShortCircuit)
                {
                    target.AddStatus(StatusType.Overheat);
                    Debug.Log($"{target.Unitname} 화재 진압 → 과열로 전환");
                }
                else
                {
                    Debug.Log($"{target.Unitname} 화재 진압 (합선으로 인해 과열 전환 불가)");
                }
                return true;

            // ── 솔: 무장 오염 해제 ───────────────────────────────
            case ItemEffectType.RemovePollution:
                if (!target.isWeaponPollution)
                {
                    Debug.Log($"{target.Unitname}은 무장 오염 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.WeaponPollution);
                Debug.Log($"{target.Unitname} 무장 오염 제거 (솔)");
                return true;

            // ── 절연 테이프: 합선 해제 ───────────────────────────
            case ItemEffectType.RemoveShortCircuit:
                if (!target.isShortCircuit)
                {
                    Debug.Log($"{target.Unitname}은 합선 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.ShortCircuit);
                Debug.Log($"{target.Unitname} 합선 해제 (절연 테이프)");
                return true;

            // ── 방수 테이프: 윤활유 누유 해제 ───────────────────
            case ItemEffectType.RemoveOilLeak:
                if (!target.isOilLeak)
                {
                    Debug.Log($"{target.Unitname}은 윤활유 누유 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.OilLeak);
                Debug.Log($"{target.Unitname} 윤활유 누유 수리 (방수 테이프)");
                return true;

            default:
                Debug.LogWarning($"미구현 아이템 효과: {item.effectType}");
                return false;
        }
    }

    // ============================================================
    // 상점 강화 (기존 유지)
    // ============================================================

    /// <summary>방어 강화 구매</summary>
    public void BuyDefenseUpgrade(Unit user)
    {
        if (user.defenseLevel >= user.maxDefenseLevel)
        {
            Debug.Log("최대 레벨");
            return;
        }

        int price = 50 * (user.defenseLevel + 1);
        if (money < price)
        {
            Debug.Log("돈 부족");
            return;
        }

        money -= price;
        user.defenseLevel++;
        user.defensePower += 3;
        Debug.Log(user.name + " 방어 강화 Lv." + user.defenseLevel);
    }

    /// <summary>공격 강화 구매</summary>
    public void BuyAttackUpgrade(Unit user)
    {
        if (user.attackLevel >= user.maxAttackLevel)
        {
            Debug.Log("최대 레벨");
            return;
        }

        int price = 100 * (user.attackLevel + 1);
        if (money < price)
        {
            Debug.Log("돈 부족");
            return;
        }

        money -= price;
        user.attackLevel++;
        user.attackPower += 5;
        Debug.Log(user.name + " 공격 강화 Lv." + user.attackLevel);
    }

    /// <summary>정확도 강화 구매</summary>
    public void BuyAccuracyUpgrade(Unit user)
    {
        if (user.accuracyupgrade)
        {
            Debug.Log("최대 레벨");
            return;
        }

        if (money < 3000)
        {
            Debug.Log("돈 부족");
            return;
        }

        money -= 3000;
        user.accuracyupgrade = true;
        user.accuracy += 10;
        Debug.Log(user.name + " 정확도 강화 완료");
    }
}