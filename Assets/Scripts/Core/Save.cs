using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

[Serializable]
public class SavedItemEntry
{
    public string itemName;
    public int count;
    public int usedCount;
}

[Serializable]
public class SaveData
{
    public int language = 0;
    public int currentNodeId = -1;
    public int currentZone = 0;
    public int money = 0;
    public int material = 0;
    public string savedScene = "Factory";
    public List<string> partySlotUnitNames = new List<string>();
    public List<SavedItemEntry> inventory = new List<SavedItemEntry>();

    // ── 챕터 & 런 진행 데이터 ──
    public int highestClearedChapter = 0; // 클리어한 최고 챕터 (0: 없음, 1: 1챕 클리어, ...)
    public int unlockedChapter = 1;       // 해금된 최고 챕터 (기본: 1챕터 해금)
    public int currentChapter = 1;        // 현재 진행 챕터
    public int currentZoneIndex = 0;      // 현재 챕터 내 진행 중인 Zone 인덱스
    public bool hasActiveRun = false;     // 현재 진행 중인 런 세이브가 존재하는가 (이어하기 가능 여부)
}

public class Save : MonoBehaviour
{
    public static Save instance;

    private const string KEY_SAVE_DATA = "GameSaveData_JSON";
    private static SaveData cachedData;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        EnsureDataLoaded();
    }

    private static void EnsureDataLoaded()
    {
        if (cachedData != null) return;

        if (PlayerPrefs.HasKey(KEY_SAVE_DATA))
        {
            string json = PlayerPrefs.GetString(KEY_SAVE_DATA);
            try
            {
                cachedData = JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveData JSON 파싱 실패, 신규 생성: {e.Message}");
                cachedData = new SaveData();
            }
        }
        else
        {
            cachedData = new SaveData();
        }

        EnsurePartySlotsCapacity(cachedData);
    }

    public static SaveData GetSaveData()
    {
        EnsureDataLoaded();
        return cachedData;
    }

    public static void CommitSave()
    {
        EnsureDataLoaded();
        string json = JsonUtility.ToJson(cachedData, true);
        PlayerPrefs.SetString(KEY_SAVE_DATA, json);
        PlayerPrefs.Save();
    }

    private static void EnsurePartySlotsCapacity(SaveData data, int requiredCapacity = 4)
    {
        if (data == null) return;
        if (data.partySlotUnitNames == null)
            data.partySlotUnitNames = new List<string>();

        while (data.partySlotUnitNames.Count < requiredCapacity)
        {
            data.partySlotUnitNames.Add("");
        }
    }

    public static bool HasPartySaveData(int requiredSlots = 4)
    {
        SaveData data = GetSaveData();
        if (data.partySlotUnitNames == null || data.partySlotUnitNames.Count == 0)
            return false;

        for (int i = 0; i < Mathf.Min(data.partySlotUnitNames.Count, requiredSlots); i++)
        {
            if (!string.IsNullOrEmpty(data.partySlotUnitNames[i]))
                return true;
        }

        return false;
    }

    public static void SaveParty(Unit[] partySlots)
    {
        if (partySlots == null) return;

        SaveData data = GetSaveData();
        EnsurePartySlotsCapacity(data, partySlots.Length);

        for (int i = 0; i < partySlots.Length; i++)
        {
            data.partySlotUnitNames[i] = (partySlots[i] != null) ? partySlots[i].name.Replace("(Clone)", "").Trim() : "";
        }

        CommitSave();
        Debug.Log("파티 정보 저장 완료 (JSON)");
    }

    public static string GetPartySlotUnitName(int slotIndex)
    {
        SaveData data = GetSaveData();
        if (data.partySlotUnitNames != null && slotIndex >= 0 && slotIndex < data.partySlotUnitNames.Count)
        {
            return data.partySlotUnitNames[slotIndex];
        }
        return "";
    }

    public static string GetSavedScene(string defaultScene = "Factory")
    {
        SaveData data = GetSaveData();
        return string.IsNullOrEmpty(data.savedScene) ? defaultScene : data.savedScene;
    }

    public static bool HasActiveRunSave()
    {
        SaveData data = GetSaveData();
        return data != null && data.hasActiveRun;
    }

    public static void StartNewRun(int chapter)
    {
        SaveData data = GetSaveData();
        data.hasActiveRun = true;
        data.currentChapter = Mathf.Clamp(chapter, 1, 5);
        data.currentZoneIndex = 0;
        data.currentNodeId = -1;
        data.savedScene = "Factory";

        if (ChapterManager.instance != null)
        {
            ChapterManager.instance.StartChapter(data.currentChapter);
            data.currentZone = (int)ChapterManager.instance.GetCurrentZone();
        }
        else
        {
            data.currentZone = (int)ZoneType.Forest;
        }

        CommitSave();
        Debug.Log($"<color=green>[Save]</color> 제 {data.currentChapter}챕터 신규 런 시작 세이브 저장 완료");
    }

    public static void SaveChapterClear(int chapter)
    {
        SaveData data = GetSaveData();
        if (chapter > data.highestClearedChapter)
        {
            data.highestClearedChapter = chapter;
        }

        int nextUnlock = Mathf.Min(chapter + 1, 5);
        if (nextUnlock > data.unlockedChapter)
        {
            data.unlockedChapter = nextUnlock;
        }

        CommitSave();
        Debug.Log($"<color=gold>[Save]</color> 제 {chapter}챕터 클리어 완료 저장! (최고 클리어: {data.highestClearedChapter}, 해금 챕터: {data.unlockedChapter})");
    }

    public static void EndCurrentRun()
    {
        SaveData data = GetSaveData();
        data.hasActiveRun = false;
        data.currentNodeId = -1;
        CommitSave();
        Debug.Log("<color=orange>[Save]</color> 현재 런 종료 처리 완료");
    }

    public void SaveGame()
    {
        SaveData data = GetSaveData();

        data.language = GetCurrentLanguageIndex();

        if (ChapterManager.instance != null)
        {
            data.currentChapter = ChapterManager.instance.currentChapter;
            data.currentZoneIndex = ChapterManager.instance.currentZoneIndex;
            data.hasActiveRun = true;
        }

        if (RoomManager.instance != null)
        {
            data.currentNodeId = RoomManager.instance.GetCurrentNodeId();
            data.currentZone = (int)RoomManager.instance.currentZone;
        }

        if (PartyManager.instance != null)
        {
            EnsurePartySlotsCapacity(data, PartyManager.instance.partySlots.Length);
            for (int i = 0; i < PartyManager.instance.partySlots.Length; i++)
            {
                data.partySlotUnitNames[i] = PartyManager.instance.partySlots[i] != null 
                    ? PartyManager.instance.partySlots[i].name.Replace("(Clone)", "").Trim()
                    : "";
            }
        }

        // 재화 저장 (CurrencyManager)
        if (CurrencyManager.instance != null)
        {
            data.money    = CurrencyManager.instance.Gold;
            data.material = CurrencyManager.instance.Material;
        }

        // 인벤토리 저장 (ItemManager)
        if (ItemManager.Instance != null)
        {
            data.inventory.Clear();
            if (ItemManager.Instance.inventory != null)
            {
                foreach (var runtime in ItemManager.Instance.inventory)
                {
                    if (runtime?.data != null)
                    {
                        data.inventory.Add(new SavedItemEntry
                        {
                            itemName  = runtime.data.name,
                            count     = runtime.count,
                            usedCount = runtime.usedCount
                        });
                    }
                }
            }
        }

        data.savedScene = SceneManager.GetActiveScene().name;

        CommitSave();
        Debug.Log("게임 전체 데이터 JSON 저장 완료");
    }

    public void LoadGame()
    {
        SaveData data = GetSaveData();

        ChangeLanguage(data.language);

        if (ChapterManager.instance != null)
        {
            ChapterManager.instance.currentChapter = data.currentChapter;
            ChapterManager.instance.currentZoneIndex = data.currentZoneIndex;
        }

        if (RoomManager.instance != null)
        {
            RoomManager.instance.currentZone = (ZoneType)data.currentZone;
            if (data.currentNodeId >= 0)
            {
                RoomManager.instance.LoadNode(data.currentNodeId);
            }
        }

        if (PartyManager.instance != null)
        {
            PartyManager.instance.LoadParty();
        }

        // 재화 로드 (CurrencyManager)
        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.LoadFromSave();
        }

        // 아이템 로드 (ItemManager)
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.LoadFromSave();
        }

        Debug.Log("게임 전체 데이터 JSON 불러오기 완료");
    }

    public static void ClearAllData()
    {
        cachedData = new SaveData();
        EnsurePartySlotsCapacity(cachedData);
        PlayerPrefs.DeleteKey(KEY_SAVE_DATA);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("모든 저장 데이터 초기화 완료");
    }

    private int GetCurrentLanguageIndex()
    {
        if (LocalizationSettings.SelectedLocale == null || LocalizationSettings.AvailableLocales == null)
            return 0;

        return LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[0] ? 0 : 1;
    }

    private void ChangeLanguage(int index)
    {
        if (LocalizationSettings.AvailableLocales == null || LocalizationSettings.AvailableLocales.Locales.Count == 0)
            return;

        if (index < 0 || index >= LocalizationSettings.AvailableLocales.Locales.Count)
            return;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }
}