using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 챕터 안의 지역 진행 순서와 챕터 완료 처리를 담당한다.
/// 실제 진행 값은 SaveData가 소유하며, 이 컴포넌트는 런타임에서 이를 읽고 갱신한다.
/// </summary>
public class ChapterManager : MonoBehaviour
{
    [Serializable]
    public class ChapterRoute
    {
        [Min(1)] public int chapter = 1;
        public List<ZoneType> zones = new List<ZoneType>();
    }

    public static ChapterManager instance;

    [Header("Progress")]
    [Min(1)] public int currentChapter = 1;
    [Min(0)] public int currentZoneIndex;

    [Header("Chapter Routes")]
    [Tooltip("비어 있으면 기본 5개 챕터 경로를 사용합니다.")]
    [SerializeField] private List<ChapterRoute> chapterRoutes = new List<ChapterRoute>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureDefaultRoutes();
        LoadProgressFromSave();
    }

    /// <summary>새 런의 시작 챕터와 첫 지역을 설정합니다.</summary>
    public void StartChapter(int chapter)
    {
        currentChapter = Mathf.Clamp(chapter, 1, GetMaximumChapter());
        currentZoneIndex = 0;
        NormalizeProgress();
    }

    /// <summary>현재 챕터의 현재 지역입니다.</summary>
    public ZoneType GetCurrentZone()
    {
        NormalizeProgress();
        ChapterRoute route = GetRoute(currentChapter);
        return route.zones[currentZoneIndex];
    }

    /// <summary>
    /// 보스방 클리어 후 다음 지역으로 이동합니다.
    /// 다음 지역이 없으면 챕터 완료를 저장하고 false를 반환합니다.
    /// </summary>
    public bool AdvanceNextZone()
    {
        NormalizeProgress();
        ChapterRoute route = GetRoute(currentChapter);

        if (currentZoneIndex + 1 < route.zones.Count)
        {
            currentZoneIndex++;
            SaveProgress();
            return true;
        }

        Save.SaveChapterClear(currentChapter);
        Save.EndCurrentRun();
        return false;
    }

    public int GetCurrentZoneCount()
    {
        return GetRoute(currentChapter).zones.Count;
    }

    public bool IsFinalZone()
    {
        return currentZoneIndex >= GetCurrentZoneCount() - 1;
    }

    /// <summary>메뉴나 씬 로드 후 저장된 진행 값을 런타임에 반영합니다.</summary>
    public void LoadProgressFromSave()
    {
        SaveData data = Save.GetSaveData();
        if (data == null)
            return;

        currentChapter = data.currentChapter;
        currentZoneIndex = data.currentZoneIndex;
        NormalizeProgress();
    }

    private void SaveProgress()
    {
        SaveData data = Save.GetSaveData();
        data.currentChapter = currentChapter;
        data.currentZoneIndex = currentZoneIndex;
        data.currentZone = (int)GetCurrentZone();
        data.hasActiveRun = true;
        Save.CommitSave();
    }

    private void NormalizeProgress()
    {
        currentChapter = Mathf.Clamp(currentChapter, 1, GetMaximumChapter());
        ChapterRoute route = GetRoute(currentChapter);
        currentZoneIndex = Mathf.Clamp(currentZoneIndex, 0, route.zones.Count - 1);
    }

    private int GetMaximumChapter()
    {
        int max = 1;
        foreach (ChapterRoute route in chapterRoutes)
        {
            if (route != null)
                max = Mathf.Max(max, route.chapter);
        }
        return max;
    }

    private ChapterRoute GetRoute(int chapter)
    {
        foreach (ChapterRoute route in chapterRoutes)
        {
            if (route != null && route.chapter == chapter && route.zones != null && route.zones.Count > 0)
                return route;
        }

        // 잘못된 인스펙터 값이 있어도 저장/로드가 중단되지 않도록 안전한 기본 경로를 사용한다.
        return chapterRoutes[0];
    }

    private void EnsureDefaultRoutes()
    {
        bool hasValidRoute = false;
        foreach (ChapterRoute route in chapterRoutes)
        {
            if (route != null && route.zones != null && route.zones.Count > 0)
            {
                hasValidRoute = true;
                break;
            }
        }

        if (hasValidRoute)
            return;

        chapterRoutes = new List<ChapterRoute>
        {
            CreateRoute(1, ZoneType.Forest, ZoneType.Coast),
            CreateRoute(2, ZoneType.Cave, ZoneType.Basement),
            CreateRoute(3, ZoneType.Lab, ZoneType.City),
            CreateRoute(4, ZoneType.Forest, ZoneType.Cave, ZoneType.Lab),
            CreateRoute(5, ZoneType.Coast, ZoneType.Basement, ZoneType.City)
        };
    }

    private static ChapterRoute CreateRoute(int chapter, params ZoneType[] zones)
    {
        return new ChapterRoute
        {
            chapter = chapter,
            zones = new List<ZoneType>(zones)
        };
    }
}
