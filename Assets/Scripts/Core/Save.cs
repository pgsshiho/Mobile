using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

/// <summary>
/// 유저의 모든 중요 데이터를 하나의 구조화된 객체로 관리하는 DTO
/// </summary>
[Serializable]
public class SaveData
{
    public int language = 0;
    public string savedScene = "Factory";
    public int currentNodeId = -1;
    public int currentZone = 0;
    public List<string> partySlotUnitNames = new List<string> { "", "", "", "" };
    public int money = 0;
    public int material = 0;
    public List<SavedItemEntry> inventory = new List<SavedItemEntry>();
}

/// <summary>
/// 인벤토리 아이템 저장용 단위 데이터
/// </summary>
[Serializable]
public class SavedItemEntry
{
    public string itemName;
    public int count;
    public int usedCount;
}

public class Save : MonoBehaviour
{
    public static Save instance;

    // JSON을 담는 단일 PlayerPrefs 키
    public const string KEY_SAVE_DATA = "SaveData_JSON";

    private static SaveData cachedData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // JSON 직렬화 & 로드 코어
    // ==========================================

    /// <summary>
    /// 현재 세이브 데이터 객체를 반환 (메모리 캐시 또는 PlayerPrefs의 JSON에서 역직렬화)
    /// </summary>
    public static SaveData GetSaveData()
    {
        if (cachedData != null)
            return cachedData;

        if (PlayerPrefs.HasKey(KEY_SAVE_DATA))
        {
            string json = PlayerPrefs.GetString(KEY_SAVE_DATA, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    cachedData = JsonUtility.FromJson<SaveData>(json);
                    if (cachedData != null)
                    {
                        EnsurePartySlotsCapacity(cachedData);
                        return cachedData;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Save] JSON 세이브 데이터 파싱 실패. 새 데이터를 생성합니다: {e.Message}");
                }
            }
        }

