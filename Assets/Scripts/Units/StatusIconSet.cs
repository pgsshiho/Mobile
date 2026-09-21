using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusIconSet", menuName = "OverCharge/StatusIconSet")]
public class StatusIconSet : ScriptableObject
{
    [Header("Icon Prefab")]
    [Tooltip("상태이상 아이콘 표시 시 복제할 UI 프리팹 (Image 또는 SpriteRenderer 포함)")]
    public GameObject statusIconPrefab;

    [Header("Status Icon List")]
    [Tooltip("각 상태이상 타입별 StatusIconData 에셋 목록")]
    public List<StatusIconData> statusIconDatas = new List<StatusIconData>();

    /// <summary>
    /// 지정된 상태이상 타입에 해당하는 아이콘 스프라이트를 반환합니다.
    /// 직접 매핑이 없으면 유사/호환 상태이상 아이콘으로 자동 폴백합니다.
    /// </summary>
    public Sprite GetIcon(StatusType type)
    {
        if (statusIconDatas == null) return null;

        // 1. 직접 매핑 검색
        for (int i = 0; i < statusIconDatas.Count; i++)
        {
            if (statusIconDatas[i] != null && statusIconDatas[i].statusType == type && statusIconDatas[i].icon != null)
            {
                return statusIconDatas[i].icon;
            }
        }

        // 2. 호환 상태이상 타입 폴백
        StatusType fallback = type switch
        {
            StatusType.LubricantLeak     => StatusType.OilLeak,
            StatusType.CircuitryShort    => StatusType.ShortCircuit,
            StatusType.DataFragmentation => StatusType.WeaponPollution,
            StatusType.LogicLoop         => StatusType.Stun,
            StatusType.Ghosting          => StatusType.Marked,
            StatusType.Broken            => StatusType.WeaponPollution,
            StatusType.MetalFatigue      => StatusType.OxidationI,
            _ => StatusType.None
        };

        if (fallback != StatusType.None)
        {
            for (int i = 0; i < statusIconDatas.Count; i++)
            {
                if (statusIconDatas[i] != null && statusIconDatas[i].statusType == fallback && statusIconDatas[i].icon != null)
                {
                    return statusIconDatas[i].icon;
                }
            }
        }

        return null;
    }
}
