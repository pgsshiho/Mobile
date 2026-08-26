using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemDropEntry
{
    [Tooltip("드랍할 아이템 데이터")]
    public ItemData item;

    [Tooltip("드랍 확률 (0 ~ 100%)")]
    [Range(0f, 100f)]
    public float dropRate = 50f;

    [Tooltip("드랍 수량")]
    [Min(1)]
    public int count = 1;
}

[CreateAssetMenu(fileName = "RoomRewardData", menuName = "OverCharge/RoomRewardData")]
public class RoomRewardData : ScriptableObject
{
    [Header("기본 재화 보상 범위")]
    public int minMoney = 50;
    public int maxMoney = 150;
    public int minMaterial = 10;
    public int maxMaterial = 30;

    [Header("드랍 아이템 목록 (개별 확률)")]
    public List<ItemDropEntry> dropItems = new List<ItemDropEntry>();

    /// <summary>
    /// 설정된 확률에 따라 골드, 재료, 드랍 아이템 목록을 롤링합니다.
    /// </summary>
    public (int money, int material, List<ItemData> items) RollRewards()
    {
        int rolledMoney = (maxMoney >= minMoney) ? UnityEngine.Random.Range(minMoney, maxMoney + 1) : minMoney;
        int rolledMaterial = (maxMaterial >= minMaterial) ? UnityEngine.Random.Range(minMaterial, maxMaterial + 1) : minMaterial;

        List<ItemData> rolledItems = new List<ItemData>();
        if (dropItems != null)
        {
            foreach (var entry in dropItems)
            {
                if (entry == null || entry.item == null) continue;

                float roll = UnityEngine.Random.Range(0f, 100f);
                if (roll <= entry.dropRate)
                {
                    int dropCount = Mathf.Max(1, entry.count);
                    for (int i = 0; i < dropCount; i++)
                    {
                        rolledItems.Add(entry.item);
                    }
                }
            }
        }

        return (rolledMoney, rolledMaterial, rolledItems);
    }
}
