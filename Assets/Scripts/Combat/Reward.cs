using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Reward : MonoBehaviour
{
    public static Reward Instance;

    [Header("UI Panel")]
    public GameObject Rewardpanel;
    public TextMeshProUGUI MoneyT;
    public TextMeshProUGUI MaterialT;

    [Tooltip("아이템 이름을 띄울 TextMeshPro 3개 (Element 0, 1, 2)")]
    public TextMeshProUGUI[] ItemT;

    [Header("현재 보상 데이터 (런타임)")]
    public int PendingGold;
    public int PendingMaterial;
    public List<ItemData> PendingItems = new List<ItemData>();

    public void Awake()
    {
        Instance = this;
    }

    // ────────────────────────────────────────────────────────────────
    //  보상 데이터 세팅 (Room 등 외부에서 호출)
    // ────────────────────────────────────────────────────────────────

    /// <summary>보상 데이터를 설정하고 UI를 갱신합니다.</summary>
    public void SetRewards(int gold, int material, List<ItemData> items)
    {
        PendingGold     = gold;
        PendingMaterial = material;
        PendingItems    = (items != null) ? new List<ItemData>(items) : new List<ItemData>();
    }

    // ────────────────────────────────────────────────────────────────
    //  UI 열기 / 닫기
    // ────────────────────────────────────────────────────────────────

    /// <summary>보상 창을 열고 재화·아이템 텍스트를 표시합니다.</summary>
    public void RewardOpen()
    {
        if (Rewardpanel == null) return;

        Rewardpanel.SetActive(true);

        if (MoneyT    != null) MoneyT.text    = $"골드 : {PendingGold}";
        if (MaterialT != null) MaterialT.text = $"재료 : {PendingMaterial}";

        RefreshItemTextUI();
    }

    /// <summary>보상 창을 닫습니다.</summary>
    public void CancelReward()
    {
        if (Rewardpanel != null)
            Rewardpanel.SetActive(false);
    }

    // ────────────────────────────────────────────────────────────────
    //  수락 / 거절
    // ────────────────────────────────────────────────────────────────

    public void AcceptReward(bool accept)
    {
        if (accept)
            ClaimRewards();
        else
            CancelReward();
    }

    // ────────────────────────────────────────────────────────────────
    //  보상 실제 지급 (CurrencyManager & ItemManager로 위임)
    // ────────────────────────────────────────────────────────────────

    /// <summary>보상을 실제로 플레이어에게 지급합니다.</summary>
    public void ClaimRewards()
    {
        // ── 재화 지급 (CurrencyManager에 위임) ──────────────────────
        if (CurrencyManager.instance != null)
        {
            if (PendingGold > 0)
                CurrencyManager.instance.AddCurrency(CurrencyType.Gold, PendingGold);

            if (PendingMaterial > 0)
                CurrencyManager.instance.AddCurrency(CurrencyType.Material, PendingMaterial);
        }

        // ── 아이템 지급 (ItemManager에 위임) ─────────────────────────
        if (ItemManager.Instance != null && PendingItems != null)
        {
            foreach (ItemData item in PendingItems)
            {
                if (item != null)
                {
                    ItemManager.Instance.AddItem(item);
                    Debug.Log($"[Reward] 아이템 [{item.itemName}] 획득");
                }
            }
        }

        // ── 창 닫기 ──────────────────────────────────────────────────
        CancelReward();
    }

    // ────────────────────────────────────────────────────────────────
    //  유틸
    // ────────────────────────────────────────────────────────────────

    /// <summary>동적으로 보상 아이템을 추가합니다.</summary>
    public void AddRewardItem(ItemData item)
    {
        if (item != null)
            PendingItems.Add(item);
    }

    private void RefreshItemTextUI()
    {
        if (ItemT == null || ItemT.Length < 3) return;

        for (int i = 0; i < ItemT.Length; i++)
            if (ItemT[i] != null) ItemT[i].text = "";

        if (PendingItems == null || PendingItems.Count == 0) return;

        int total = PendingItems.Count;
        if (total <= 3)
        {
            for (int i = 0; i < total; i++)
                if (PendingItems[i] != null && ItemT[i] != null)
                    ItemT[i].text = PendingItems[i].itemName;
        }
        else
        {
            if (PendingItems[0] != null) ItemT[0].text = PendingItems[0].itemName;
            if (PendingItems[1] != null) ItemT[1].text = PendingItems[1].itemName;
            ItemT[2].text = $"외 {total - 2}개";
        }
    }
}
