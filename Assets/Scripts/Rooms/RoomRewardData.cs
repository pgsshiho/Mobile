using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemDropEntry
{
    [Tooltip("드랍할 아이템 에셋 (ItemData)")]
    public ItemData item;

    [Range(0f, 100f)]
    [Tooltip("드랍 확률 (0% ~ 100%)")]
    public float dropRate = 100f;

    [Min(1)]
    [Tooltip("드랍 개수 (기본: 1개)")]
    public int count = 1;
}

[CreateAssetMenu(fileName = "RoomRewardData", menuName = "OverCharge/RoomRewardData")]
public class RoomRewardData : ScriptableObject
{
    [Header("골드 보상 범위 (랜덤)")]
    public int minMoney = 10;
    public int maxMoney = 30;

    [Header("재료/고철 보상 범위 (랜덤)")]
    public int minMaterial = 0;
    public int maxMaterial = 2;

    [Header("아이템 드랍 목록 (확률 0~100%)")]
    [Tooltip("이 방에서 드랍 가능한 아이템과 각 아이템의 확률을 설정합니다.")]
    public List<ItemDropEntry> dropItems = new List<ItemDropEntry>();

    /// <summary>
    /// 설정된 확률과 범위를 기반으로 보상(골드, 재료, 드랍 아이템)을 계산하여 반환합니다.
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

                // 확률 판정 (0f ~ 100f)
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
