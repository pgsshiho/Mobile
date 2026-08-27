using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 인벤토리 및 아이템 효과 적용을 관리합니다. 재화(골드·재료)는 CurrencyManager에서 담당합니다.
/// </summary>
public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    [Header("Inventory")]
    public List<ItemRuntime> inventory = new List<ItemRuntime>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadFromSave();
    }

    // ════════════════════════════════════════════════════════════════════
    //  저장 / 로드
    // ════════════════════════════════════════════════════════════════════

    public void LoadFromSave()
    {
        SaveData data = Save.GetSaveData();
        if (data == null) return;

        if (data.inventory != null && data.inventory.Count > 0)
        {
            inventory.Clear();
            foreach (var savedItem in data.inventory)
            {
                ItemData itemData = Resources.Load<ItemData>($"Items/{savedItem.itemName}");
                if (itemData != null)
                {
                    inventory.Add(new ItemRuntime
                    {
                        data      = itemData,
                        count     = savedItem.count,
                        usedCount = savedItem.usedCount
                    });
                }
            }
        }
    }

    public void SaveToData()
    {
        SaveData data = Save.GetSaveData();
        if (data == null) return;

        data.inventory.Clear();
        foreach (var runtime in inventory)
        {
            if (runtime?.data != null)
            {
                data.inventory.Add(new SavedItemEntry
                {
                    itemName  = runtime.data.name,
                    count     = runtime.count,
                    usedCount = runtime.usedCount
                });
            }
        }
        Save.CommitSave();
    }

    // ════════════════════════════════════════════════════════════════════
    //  인벤토리 관리
    // ════════════════════════════════════════════════════════════════════

    /// <summary>아이템 획득 (수량 +1)</summary>
    public bool AddItem(ItemData item)
    {
        if (item == null) return false;

        ItemRuntime runtime = inventory.Find(r => r.data == item);
        if (runtime != null)
        {
            if (runtime.count >= item.maxStackCount)
            {
                Debug.Log($"{item.itemName}의 최대 소지 개수({item.maxStackCount}개)에 도달했습니다.");
                return false;
            }
            runtime.count++;
        }
        else
        {
            inventory.Add(new ItemRuntime
            {
                data      = item,
                count     = 1,
                usedCount = 0
            });
        }

        SaveToData();
        Debug.Log($"아이템 획득: {item.itemName}");
        return true;
    }

    /// <summary>아이템 보유 수량 조회</summary>
    public int GetItemCount(ItemData item)
    {
        ItemRuntime runtime = inventory.Find(r => r.data == item);
        return runtime?.count ?? 0;
    }

    // ════════════════════════════════════════════════════════════════════
    //  아이템 사용 (전투 중)
    // ════════════════════════════════════════════════════════════════════

    /// <summary>전투 중 아이템 사용. 성공하면 수량 차감·저장 처리.</summary>
    public bool UseItem(ItemData item, Unit target)
    {
        if (item == null || target == null) return false;

        ItemRuntime runtime = inventory.Find(r => r.data == item);
        if (runtime == null || runtime.count <= 0)
        {
            Debug.Log($"{item.itemName}: 보유 없음");
            return false;
        }

        if (item.maxUseCount > 0 && runtime.usedCount >= item.maxUseCount)
        {
            Debug.Log($"{item.itemName}: 연료 고갈로 사용 불가! (사용 한도 {item.maxUseCount}회 초과)");
            return false;
        }

        bool success = ApplyItemEffect(item, target, runtime);
        if (success)
        {
            runtime.count--;
            runtime.usedCount++;
            SaveToData();
            Debug.Log($"{item.itemName} 사용 완료 → {target.Unitname}");
        }

        return success;
    }

    private bool ApplyItemEffect(ItemData item, Unit target, ItemRuntime runtime)
    {
        switch (item.effectType)
        {
            // ── 1. 고철덩어리: HP 회복 (사용 한도 초과 시 연료 고갈로 사용 불가) ──
            case ItemEffectType.HealHp:
                int healed = Mathf.RoundToInt(item.healAmount * target.GetHealMultiplier());
                target.Heal(healed);
                Debug.Log($"[고철덩어리] {target.Unitname} HP +{healed} 회복 (연료 사용: {runtime.usedCount + 1}/{item.maxUseCount})");
                return true;

            // ── 2. 비상배터리: 전력(에너지) 회복 및 고철덩어리 연료 고갈 복구 ──
            case ItemEffectType.RecoverEnergy:
                target.energyCurrent = Mathf.Min(target.energyCurrent + item.energyAmount, target.energyMax);
                
                // 인벤토리 내 고철덩어리(HealHp)의 사용 횟수를 리셋하여 연료 고갈 복구
                ItemRuntime scrapRuntime = inventory.Find(r => r.data != null && r.data.effectType == ItemEffectType.HealHp);
                if (scrapRuntime != null)
                {
                    scrapRuntime.usedCount = 0;
                    Debug.Log($"[비상배터리] 고철덩어리의 연료 고갈을 복구했습니다! (사용 횟수 0으로 초기화)");
                }

                Debug.Log($"[비상배터리] {target.Unitname} 에너지 +{item.energyAmount} 회복 (현재: {target.energyCurrent}/{target.energyMax})");
                return true;

            // ── 3. 안테나: 통신도 복구 (스트레스 / 제어불능 회복) ──
            case ItemEffectType.RecoverAntenna:
                int commRecover = item.communicationAmount > 0 ? item.communicationAmount : 50;
                target.ModifyCommunication(commRecover);
                Debug.Log($"[안테나] {target.Unitname} 통신도 +{commRecover} 복구 (현재 통신도: {target.communication})");
                return true;

            // ── 4. 퓨즈: 과전력 / 영구 기절 / 퓨즈 파손 복구 ──
            case ItemEffectType.RecoverFuse:
                target.RemoveStatus(StatusType.FuseBroken);
                target.RemoveStatus(StatusType.Stun);
                target.isEmergencyMode = false;
                Debug.Log($"[퓨즈] {target.Unitname} 과전력 및 기절 상태 복구 완료");
                return true;

            // ── 5. 사포: 녹(산화) 복구 및 체력 2 감소 ──
            case ItemEffectType.SandpaperOxidation:
                if (!target.isOxidationI && !target.isOxidationII)
                {
                    Debug.Log($"{target.Unitname}은(는) 산화 상태가 아닙니다.");
                    return false;
                }
                target.RemoveStatus(StatusType.OxidationI);
                target.RemoveStatus(StatusType.OxidationII);
                int penalty = item.sandpaperHpPenalty > 0 ? item.sandpaperHpPenalty : 2;
                target.TakeDamage(penalty, Unit.DamageType.Normal);
                Debug.Log($"[사포] {target.Unitname} 산화(녹) 제거 완료 (사포 마찰로 HP -{penalty} 감소)");
                return true;

            // ── 6. 녹 제거제: 녹(산화) 완전 복구 ──
            case ItemEffectType.RemoveOxidation:
                if (!target.isOxidationI && !target.isOxidationII)
                {
                    Debug.Log($"{target.Unitname}은(는) 산화 상태가 아닙니다.");
                    return false;
                }
                target.RemoveStatus(StatusType.OxidationI);
                target.RemoveStatus(StatusType.OxidationII);
                Debug.Log($"[녹 제거제] {target.Unitname} 산화(녹) 제거 완료");
                return true;

            // ── 7. 냉각제: 과열 복구 ──
            case ItemEffectType.CoolDown:
                if (!target.isOverheat)
                {
                    Debug.Log($"{target.Unitname}은(는) 과열 상태가 아닙니다.");
                    return false;
                }
                target.RemoveStatus(StatusType.Overheat);
                Debug.Log($"[냉각제] {target.Unitname} 과열 해제 완료");
                return true;

            // ── 8. 소화기: 화재 → 과열로 전환 ──
            case ItemEffectType.ExtinguishFire:
                if (!target.isFire)
                {
                    Debug.Log($"{target.Unitname}은(는) 화재 상태가 아닙니다.");
                    return false;
                }
                target.RemoveStatus(StatusType.Fire);
                target.AddStatus(StatusType.Overheat, 3);
                Debug.Log($"[소화기] {target.Unitname} 화재 진압 완료 ➔ 과열 상태로 전환");
                return true;

            // ── 9. 솔: 무장 오염 복구 ──
            case ItemEffectType.RemovePollution:
                if (!target.isWeaponPollution)
                {
                    Debug.Log($"{target.Unitname}은(는) 무장 오염 상태가 아닙니다.");
                    return false;
                }
                target.RemoveStatus(StatusType.WeaponPollution);
                Debug.Log($"[솔] {target.Unitname} 무장 오염 청소 완료");
                return true;

            // ── 10. 절연 테이프: 합선 / 회로 단선 복구 ──
            case ItemEffectType.RemoveShortCircuit:
                if (!target.isShortCircuit && !target.isCircuitryShort)
                {
                    Debug.Log($"{target.Unitname}은(는) 합선/회로 단선 상태가 아닙니다.");
                    return false;
                }
                target.RemoveStatus(StatusType.ShortCircuit);
                target.RemoveStatus(StatusType.CircuitryShort);
                target.circuitShortStacks = 0;
                Debug.Log($"[절연 테이프] {target.Unitname} 합선 및 회로 단선 배선 수리 완료");
                return true;

            // ── 11. 방수 테이프: 누유(윤활유 유출) 복구 ──
            case ItemEffectType.RemoveOilLeak:
                if (!target.isOilLeak && !target.isLubricantLeak)
                {
                    Debug.Log($"{target.Unitname}은(는) 누유 상태가 아닙니다.");
                    return false;
                }
                target.RemoveStatus(StatusType.OilLeak);
                target.RemoveStatus(StatusType.LubricantLeak);
                Debug.Log($"[방수 테이프] {target.Unitname} 윤활유 누유 밀봉 완료");
                return true;

            default:
                Debug.LogWarning($"미구현 아이템 효과: {item.effectType}");
                return false;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  상점 강화 구매 → CurrencyManager에 위임
    // ════════════════════════════════════════════════════════════════════

    /// <summary>방어 강화 구매 — 재화 처리는 CurrencyManager에서 수행합니다.</summary>
    public void BuyDefenseUpgrade(Unit user)
    {
        if (CurrencyManager.instance != null)
            CurrencyManager.instance.BuyDefenseUpgrade(user);
    }

    /// <summary>공격 강화 구매 — 재화 처리는 CurrencyManager에서 수행합니다.</summary>
    public void BuyAttackUpgrade(Unit user)
    {
        if (CurrencyManager.instance != null)
            CurrencyManager.instance.BuyAttackUpgrade(user);
    }

    /// <summary>정확도 강화 구매 — 재화 처리는 CurrencyManager에서 수행합니다.</summary>
    public void BuyAccuracyUpgrade(Unit user)
    {
        if (CurrencyManager.instance != null)
            CurrencyManager.instance.BuyAccuracyUpgrade(user);
    }
}