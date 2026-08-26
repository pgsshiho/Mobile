using System;
using UnityEngine;

// ====================================================================
//  재화 타입 정의
// ====================================================================
public enum CurrencyType
{
    Gold,       // 골드 (일반 화폐)
    Material    // 재료 (강화 및 특수 자원)
}

// ====================================================================
//  인터페이스 계층 구조
// ====================================================================

/// <summary>재화 보유량 조회 전용 인터페이스</summary>
public interface ICurrencyHolder
{
    int GetCurrency(CurrencyType type);
    bool HasEnough(CurrencyType type, int amount);
}

/// <summary>재화 획득 전용 인터페이스</summary>
public interface ICurrencyReceiver : ICurrencyHolder
{
    void AddCurrency(CurrencyType type, int amount);
}

/// <summary>재화 소비 전용 인터페이스</summary>
public interface ICurrencySpender : ICurrencyHolder
{
    bool SpendCurrency(CurrencyType type, int amount);
    bool SpendMultiple(params (CurrencyType type, int amount)[] costs);
}

/// <summary>재화 완전 관리(획득·소비·조회) 인터페이스</summary>
public interface ICurrencyWallet : ICurrencyReceiver, ICurrencySpender { }

/// <summary>재화 변동 알림(옵저버) 인터페이스</summary>
public interface ICurrencyObservable
{
    /// <summary>
    /// 재화 변동 이벤트
    /// (CurrencyType type, int delta, int newValue)
    /// </summary>
    event Action<CurrencyType, int, int> OnCurrencyChanged;
}

// ====================================================================
//  통합 재화 관리자
// ====================================================================
public class CurrencyManager : MonoBehaviour, ICurrencyWallet, ICurrencyObservable
{
    public static CurrencyManager instance;

    [Header("=== 보유 재화 (런타임) ===")]
    [SerializeField] private int _gold = 0;
    [SerializeField] private int _material = 0;

    // 프로퍼티 (직접 읽기)
    public int Gold => _gold;
    public int Material => _material;

    // 옵저버 이벤트
    public event Action<CurrencyType, int, int> OnCurrencyChanged;

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

    // ────────────────────────────────────────────────────────────────
    //  ICurrencyHolder 구현
    // ────────────────────────────────────────────────────────────────

    public int GetCurrency(CurrencyType type)
    {
        return type switch
        {
            CurrencyType.Gold     => _gold,
            CurrencyType.Material => _material,
            _ => 0
        };
    }

    public bool HasEnough(CurrencyType type, int amount)
    {
        if (amount <= 0) return true;
        return GetCurrency(type) >= amount;
    }

    // ────────────────────────────────────────────────────────────────
    //  ICurrencyReceiver 구현 (획득)
    // ────────────────────────────────────────────────────────────────

    public void AddCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return;

        int prev;
        switch (type)
        {
            case CurrencyType.Gold:
                prev = _gold;
                _gold += amount;
                Debug.Log($"[Currency] 골드 +{amount} (보유: {_gold})");
                OnCurrencyChanged?.Invoke(type, amount, _gold);
                break;

            case CurrencyType.Material:
                prev = _material;
                _material += amount;
                Debug.Log($"[Currency] 재료 +{amount} (보유: {_material})");
                OnCurrencyChanged?.Invoke(type, amount, _material);
                break;
        }

