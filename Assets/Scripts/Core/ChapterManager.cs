using System;
using System.Collections.Generic;
using UnityEngine;

public class ChapterManager : MonoBehaviour
{
    public static ChapterManager instance;

    // ────────────────────────────────────────────────────────────
    //  챕터별 지역(Zone) 구성
    //  1챕: Forest, Coast
    //  2챕: Forest, Coast, Cave
    //  3챕: Forest, Coast, Cave, Basement
    //  4챕: Forest, Coast, Cave, Basement, Lab
    //  5챕: Forest, Coast, Cave, Basement, Lab, City
    // ────────────────────────────────────────────────────────────
    public static readonly Dictionary<int, ZoneType[]> ChapterZones = new Dictionary<int, ZoneType[]>
    {
        { 1, new[] { ZoneType.Forest, ZoneType.Coast } },
        { 2, new[] { ZoneType.Forest, ZoneType.Coast, ZoneType.Cave } },
        { 3, new[] { ZoneType.Forest, ZoneType.Coast, ZoneType.Cave, ZoneType.Basement } },
        { 4, new[] { ZoneType.Forest, ZoneType.Coast, ZoneType.Cave, ZoneType.Basement, ZoneType.Lab } },
        { 5, new[] { ZoneType.Forest, ZoneType.Coast, ZoneType.Cave, ZoneType.Basement, ZoneType.Lab, ZoneType.City } }
    };

    [Header("Current Run Chapter State")]
    public int currentChapter = 1;
    public int currentZoneIndex = 0;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 지정된 챕터에서 순차적으로 진행할 지역(Zone) 목록을 반환합니다.
    /// </summary>
    public static ZoneType[] GetZonesForChapter(int chapter)
    {
        chapter = Mathf.Clamp(chapter, 1, 5);
        if (ChapterZones.TryGetValue(chapter, out ZoneType[] zones))
        {
            return zones;
        }
        return new[] { ZoneType.Forest, ZoneType.Coast };
    }

    /// <summary>
    /// 현재 챕터에서 진행해야 할 현재 ZoneType을 반환합니다.
    /// </summary>
    public ZoneType GetCurrentZone()
    {
        ZoneType[] zones = GetZonesForChapter(currentChapter);
        if (currentZoneIndex >= 0 && currentZoneIndex < zones.Length)
        {
            return zones[currentZoneIndex];
        }
        return ZoneType.Forest;
    }

    /// <summary>
    /// 새로운 챕터 원정을 시작합니다.
    /// </summary>
    public void StartChapter(int chapter)
    {
        currentChapter = Mathf.Clamp(chapter, 1, 5);
        currentZoneIndex = 0;

        Debug.Log($"<color=cyan>[ChapterManager]</color> 제 {currentChapter}챕터 원정 시작! (첫 지역: {GetCurrentZone()})");
    }

    /// <summary>
    /// 보스 클리어 후 호출:
    /// 다음 지역이 남아있으면 다음 지역으로 전진하고 true 반환,
    /// 마지막 지역까지 모두 클리어했으면 챕터 클리어 처리 후 false 반환.
    /// </summary>
    public bool AdvanceNextZone()
    {
        ZoneType[] zones = GetZonesForChapter(currentChapter);
        currentZoneIndex++;

        if (currentZoneIndex < zones.Length)
        {
            ZoneType nextZone = zones[currentZoneIndex];
            Debug.Log($"<color=cyan>[ChapterManager]</color> 다음 지역으로 이동: {nextZone} (진행도: {currentZoneIndex + 1}/{zones.Length})");
            return true;
        }
        else
        {
            OnChapterCleared();
            return false;
        }
    }

    /// <summary>
    /// 챕터 최종 완료 시 처리
    /// </summary>
    private void OnChapterCleared()
    {
        Debug.Log($"<color=yellow>★ [ChapterManager] 제 {currentChapter}챕터 원정 최종 완료! ★</color>");

        // 영구 세이브에 챕터 클리어 기록
        Save.SaveChapterClear(currentChapter);

        // 현재 런 종료
        Save.EndCurrentRun();
    }
}
