using System.Collections.Generic;
using UnityEngine;

/// <summary>전투 보상 아이템 규칙: 0~3종, 각 종류당 1~3개.</summary>
public static class BattleItemRewardGenerator
{
    public static List<ItemData> Roll(IList<ItemData> sourceItems)
    {
        List<ItemData> candidates = new List<ItemData>();
        if (sourceItems != null)
        {
            foreach (ItemData item in sourceItems)
            {
                if (item != null && !candidates.Contains(item))
                    candidates.Add(item);
            }
        }

        int typeCount = Random.Range(0, Mathf.Min(3, candidates.Count) + 1);
        List<ItemData> result = new List<ItemData>();

        for (int i = 0; i < typeCount; i++)
        {
            int pickIndex = Random.Range(0, candidates.Count);
            ItemData picked = candidates[pickIndex];
            candidates.RemoveAt(pickIndex);

            int quantity = Random.Range(1, 4);
            for (int count = 0; count < quantity; count++)
                result.Add(picked);
        }

        return result;
    }
}