        // 기존 레거시 데이터 마이그레이션 또는 신규 생성
        cachedData = MigrateLegacyOrCreate();
        return cachedData;
    }

    /// <summary>
    /// 세이브 데이터 객체를 JSON으로 직렬화하여 PlayerPrefs에 저장
    /// </summary>
    public static void CommitSave()
    {
        SaveData data = GetSaveData();
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        PlayerPrefs.SetString(KEY_SAVE_DATA, json);
        PlayerPrefs.Save();
        Debug.Log("[Save] JSON 세이브 데이터 커밋 완료");
    }

    private static void EnsurePartySlotsCapacity(SaveData data, int slotCount = 4)
    {
        if (data.partySlotUnitNames == null)
            data.partySlotUnitNames = new List<string>();

        while (data.partySlotUnitNames.Count < slotCount)
        {
            data.partySlotUnitNames.Add("");
        }
    }

    private static SaveData MigrateLegacyOrCreate()
    {
        SaveData data = new SaveData();
        EnsurePartySlotsCapacity(data);

        // 이전 버전의 개별 키가 남아있는 경우 JSON으로 자동 통합
        if (PlayerPrefs.HasKey("Language"))
            data.language = PlayerPrefs.GetInt("Language", 0);

        if (PlayerPrefs.HasKey("SavedScene"))
            data.savedScene = PlayerPrefs.GetString("SavedScene", "Factory");

        if (PlayerPrefs.HasKey("CurrentNodeId"))
            data.currentNodeId = PlayerPrefs.GetInt("CurrentNodeId", -1);

        for (int i = 0; i < 4; i++)
        {
            if (PlayerPrefs.HasKey($"PartySlot{i}"))
            {
                data.partySlotUnitNames[i] = PlayerPrefs.GetString($"PartySlot{i}", "");
            }
        }

        return data;
    }

    // ==========================================
    // Save & Load - Party
    // ==========================================

    /// <summary>
    /// 파티 슬롯 유닛 정보 저장 (JSON 객체에 반영 후 커밋)
    /// </summary>
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

    /// <summary>
    /// 특정 슬롯에 저장된 유닛 이름 조회
    /// </summary>
    public static string GetPartySlotUnitName(int slotIndex)
    {
        SaveData data = GetSaveData();
        if (data.partySlotUnitNames != null && slotIndex >= 0 && slotIndex < data.partySlotUnitNames.Count)
        {
            return data.partySlotUnitNames[slotIndex] ?? "";
        }
        return "";
    }

    /// <summary>
    /// 저장된 파티 데이터가 하나라도 존재하는지 확인
    /// </summary>
    public static bool HasPartySaveData(int slotCount = 4)
    {
        SaveData data = GetSaveData();
        if (data.partySlotUnitNames == null) return false;

        int limit = Mathf.Min(slotCount, data.partySlotUnitNames.Count);
        for (int i = 0; i < limit; i++)
        {
            if (!string.IsNullOrEmpty(data.partySlotUnitNames[i]))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 파티 저장 데이터 초기화
    /// </summary>
    public static void ClearPartyData(int slotCount = 4)
    {
        SaveData data = GetSaveData();
        EnsurePartySlotsCapacity(data, slotCount);
        for (int i = 0; i < slotCount; i++)
        {
            data.partySlotUnitNames[i] = "";
        }
        CommitSave();
    }

    // ==========================================
    // Save & Load - Scene & Node
    // ==========================================

    /// <summary>
    /// 저장된 씬 이름 저장
    /// </summary>
    public static void SaveSavedScene(string sceneName)
    {
        SaveData data = GetSaveData();
        data.savedScene = sceneName;
        CommitSave();
    }

    /// <summary>
    /// 저장된 씬 이름 조회 (기본값: Factory)
    /// </summary>
    public static string GetSavedScene(string defaultScene = "Factory")
    {
        SaveData data = GetSaveData();
        return string.IsNullOrEmpty(data.savedScene) ? defaultScene : data.savedScene;
    }

    /// <summary>
    /// 현재 노드 ID 저장
    /// </summary>
    public static void SaveCurrentNodeId(int nodeId)
    {
        SaveData data = GetSaveData();
        data.currentNodeId = nodeId;
        CommitSave();
    }

    /// <summary>
    /// 현재 노드 ID 조회
    /// </summary>
    public static int GetCurrentNodeId(int defaultNodeId = -1)
    {
        SaveData data = GetSaveData();
        return data.currentNodeId >= 0 ? data.currentNodeId : defaultNodeId;
    }

    // ==========================================
    // Save & Load - Language
    // ==========================================

    /// <summary>
    /// 언어 설정 인덱스 저장
    /// </summary>
    public static void SaveLanguage(int languageIndex)
    {
        SaveData data = GetSaveData();
        data.language = languageIndex;
        CommitSave();
    }

    /// <summary>
    /// 언어 설정 인덱스 조회
    /// </summary>
    public static int GetLanguage(int defaultLanguage = 0)
    {
        SaveData data = GetSaveData();
        return data.language;
    }

    // ==========================================
    // Integrated Game Save & Load
    // ==========================================

    /// <summary>
    /// 게임 전체 데이터 일괄 저장 (언어, 노드, 파티, 씬, 재화, 인벤토리 등)
    /// </summary>
    public void SaveGame()
    {
        SaveData data = GetSaveData();

        data.language = GetCurrentLanguageIndex();

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

    /// <summary>
    /// 게임 전체 데이터 일괄 불러오기
    /// </summary>
    public void LoadGame()
    {
        SaveData data = GetSaveData();

        ChangeLanguage(data.language);

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

        Debug.Log("게임 전체 데이터 JSON 불러오기 완료");
    }

    /// <summary>
    /// 모든 저장 데이터 초기화
    /// </summary>
    public static void ClearAllData()
    {
        cachedData = new SaveData();
        EnsurePartySlotsCapacity(cachedData);
        PlayerPrefs.DeleteKey(KEY_SAVE_DATA);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("모든 저장 데이터 초기화 완료");
    }

    // ==========================================
    // Helper Methods
    // ==========================================

    int GetCurrentLanguageIndex()
    {
        if (LocalizationSettings.AvailableLocales == null || 
            LocalizationSettings.AvailableLocales.Locales.Count == 0)
            return 0;

        return LocalizationSettings.SelectedLocale ==
            LocalizationSettings.AvailableLocales.Locales[0]
            ? 0
            : 1;
    }

    void ChangeLanguage(int index)
    {
        if (LocalizationSettings.AvailableLocales == null ||
            index < 0 ||
            index >= LocalizationSettings.AvailableLocales.Locales.Count)
            return;

        LocalizationSettings.SelectedLocale =
            LocalizationSettings.AvailableLocales.Locales[index];
    }
}