        SaveToData();
    }

    // ────────────────────────────────────────────────────────────────
    //  ICurrencySpender 구현 (소비)
    // ────────────────────────────────────────────────────────────────

    public bool SpendCurrency(CurrencyType type, int amount)
    {
        if (amount < 0) return false;
        if (amount == 0) return true;

        if (!HasEnough(type, amount))
        {
            Debug.LogWarning($"[Currency] {type} 부족! (필요: {amount}, 보유: {GetCurrency(type)})");
            return false;
        }

        switch (type)
        {
            case CurrencyType.Gold:
                _gold -= amount;
                Debug.Log($"[Currency] 골드 -{amount} (보유: {_gold})");
                OnCurrencyChanged?.Invoke(type, -amount, _gold);
                break;

            case CurrencyType.Material:
                _material -= amount;
                Debug.Log($"[Currency] 재료 -{amount} (보유: {_material})");
                OnCurrencyChanged?.Invoke(type, -amount, _material);
                break;
        }

        SaveToData();
        return true;
    }

    public bool SpendMultiple(params (CurrencyType type, int amount)[] costs)
    {
        if (costs == null || costs.Length == 0) return true;

        // 1. 모든 재화가 충분한지 사전 검사
        foreach (var (type, amount) in costs)
        {
            if (!HasEnough(type, amount))
            {
                Debug.LogWarning($"[Currency] {type} 부족으로 복합 결제 실패 (필요: {amount}, 보유: {GetCurrency(type)})");
                return false;
            }
        }

        // 2. 일괄 차감
        foreach (var (type, amount) in costs)
        {
            SpendCurrency(type, amount);
        }

        return true;
    }

    // ────────────────────────────────────────────────────────────────
    //  상점 강화 구매 로직 (기존 ItemManager에서 완전 이전)
    // ────────────────────────────────────────────────────────────────

    /// <summary>방어 강화 구매</summary>
    public bool BuyDefenseUpgrade(Unit user)
    {
        if (user == null)
        {
            Debug.LogWarning("[Upgrade] 유닛이 지정되지 않았습니다.");
            return false;
        }

        if (user.defenseLevel >= user.maxDefenseLevel)
        {
            Debug.Log($"{user.Unitname}: 이미 방어력 최대 레벨({user.maxDefenseLevel})입니다.");
            return false;
        }

        int materialCost = 30 * (user.defenseLevel + 1);

        if (!SpendCurrency(CurrencyType.Material, materialCost))
        {
            Debug.Log($"방어 강화 실패: 재료 부족 (필요: {materialCost}, 보유: {_material})");
            return false;
        }

        user.defenseLevel++;
        user.defensePower += 1;
        Debug.Log($"방어 강화 성공! Lv.{user.defenseLevel} (방어력: {user.defensePower})");
        return true;
    }

    /// <summary>공격 강화 구매</summary>
    public bool BuyAttackUpgrade(Unit user)
    {
        if (user == null)
        {
            Debug.LogWarning("[Upgrade] 유닛이 지정되지 않았습니다.");
            return false;
        }

        if (user.attackLevel >= user.maxAttackLevel)
        {
            Debug.Log($"{user.Unitname}: 이미 공격력 최대 레벨({user.maxAttackLevel})입니다.");
            return false;
        }

        int materialCost = 100 * (user.attackLevel + 1);

        if (!SpendCurrency(CurrencyType.Material, materialCost))
        {
            Debug.Log($"공격 강화 실패: 재료 부족 (필요: {materialCost}, 보유: {_material})");
            return false;
        }

        user.attackLevel++;
        user.attackPower += 2;
        Debug.Log($"공격 강화 성공! Lv.{user.attackLevel} (공격력: {user.attackPower})");
        return true;
    }

    /// <summary>정확도 강화 구매</summary>
    public bool BuyAccuracyUpgrade(Unit user)
    {
        if (user == null)
        {
            Debug.LogWarning("[Upgrade] 유닛이 지정되지 않았습니다.");
            return false;
        }

        if (user.accuracyupgrade)
        {
            Debug.Log($"{user.Unitname}: 이미 정확도 강화를 완료했습니다.");
            return false;
        }

        int materialCost = 100;

        if (!SpendCurrency(CurrencyType.Material, materialCost))
        {
            Debug.Log($"정확도 강화 실패: 재료 부족 (필요: {materialCost}, 보유: {_material})");
            return false;
        }

        user.accuracyupgrade = true;
        user.accuracy += 5;
        Debug.Log($"정확도 강화 성공! (정확도: {user.accuracy})");
        return true;
    }

    // ────────────────────────────────────────────────────────────────
    //  저장 / 로드
    // ────────────────────────────────────────────────────────────────

    public void LoadFromSave()
    {
        SaveData data = Save.GetSaveData();
        if (data == null) return;

        _gold     = data.money;
        _material = data.material;

        Debug.Log($"[Currency] 로드 완료 — 골드: {_gold}, 재료: {_material}");
    }

    public void SaveToData()
    {
        SaveData data = Save.GetSaveData();
        if (data == null) return;

        data.money    = _gold;
        data.material = _material;

        Save.CommitSave();
    }
}
