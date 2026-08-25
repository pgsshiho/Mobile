using System;
using System.Collections.Generic;
using UnityEngine;

// ════════════════════════════════════════════════════════════════════════
//  재화 타입 정의
// ════════════════════════════════════════════════════════════════════════

/// <summary>게임 내 재화 종류</summary>
public enum CurrencyType
{
    Gold,       // 골드 (기본 재화)
    Material,   // 재료/고철 (강화 재화)
}

// ════════════════════════════════════════════════════════════════════════
//  재화 관련 인터페이스 (결합도 최소화)
// ════════════════════════════════════════════════════════════════════════

/// <summary>재화를 보유하고 읽을 수 있는 객체</summary>
public interface ICurrencyHolder
{
    int GetCurrency(CurrencyType type);
    bool HasEnough(CurrencyType type, int amount);
}

/// <summary>재화를 지급(획득)할 수 있는 객체</summary>
public interface ICurrencyReceiver : ICurrencyHolder
{
    void AddCurrency(CurrencyType type, int amount);
}

/// <summary>재화를 소비할 수 있는 객체</summary>
public interface ICurrencySpender : ICurrencyHolder
{
    /// <returns>소비 성공 여부</returns>
    bool SpendCurrency(CurrencyType type, int amount);
}

/// <summary>재화를 획득·소비 모두 가능한 객체 (ICurrencyReceiver + ICurrencySpender)</summary>
public interface ICurrencyWallet : ICurrencyReceiver, ICurrencySpender { }

/// <summary>재화 변동 이벤트를 외부에 알릴 수 있는 객체</summary>
public interface ICurrencyObservable
{
    event Action<CurrencyType, int, int> OnCurrencyChanged; // (type, oldValue, newValue)
}

// ════════════════════════════════════════════════════════════════════════
//  CurrencyManager  ─  재화의 유일한 진실 소스(Single Source of Truth)
// ════════════════════════════════════════════════════════════════════════

/// <summary>
/// 게임 내 모든 재화(골드, 재료)를 중앙 관리합니다.
///
/// 사용법:
///   재화 획득  → CurrencyManager.instance.AddCurrency(CurrencyType.Gold, 100);
///   재화 소비  → CurrencyManager.instance.SpendCurrency(CurrencyType.Material, 30);
///   재화 조회  → CurrencyManager.instance.GetCurrency(CurrencyType.Gold);
///   이벤트 구독 → CurrencyManager.instance.OnCurrencyChanged += (type, old, next) => ...;
///
/// 저장/로드는 SaveData(Save.cs)와 자동 연동됩니다.
/// </summary>
public class CurrencyManager : MonoBehaviour, ICurrencyWallet, ICurrencyObservable
{
    public static CurrencyManager instance;

    // ──── 이벤트 ────────────────────────────────────────────────────────
    /// <summary>재화가 변동될 때 발생. (CurrencyType, 변경 전 값, 변경 후 값)</summary>
    public event Action<CurrencyType, int, int> OnCurrencyChanged;

    // ──── 내부 저장소 ─────────────────────────────────────────────────
    [Header("재화 현황 (런타임)")]
    [SerializeField] private int _gold = 0;
    [SerializeField] private int _material = 0;

    // ──── 프로퍼티 (읽기 전용 외부 노출) ──────────────────────────────
    public int Gold => _gold;
    public int Material => _material;

