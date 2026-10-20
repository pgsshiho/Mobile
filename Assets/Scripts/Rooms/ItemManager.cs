using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>인벤토리, 아이템 효과, 전투 아이템 규칙을 관리합니다. UI는 이 클래스를 구독만 합니다.</summary>
public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    public event Action OnInventoryChanged;

    [Header("Inventory")]
    public List<ItemRuntime> inventory = new List<ItemRuntime>();

    private bool itemUsedThisTurn;

    public bool ItemUsedThisTurn => itemUsedThisTurn;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadFromSave();
    }

    /// <summary>새 전투 턴마다 호출됩니다. 아이템은 턴당 하나만 사용할 수 있습니다.</summary>
    public void BeginTurn()
    {
        itemUsedThisTurn = false;
    }

    public int GetItemCount(ItemData item)
    {
        return inventory.Find(runtime => runtime != null && runtime.data == item)?.count ?? 0;
    }

    public bool CanAddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        int maximum = Mathf.Max(1, item.maxStackCount);
        return GetItemCount(item) < maximum;
    }

    /// <summary>아이템 슬롯 수는 제한하지 않으며, 해당 종류의 최대 보유량까지만 추가합니다.</summary>
    public bool TryAddItem(ItemData item, int requestedAmount, out int addedAmount)
    {
        addedAmount = 0;
        if (item == null || requestedAmount <= 0)
            return false;

        ItemRuntime runtime = inventory.Find(entry => entry != null && entry.data == item);
        if (runtime == null)
        {
            runtime = new ItemRuntime { data = item, count = 0, usedCount = 0 };
            inventory.Add(runtime);
        }
        int capacity = Mathf.Max(0, Mathf.Max(1, item.maxStackCount) - runtime.count);
        addedAmount = Mathf.Min(requestedAmount, capacity);
        if (addedAmount <= 0)
            return false;

        runtime.count += addedAmount;
        SaveToData();
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool AddItem(ItemData item) => TryAddItem(item, 1, out _);

    public int GetItemCountByQuestNeed(QuestNeed needType)
    {
        int total = 0;
        foreach (ItemRuntime runtime in inventory)
        {
            if (runtime?.data != null && MatchesQuestNeed(runtime.data.effectType, needType))
                total += runtime.count;
        }
        return total;
    }

    public bool TryPurchaseItem(ItemData item, CurrencyType currency, int price, out string reason)
    {
        reason = string.Empty;
        if (!CanAddItem(item))
        {
            reason = "최대 보유량에 도달했습니다.";
            return false;
        }

        if (CurrencyManager.instance == null || !CurrencyManager.instance.HasEnough(currency, price))
        {
            reason = "재화가 부족합니다.";
            return false;
        }

        if (!CurrencyManager.instance.SpendCurrency(currency, Mathf.Max(0, price)))
        {
            reason = "결제에 실패했습니다.";
            return false;
        }

        TryAddItem(item, 1, out _);
        return true;
    }

    public bool CanUseItem(ItemData item, Unit target, out string reason)
    {
        reason = string.Empty;
        if (item == null || target == null || GetItemCount(item) <= 0)
        {
            reason = "아이템 또는 대상이 없습니다.";
            return false;
        }

        bool inBattle = BattleManager.instance != null && BattleManager.instance.isBattle;
        if (!inBattle)
            return IsPartyMember(target, out reason);

        if (!item.usableInBattle || item.effectType == ItemEffectType.HealHp)
        {
            reason = "이 아이템은 전투 중 사용할 수 없습니다.";
            return false;
        }

        if (itemUsedThisTurn)
        {
            reason = "이번 턴에는 이미 아이템을 사용했습니다.";
            return false;
        }

        if (!IsPartyMember(target, out reason))
            return false;

        if (TurnManager.instance == null || !(TurnManager.instance.currentUnit is PlayerUnit))
        {
            reason = "아군 턴에만 아이템을 사용할 수 있습니다.";
            return false;
        }

        if (target == TurnManager.instance.currentUnit)
        {
            reason = "전투 중에는 자신에게 사용할 수 없습니다.";
            return false;
        }

        return true;
    }

    public bool UseItem(ItemData item, Unit target)
    {
        if (!CanUseItem(item, target, out string reason))
        {
            Debug.Log($"[Item] 사용 불가: {reason}");
            return false;
        }

        ItemRuntime runtime = inventory.Find(entry => entry != null && entry.data == item);
        if (item.maxUseCount > 0 && runtime.usedCount >= item.maxUseCount)
        {
            Debug.Log("[Item] 사용 횟수 한도에 도달했습니다.");
            return false;
        }

        if (!ApplyItemEffect(item, target, runtime))
            return false;

        runtime.count--;
        runtime.usedCount++;
        if (BattleManager.instance != null && BattleManager.instance.isBattle)
            itemUsedThisTurn = true;

        SaveToData();
        OnInventoryChanged?.Invoke();
        return true;
    }

    private static bool IsPartyMember(Unit target, out string reason)
    {
        reason = string.Empty;
        if (PartyManager.instance?.partySlots == null || Array.IndexOf(PartyManager.instance.partySlots, target) < 0)
        {
            reason = "현재 파티원만 대상이 될 수 있습니다.";
            return false;
        }

        if (target.health <= 0)
        {
            reason = "쓰러진 파티원은 대상이 될 수 없습니다.";
            return false;
        }

        return true;
    }
private static bool MatchesQuestNeed(ItemEffectType effect, QuestNeed need)
    {
        return (effect, need) switch
        {
            (ItemEffectType.HealHp, QuestNeed.ScrapMetal) => true,
            (ItemEffectType.RecoverEnergy, QuestNeed.EmergencyBattery) => true,
            (ItemEffectType.RecoverAntenna, QuestNeed.Antenna) => true,
            (ItemEffectType.RecoverFuse, QuestNeed.Fuse) => true,
            (ItemEffectType.SandpaperOxidation, QuestNeed.Sandpaper) => true,
            (ItemEffectType.RemoveOxidation, QuestNeed.RustRemover) => true,
            (ItemEffectType.CoolDown, QuestNeed.Coolant) => true,
            (ItemEffectType.ExtinguishFire, QuestNeed.FireExtinguisher) => true,
            (ItemEffectType.RemovePollution, QuestNeed.Brush) => true,
            (ItemEffectType.RemoveShortCircuit, QuestNeed.ElectricalTape) => true,
            (ItemEffectType.RemoveOilLeak, QuestNeed.WaterproofTape) => true,
            _ => false
        };
    }

    private bool ApplyItemEffect(ItemData item, Unit target, ItemRuntime runtime)
    {
        switch (item.effectType)
        {
            case ItemEffectType.HealHp:
                target.Heal(Mathf.RoundToInt(item.healAmount * target.GetHealMultiplier()));
                return true;
            case ItemEffectType.RecoverEnergy:
                target.energyCurrent = Mathf.Min(target.energyMax, target.energyCurrent + item.energyAmount);
                ItemRuntime scrap = inventory.Find(entry => entry?.data != null && entry.data.effectType == ItemEffectType.HealHp);
                if (scrap != null) scrap.usedCount = 0;
                return true;
            case ItemEffectType.RecoverAntenna:
                target.ModifyCommunication(item.communicationAmount);
                return true;
            case ItemEffectType.RecoverFuse:
                target.RemoveStatus(StatusType.FuseBroken);
                target.RemoveStatus(StatusType.Stun);
                target.isEmergencyMode = false;
                return true;
            case ItemEffectType.SandpaperOxidation:
                if (!target.isOxidationI && !target.isOxidationII) return false;
                target.RemoveStatus(StatusType.OxidationI);
                target.RemoveStatus(StatusType.OxidationII);
                target.TakeDamage(Mathf.Max(0, item.sandpaperHpPenalty), Unit.DamageType.Normal);
                return true;
            case ItemEffectType.RemoveOxidation:
                if (!target.isOxidationI && !target.isOxidationII) return false;
                target.RemoveStatus(StatusType.OxidationI);
                target.RemoveStatus(StatusType.OxidationII);
                return true;
            case ItemEffectType.CoolDown:
                if (!target.isOverheat) return false;
                target.RemoveStatus(StatusType.Overheat);
                return true;
            case ItemEffectType.ExtinguishFire:
                if (!target.isFire) return false;
                target.RemoveStatus(StatusType.Fire);
                target.AddStatus(StatusType.Overheat, 3);
                return true;
            case ItemEffectType.RemovePollution:
                if (!target.isWeaponPollution) return false;
                target.RemoveStatus(StatusType.WeaponPollution);
                return true;
            case ItemEffectType.RemoveShortCircuit:
                if (!target.isShortCircuit && !target.isCircuitryShort) return false;
                target.RemoveStatus(StatusType.ShortCircuit);
                target.RemoveStatus(StatusType.CircuitryShort);
                target.circuitShortStacks = 0;
                return true;
            case ItemEffectType.RemoveOilLeak:
                if (!target.isOilLeak && !target.isLubricantLeak) return false;
                target.RemoveStatus(StatusType.OilLeak);
                target.RemoveStatus(StatusType.LubricantLeak);
                return true;
            default:
                return false;
        }
    }

    public void LoadFromSave()
    {
        SaveData saveData = Save.GetSaveData();
        if (saveData?.inventory == null || saveData.inventory.Count == 0)
        {
            OnInventoryChanged?.Invoke();
            return;
        }

        Dictionary<string, ItemData> catalog = new Dictionary<string, ItemData>();
        foreach (ItemRuntime runtime in inventory)
        {
            if (runtime?.data != null)
                catalog[runtime.data.name] = runtime.data;
        }

        inventory.Clear();
        foreach (SavedItemEntry saved in saveData.inventory)
        {
            if (saved == null || !catalog.TryGetValue(saved.itemName, out ItemData item))
                continue;
            inventory.Add(new ItemRuntime
            {
                data = item,
                count = Mathf.Clamp(saved.count, 0, Mathf.Max(1, item.maxStackCount)),
                usedCount = Mathf.Max(0, saved.usedCount)
            });
        }

        OnInventoryChanged?.Invoke();
    }

    public void SaveToData()
    {
        SaveData saveData = Save.GetSaveData();
        if (saveData == null) return;

        saveData.inventory.Clear();
        foreach (ItemRuntime runtime in inventory)
        {
            if (runtime?.data == null || runtime.count <= 0) continue;
            saveData.inventory.Add(new SavedItemEntry
            {
                itemName = runtime.data.name,
                count = runtime.count,
                usedCount = runtime.usedCount
            });
        }
        Save.CommitSave();
    }
}