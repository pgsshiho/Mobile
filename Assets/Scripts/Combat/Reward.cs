
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

    [Tooltip("아이템 이름을 표시할 TextMeshPro 배열")]
    public TextMeshProUGUI[] ItemT;

    [Header("현재 보상 데이터")]
    public int PendingGold;
    public int PendingMaterial;

    // 기존 시스템과의 호환성을 위해 ItemData 목록을 유지한다.
    public List<ItemData> PendingItems = new List<ItemData>();

    // PendingItems의 같은 인덱스에 대응하는 아이템 수량이다.
    public List<int> PendingItemAmounts = new List<int>();

    private bool isClaiming;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[Reward] 중복된 Reward 인스턴스를 제거합니다.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 기존 호출 방식과 호환되는 보상 설정.
    /// 각 아이템의 수량은 1개로 설정한다.
    /// </summary>
    public void SetRewards(int gold, int material, List<ItemData> items)
    {
        SetRewards(gold, material, items, null);
    }

    /// <summary>
    /// 재화, 아이템 및 아이템별 수량을 설정한다.
    /// amounts가 없거나 해당 인덱스의 수량이 없으면 1개로 설정한다.
    /// </summary>
    public void SetRewards(
        int gold,
        int material,
        List<ItemData> items,
        List<int> amounts)
    {
        if (isClaiming)
        {
            Debug.LogWarning("[Reward] 보상 지급 중에는 보상 데이터를 변경할 수 없습니다.");
            return;
        }

        PendingGold = Mathf.Max(0, gold);
        PendingMaterial = Mathf.Max(0, material);

        PendingItems = new List<ItemData>();
        PendingItemAmounts = new List<int>();

        if (items != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    continue;
                }

                int amount = 1;

                if (amounts != null && i < amounts.Count)
                {
                    amount = Mathf.Max(1, amounts[i]);
                }

                PendingItems.Add(items[i]);
                PendingItemAmounts.Add(amount);
            }
        }

        RefreshItemTextUI();
    }

    /// <summary>
    /// 보상 창을 열고 현재 보상을 표시한다.
    /// </summary>
    public void RewardOpen()
    {
        if (Rewardpanel == null)
        {
            Debug.LogWarning("[Reward] Rewardpanel이 설정되지 않았습니다.");
            return;
        }

        if (isClaiming)
        {
            return;
        }

        Rewardpanel.SetActive(true);

        if (MoneyT != null)
        {
            MoneyT.text = $"골드 : {PendingGold}";
        }

        if (MaterialT != null)
        {
            MaterialT.text = $"재료 : {PendingMaterial}";
        }

        RefreshItemTextUI();
    }

    /// <summary>
    /// 보상 창을 닫는다.
    /// 보상 데이터 자체는 유지한다.
    /// </summary>
    public void CancelReward()
    {
        if (Rewardpanel != null)
        {
            Rewardpanel.SetActive(false);
        }
    }

    /// <summary>
    /// 수락이면 보상을 지급하고, 거절이면 창을 닫는다.
    /// </summary>
    public void AcceptReward(bool accept)
    {
        if (isClaiming)
        {
            return;
        }

        if (accept)
        {
            ClaimRewards();
        }
        else
        {
            CancelReward();
        }
    }

    /// <summary>
    /// 보상을 실제 인벤토리와 재화 시스템에 지급한다.
    /// </summary>
    public void ClaimRewards()
    {
        if (isClaiming)
        {
            return;
        }

        // 필요한 시스템이 없으면 보상을 소모하지 않는다.
        if (PendingItems != null && PendingItems.Count > 0
            && ItemManager.Instance == null)
        {
            Debug.LogWarning("[Reward] ItemManager를 찾을 수 없습니다.");
            return;
        }

        if ((PendingGold > 0 || PendingMaterial > 0)
            && CurrencyManager.instance == null)
        {
            Debug.LogWarning("[Reward] CurrencyManager를 찾을 수 없습니다.");
            return;
        }

        isClaiming = true;

        try
        {
            // 인벤토리 공간이 부족하면 지급하지 못한 수량을 남겨 둔다.
            int index = 0;

            while (PendingItems != null && index < PendingItems.Count)
            {
                ItemData item = PendingItems[index];

                if (item == null)
                {
                    PendingItems.RemoveAt(index);
                    PendingItemAmounts.RemoveAt(index);
                    continue;
                }

                int requestedAmount = 1;

                if (index < PendingItemAmounts.Count)
                {
                    requestedAmount = Mathf.Max(1, PendingItemAmounts[index]);
                }

                bool success = ItemManager.Instance.TryAddItem(
                    item,
                    requestedAmount,
                    out int addedAmount
                );

                addedAmount = Mathf.Clamp(addedAmount, 0, requestedAmount);

                if (addedAmount > 0)
                {
                    Debug.Log(
                        $"[Reward] {item.itemName} {addedAmount}개 지급"
                    );
                }

                int remainingAmount = requestedAmount - addedAmount;

                if (remainingAmount <= 0)
                {
                    PendingItems.RemoveAt(index);
                    PendingItemAmounts.RemoveAt(index);
                }
                else
                {
                    // 실제로 지급하지 못한 수량만 보상에 남긴다.
                    if (index < PendingItemAmounts.Count)
                    {
                        PendingItemAmounts[index] = remainingAmount;
                    }
                    else
                    {
                        PendingItemAmounts.Add(remainingAmount);
                    }

                    Debug.LogWarning(
                        $"[Reward] {item.itemName} 남은 수량: {remainingAmount}. " +
                        "인벤토리 공간을 확인해 주세요."
                    );

                    index++;
                }

                // 실패했더라도 다른 아이템은 지급을 시도한다.
                if (!success && addedAmount == 0)
                {
                    continue;
                }
            }

            // 아이템을 전부 지급하지 못했다면 재화는 아직 지급하지 않는다.
            // 남은 아이템을 다음 수락 시도에서 다시 처리할 수 있다.
            if (PendingItems != null && PendingItems.Count > 0)
            {
                RefreshItemTextUI();
                return;
            }

            // 아이템 보상이 모두 처리된 뒤 재화를 지급한다.
            if (CurrencyManager.instance != null)
            {
                if (PendingGold > 0)
                {
                    CurrencyManager.instance.AddCurrency(
                        CurrencyType.Gold,
                        PendingGold
                    );
                }

                if (PendingMaterial > 0)
                {
                    CurrencyManager.instance.AddCurrency(
                        CurrencyType.Material,
                        PendingMaterial
                    );
                }
            }

            // 지급이 완료된 보상 데이터를 초기화한다.
            PendingGold = 0;
            PendingMaterial = 0;

            PendingItems.Clear();
            PendingItemAmounts.Clear();

            RefreshItemTextUI();
            CancelReward();

            Debug.Log("[Reward] 모든 보상 지급 완료");
        }
        finally
        {
            isClaiming = false;
        }
    }

    /// <summary>
    /// 기존 코드에서 아이템 보상을 추가할 때 사용한다.
    /// </summary>
    public void AddRewardItem(ItemData item)
    {
        AddRewardItem(item, 1);
    }

    /// <summary>
    /// 지정된 수량의 아이템을 보상에 추가한다.
    /// </summary>
    public void AddRewardItem(ItemData item, int amount)
    {
        if (isClaiming)
        {
            Debug.LogWarning("[Reward] 보상 지급 중에는 아이템을 추가할 수 없습니다.");
            return;
        }

        if (item == null || amount <= 0)
        {
            return;
        }

        PendingItems.Add(item);
        PendingItemAmounts.Add(amount);

        RefreshItemTextUI();
    }

    /// <summary>
    /// 보상 UI의 아이템 목록을 갱신한다.
    /// </summary>
    private void RefreshItemTextUI()
    {
        if (ItemT == null)
        {
            return;
        }

        for (int i = 0; i < ItemT.Length; i++)
        {
            if (ItemT[i] != null)
            {
                ItemT[i].text = "";
            }
        }

        if (PendingItems == null || PendingItems.Count == 0)
        {
            return;
        }

        int displayCount = Mathf.Min(PendingItems.Count, ItemT.Length);

        for (int i = 0; i < displayCount; i++)
        {
            if (ItemT[i] == null || PendingItems[i] == null)
            {
                continue;
            }

            int amount = 1;

            if (PendingItemAmounts != null
                && i < PendingItemAmounts.Count)
            {
                amount = Mathf.Max(1, PendingItemAmounts[i]);
            }

            if (PendingItems.Count <= ItemT.Length)
            {
                ItemT[i].text = $"{PendingItems[i].itemName} x{amount}";
            }
            else if (i == ItemT.Length - 1)
            {
                ItemT[i].text = $"외 {PendingItems.Count - i}개";
            }
            else
            {
                ItemT[i].text = $"{PendingItems[i].itemName} x{amount}";
            }
        }
    }
}
