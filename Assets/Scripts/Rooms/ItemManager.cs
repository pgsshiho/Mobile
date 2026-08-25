using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 인벤토리만 관리합니다. 재화(골드·재료)는 CurrencyManager에서 담당합니다.
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
            Debug.Log($"{item.itemName}: 사용 한도({item.maxUseCount}회) 초과");
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
            case ItemEffectType.HealHp:
                int healed = Mathf.RoundToInt(item.healAmount * target.GetHealMultiplier());
                target.Heal(healed);
                Debug.Log($"{target.Unitname} HP +{healed}");
                return true;

            case ItemEffectType.RecoverEnergy:
                target.energyCurrent = Mathf.Min(target.energyCurrent + item.energyAmount, target.energyMax);
                Debug.Log($"{target.Unitname} 에너지 +{item.energyAmount}");
                return true;

            case ItemEffectType.RecoverAntenna:
                Debug.Log($"{target.Unitname} 통신기 복구 완료");
                return true;

            case ItemEffectType.RecoverFuse:
                if (!target.isFuseBroken)
                {
                    Debug.Log($"{target.Unitname}은 퓨즈 파손 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.FuseBroken);
                Debug.Log($"{target.Unitname} 퓨즈 복구 완료");
                return true;

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

            case ItemEffectType.CoolDown:
                if (!target.isOverheat)
                {
                    Debug.Log($"{target.Unitname}은 과열 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.Overheat);
                Debug.Log($"{target.Unitname} 과열 해제 (냉각제)");
                return true;

            case ItemEffectType.ExtinguishFire:
                if (!target.isFire)
                {
                    Debug.Log($"{target.Unitname}은 화재 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.Fire);
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

            case ItemEffectType.RemovePollution:
                if (!target.isWeaponPollution)
                {
                    Debug.Log($"{target.Unitname}은 무장 오염 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.WeaponPollution);
                Debug.Log($"{target.Unitname} 무장 오염 제거 (솔)");
                return true;

            case ItemEffectType.RemoveShortCircuit:
                if (!target.isShortCircuit)
                {
                    Debug.Log($"{target.Unitname}은 합선 상태가 아닙니다");
                    return false;
                }
                target.RemoveStatus(StatusType.ShortCircuit);
                Debug.Log($"{target.Unitname} 합선 해제 (절연 테이프)");
                return true;

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

    // ════════════════════════════════════════════════════════════════════
    //  상점 강화 구매 → CurrencyManager에 위임
    //  (기존 UI 버튼에서 ItemManager.Instance.BuyXxx()를 호출하던 코드 호환 유지)
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