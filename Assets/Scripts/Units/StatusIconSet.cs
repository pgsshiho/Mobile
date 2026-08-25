using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusIconSet", menuName = "OverCharge/StatusIconSet")]
public class StatusIconSet : ScriptableObject
{
    [Header("Icon Prefab")]
    [Tooltip("상태이상 아이콘 표시 시 복제할 UI 프리팹 (Image 컴포넌트 포함)")]
    public GameObject statusIconPrefab;

    [Header("Status Icon List")]
    [Tooltip("각 상태이상 타입별 StatusIconData 에셋 목록")]
    public List<StatusIconData> statusIconDatas = new List<StatusIconData>();

    /// <summary>
    /// 지정된 상태이상 타입에 해당하는 아이콘 스프라이트를 반환합니다.
    /// </summary>
    public Sprite GetIcon(StatusType type)
    {
        if (statusIconDatas == null) return null;

        for (int i = 0; i < statusIconDatas.Count; i++)
        {
            if (statusIconDatas[i] != null && statusIconDatas[i].statusType == type)
            {
                return statusIconDatas[i].icon;
            }
        }
        return null;
    }
}