    // ════════════════════════════════════════════════════════════════════
    //  Unity 생명주기
    // ════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        LoadFromSave();
    }

    // ════════════════════════════════════════════════════════════════════
    //  ICurrencyHolder 구현
    // ════════════════════════════════════════════════════════════════════

    public int GetCurrency(CurrencyType type)
    {
        return type switch
        {
            CurrencyType.Gold     => _gold,
            CurrencyType.Material => _material,
            _                     => 0
        };
    }

    public bool HasEnough(CurrencyType type, int amount)
    {
        return GetCurrency(type) >= amount;
    }

    // ════════════════════════════════════════════════════════════════════
    //  ICurrencyReceiver 구현 (재화 획득)
    // ════════════════════════════════════════════════════════════════════

    /// <summary>재화를 획득합니다.</summary>
    public void AddCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return;

        int before = GetCurrency(type);
        SetRaw(type, before + amount);
        int after = GetCurrency(type);

        OnCurrencyChanged?.Invoke(type, before, after);
        Debug.Log($"[CurrencyManager] {type} +{amount} ({before} → {after})");
        SaveToData();
    }

    // ════════════════════════════════════════════════════════════════════
    //  ICurrencySpender 구현 (재화 소비)
    // ════════════════════════════════════════════════════════════════════

    /// <summary>재화를 소비합니다. 잔액 부족 시 false 반환.</summary>
    public bool SpendCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return false;

        int before = GetCurrency(type);
        if (before < amount)
        {
            Debug.Log($"[CurrencyManager] {type} 부족 (보유: {before}, 필요: {amount})");
            return false;
        }

        SetRaw(type, before - amount);
        int after = GetCurrency(type);

        OnCurrencyChanged?.Invoke(type, before, after);
        Debug.Log($"[CurrencyManager] {type} -{amount} ({before} → {after})");
        SaveToData();
        return true;
    }

    /// <summary>여러 재화를 동시에 소비합니다. 모든 재화가 충분한 경우에만 소비됩니다.</summary>
    public bool SpendMultiple(params (CurrencyType type, int amount)[] costs)
    {
        // 1. 모든 조건 검증
        foreach (var cost in costs)
        {
            if (!HasEnough(cost.type, cost.amount))
            {
                Debug.Log($"[CurrencyManager] {cost.type} 부족 (보유: {GetCurrency(cost.type)}, 필요: {cost.amount})");
                return false;
            }
        }

        // 2. 전부 통과하면 일괄 차감
        foreach (var cost in costs)
        {
            int before = GetCurrency(cost.type);
            SetRaw(cost.type, before - cost.amount);
            int after = GetCurrency(cost.type);
            OnCurrencyChanged?.Invoke(cost.type, before, after);
            Debug.Log($"[CurrencyManager] {cost.type} -{cost.amount} ({before} → {after})");
        }

        SaveToData();
        return true;
    }

    // ════════════════════════════════════════════════════════════════════
    //  강화 구매 헬퍼 (상점·대장간 연동)
    // ════════════════════════════════════════════════════════════════════

    /// <summary>방어 강화 구매 (골드 + 재료 동시 차감)</summary>
    public bool BuyDefenseUpgrade(Unit user)
    {
        if (user.defenseLevel >= user.maxDefenseLevel)
        {
            Debug.Log("[CurrencyManager] 방어 최대 레벨");
            return false;
        }

        int goldCost     = 50  * (user.defenseLevel + 1);
        int materialCost = 30  * (user.defenseLevel + 1);

        if (!SpendMultiple((CurrencyType.Gold, goldCost), (CurrencyType.Material, materialCost)))
            return false;

        user.defenseLevel++;
        user.defensePower += 3;
        Debug.Log($"[CurrencyManager] {user.name} 방어 강화 Lv.{user.defenseLevel}");
        return true;
    }

    /// <summary>공격 강화 구매 (골드 + 재료 동시 차감)</summary>
    public bool BuyAttackUpgrade(Unit user)
    {
        if (user.attackLevel >= user.maxAttackLevel)
        {
            Debug.Log("[CurrencyManager] 공격 최대 레벨");
            return false;
        }

        int goldCost     = 100 * (user.attackLevel + 1);
        int materialCost = 100 * (user.attackLevel + 1);

        if (!SpendMultiple((CurrencyType.Gold, goldCost), (CurrencyType.Material, materialCost)))
            return false;

        user.attackLevel++;
        user.attackPower += 5;
        Debug.Log($"[CurrencyManager] {user.name} 공격 강화 Lv.{user.attackLevel}");
        return true;
    }

    /// <summary>정확도 강화 구매 (골드 + 재료 동시 차감, 1회성)</summary>
    public bool BuyAccuracyUpgrade(Unit user)
    {
        if (user.accuracyupgrade)
        {
            Debug.Log("[CurrencyManager] 정확도 이미 강화됨");
            return false;
        }

        if (!SpendMultiple((CurrencyType.Gold, 1500), (CurrencyType.Material, 1000)))
            return false;

        user.accuracyupgrade = true;
        user.accuracy += 10;
        Debug.Log($"[CurrencyManager] {user.name} 정확도 강화 완료");
        return true;
    }

    // ════════════════════════════════════════════════════════════════════
    //  저장 / 로드 (Save.cs 연동)
    // ════════════════════════════════════════════════════════════════════

    public void LoadFromSave()
    {
        SaveData data = Save.GetSaveData();
        if (data == null) return;

        SetRaw(CurrencyType.Gold,     data.money);
        SetRaw(CurrencyType.Material, data.material);
        Debug.Log($"[CurrencyManager] 로드 완료 — 골드: {_gold}, 재료: {_material}");
    }

    public void SaveToData()
    {
        SaveData data = Save.GetSaveData();
        if (data == null) return;

        data.money    = _gold;
        data.material = _material;
        Save.CommitSave();
    }

    // ════════════════════════════════════════════════════════════════════
    //  내부 유틸
    // ════════════════════════════════════════════════════════════════════

    private void SetRaw(CurrencyType type, int value)
    {
        value = Mathf.Max(0, value);
        switch (type)
        {
            case CurrencyType.Gold:     _gold     = value; break;
            case CurrencyType.Material: _material = value; break;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  레거시 호환 프로퍼티 (기존 코드가 ItemManager.Instance.money를 참조할 때)
    //  → 점진적 마이그레이션 완료 후 제거 예정
    // ════════════════════════════════════════════════════════════════════

    [Obsolete("ItemManager.money 대신 CurrencyManager.instance.GetCurrency(CurrencyType.Gold) 사용")]
    public int money
    {
        get => _gold;
        set => SetRaw(CurrencyType.Gold, value);
    }

    [Obsolete("ItemManager.material 대신 CurrencyManager.instance.GetCurrency(CurrencyType.Material) 사용")]
    public int material
    {
        get => _material;
        set => SetRaw(CurrencyType.Material, value);
    }
}
