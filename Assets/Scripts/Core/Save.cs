using System;
using System.Collections.Generic;
using System.IO;
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
public class SavedUnitEntry
{
    public string unitName;
    public float health;
    public int energyCurrent;
    public int communication;
    public bool isPowerDown;
    public bool isEmergencyMode;
    public bool hasSelfDestructed;
    public int attackLevel;
    public int defenseLevel;
    public int MaxHpLevel;
    public int accuracyUpgrade;
    public int circuitShortStacks;
    public int ghostingMissPenalty;
    public List<SavedStatusEntry> statuses = new List<SavedStatusEntry>();
}

[Serializable]
public class SavedStatusEntry
{
    public StatusType type;
    public int remainingTurns;
}

[Serializable]
public class SaveData
{
    public int saveVersion = 2;
    public long lastSavedUtcTicks;
    public int language = 0;
    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    public int currentNodeId = -1;
    public int currentZone = 0;
    public int mapZone = -1;
    public int mapSeed;
    public List<int> clearedNodeIds = new List<int>();
    public int money = 0;
    public int material = 0;
    public string savedScene = "Forest";
    public List<string> partySlotUnitNames = new List<string>();
    public List<SavedItemEntry> inventory = new List<SavedItemEntry>();
    public List<SavedUnitEntry> partyUnitStates = new List<SavedUnitEntry>();

    // ── 챕터 & 런 진행 데이터 ──
    public int highestClearedChapter = 0; // 클리어한 최고 챕터 (0: 없음, 1: 1챕 클리어, ...)
    public int unlockedChapter = 1;       // 해금된 최고 챕터 (기본: 1챕터 해금)
    public int currentChapter = 1;        // 현재 진행 챕터
    public int currentZoneIndex = 0;      // 현재 챕터 내 진행 중인 Zone 인덱스
    public bool hasActiveRun = false;     // 현재 진행 중인 런 세이브가 존재하는가 (이어하기 가능 여부)
}

/// <summary>한 슬롯의 목록 표시용 정보입니다. 실제 게임 진행은 slot_N.json에만 저장합니다.</summary>
[Serializable]
public class SaveSlotMetadata
{
    public bool exists;
    public long lastSavedUtcTicks;
    public int chapter = 1;
    public string sceneName = "Forest";
    public bool hasActiveRun;
}

/// <summary>
/// 게임 진행과 분리된 기기 공용 저장 데이터입니다.
/// 언어/사운드/마지막 선택 슬롯과 세이브 파일 목록만 보관합니다.
/// </summary>
[Serializable]
public class GlobalSaveData
{
    public int version = 1;
    public int activeSlot;
    public int language;
    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    public List<SaveSlotMetadata> slots = new List<SaveSlotMetadata>();
}

public class Save : MonoBehaviour
{
    public static Save instance;

    public const int SaveSlotCount = 3;
    private const string KEY_SAVE_DATA = "GameSaveData_JSON";
    private const string KEY_ACTIVE_SLOT = "GameSaveData_ActiveSlot";
    private const string KEY_SLOT_PREFIX = "GameSaveData_Slot_";
    private const string SaveFolderName = "Saves";
    private const string GlobalSaveFileName = "global_meta.json";
    private const string GlobalSaveBackupFileName = "global_meta.json.bak";
    private static SaveData cachedData;
    private static GlobalSaveData globalData;
    public static int activeSlot = -1;
    private float autosaveTimer;
    private const float AutosaveIntervalSeconds = 30f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureDataLoaded();
        EnsureChapterManager();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 모든 플레이 씬에서 챕터 진행 관리자가 항상 존재하도록 보장한다.
    /// 씬에 별도로 배치된 ChapterManager가 있으면 그 인스턴스를 그대로 사용한다.
    /// </summary>
    private static void EnsureChapterManager()
    {
        if (ChapterManager.instance != null)
            return;

        GameObject managerObject = new GameObject("ChapterManager");
        managerObject.AddComponent<ChapterManager>();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveGame();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveGame();
    }

    private void OnApplicationQuit() => SaveGame();

    private void Update()
    {
        if (!HasActiveRunSave() || SceneManager.GetActiveScene().name == "Mainmenu")
            return;

        autosaveTimer += Time.unscaledDeltaTime;
        if (autosaveTimer < AutosaveIntervalSeconds)
            return;

        autosaveTimer = 0f;
        SaveGame();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Mainmenu" && HasActiveRunSave())
            StartCoroutine(LoadAfterSceneInitialization());
    }

    private System.Collections.IEnumerator LoadAfterSceneInitialization()
    {
        yield return null;
        yield return null;
        LoadGame();
    }

    private static void EnsureDataLoaded()
    {
        if (cachedData != null) return;

        EnsureGlobalDataLoaded();
        activeSlot = Mathf.Clamp(globalData.activeSlot, 0, SaveSlotCount - 1);

        // 이전 단일 세이브를 첫 번째 슬롯으로 한 번만 옮긴다.
        if (!PlayerPrefs.HasKey(GetSlotKey(0)) && PlayerPrefs.HasKey(KEY_SAVE_DATA))
        {
            PlayerPrefs.SetString(GetSlotKey(0), PlayerPrefs.GetString(KEY_SAVE_DATA));
            PlayerPrefs.Save();

            // 단일 저장 데이터를 슬롯으로 이관한 직후 전역 슬롯 목록도 함께 갱신한다.
            RefreshAllSlotMetadata();
            CommitGlobalSave();
        }

        string json = ReadSlotJson(activeSlot);
        if (!string.IsNullOrEmpty(json))
        {
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
        cachedData.saveVersion = 2;
    }

    private static void EnsureGlobalDataLoaded()
    {
        if (globalData != null)
            return;

        globalData = ReadGlobalData() ?? new GlobalSaveData();
        EnsureGlobalSlotCapacity(globalData);

        // 최초 전환 때 기존 활성 슬롯의 설정을 전역 메타로 이관한다.
        if (globalData.version < 1)
            globalData.version = 1;

        if (!File.Exists(GetGlobalSavePath()) && !File.Exists(GetGlobalBackupPath()))
        {
            globalData.activeSlot = Mathf.Clamp(PlayerPrefs.GetInt(KEY_ACTIVE_SLOT, 0), 0, SaveSlotCount - 1);
            globalData.masterVolume = PlayerPrefs.GetFloat("Master", 1f);
            globalData.bgmVolume = PlayerPrefs.GetFloat("BGM", 1f);
            globalData.sfxVolume = PlayerPrefs.GetFloat("SFX", 1f);

            SaveData legacy = TryReadSlotData(globalData.activeSlot);
            if (legacy != null)
            {
                globalData.language = legacy.language;
                globalData.masterVolume = legacy.masterVolume;
                globalData.bgmVolume = legacy.bgmVolume;
                globalData.sfxVolume = legacy.sfxVolume;
            }
        }

        RefreshAllSlotMetadata();
        CommitGlobalSave();
    }

    public static GlobalSaveData GetGlobalSaveData()
    {
        EnsureGlobalDataLoaded();
        return globalData;
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
        cachedData.lastSavedUtcTicks = DateTime.UtcNow.Ticks;
        json = JsonUtility.ToJson(cachedData, true);
        WriteSlotJson(GetActiveSlotIndex(), json);
        UpdateSlotMetadata(GetActiveSlotIndex(), cachedData);
        CommitGlobalSave();
        // 이전 PlayerPrefs 기반 저장과의 하위 호환용 메타데이터만 유지한다.
        PlayerPrefs.Save();
    }

    public static int GetActiveSlotIndex()
    {
        EnsureDataLoaded();
        return activeSlot;
    }

    public static void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SaveSlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        activeSlot = slotIndex;
        cachedData = null;
        EnsureGlobalDataLoaded();
        globalData.activeSlot = activeSlot;
        CommitGlobalSave();
        PlayerPrefs.SetInt(KEY_ACTIVE_SLOT, activeSlot); // 구버전 호환용
        PlayerPrefs.Save();
        EnsureDataLoaded();
    }

    public static bool HasSaveSlot(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < SaveSlotCount && !string.IsNullOrEmpty(ReadSlotJson(slotIndex));
    }

    /// <summary>메인 메뉴의 슬롯 목록 표시에 쓰는 전역 메타데이터입니다.</summary>
    public static SaveSlotMetadata GetSaveSlotMetadata(int slotIndex)
    {
        EnsureGlobalDataLoaded();
        if (slotIndex < 0 || slotIndex >= SaveSlotCount)
            return null;

        EnsureGlobalSlotCapacity(globalData);
        return globalData.slots[slotIndex];
    }

    public static IReadOnlyList<SaveSlotMetadata> GetSaveSlotMetadataList()
    {
        EnsureGlobalDataLoaded();
        EnsureGlobalSlotCapacity(globalData);
        return globalData.slots;
    }

    public static SaveData GetSaveSlotData(int slotIndex)
    {
        if (!HasSaveSlot(slotIndex)) return null;
        try { return JsonUtility.FromJson<SaveData>(ReadSlotJson(slotIndex)); }
        catch (Exception) { return null; }
    }

    public static void DeleteSaveSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SaveSlotCount) return;
        PlayerPrefs.DeleteKey(GetSlotKey(slotIndex));
        DeleteSlotFiles(slotIndex);
        if (GetActiveSlotIndex() == slotIndex)
        {
            cachedData = new SaveData();
            EnsurePartySlotsCapacity(cachedData);
        }
        EnsureGlobalDataLoaded();
        globalData.slots[slotIndex] = new SaveSlotMetadata();
        CommitGlobalSave();
        PlayerPrefs.Save();
    }

    private static string GetSlotKey(int slotIndex) => KEY_SLOT_PREFIX + slotIndex;

    private static string GetSlotPath(int slotIndex) => Path.Combine(Application.persistentDataPath, SaveFolderName, $"slot_{slotIndex}.json");
    private static string GetBackupPath(int slotIndex) => GetSlotPath(slotIndex) + ".bak";
    private static string GetGlobalSavePath() => Path.Combine(Application.persistentDataPath, SaveFolderName, GlobalSaveFileName);
    private static string GetGlobalBackupPath() => Path.Combine(Application.persistentDataPath, SaveFolderName, GlobalSaveBackupFileName);

    private static GlobalSaveData ReadGlobalData()
    {
        try
        {
            string path = GetGlobalSavePath();
            string backup = GetGlobalBackupPath();
            if (File.Exists(path)) return JsonUtility.FromJson<GlobalSaveData>(File.ReadAllText(path));
            if (File.Exists(backup)) return JsonUtility.FromJson<GlobalSaveData>(File.ReadAllText(backup));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Save] 전역 메타 저장 읽기 실패: {e.Message}");
        }
        return null;
    }

    private static void CommitGlobalSave()
    {
        if (globalData == null) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GetGlobalSavePath()));
            string path = GetGlobalSavePath();
            string temp = path + ".tmp";
            string backup = GetGlobalBackupPath();
            File.WriteAllText(temp, JsonUtility.ToJson(globalData, true));
            if (File.Exists(path)) File.Copy(path, backup, true);
            File.Copy(temp, path, true);
            File.Delete(temp);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Save] 전역 메타 저장 실패: {e.Message}");
        }
    }

    private static void EnsureGlobalSlotCapacity(GlobalSaveData data)
    {
        if (data.slots == null) data.slots = new List<SaveSlotMetadata>();
        while (data.slots.Count < SaveSlotCount) data.slots.Add(new SaveSlotMetadata());
        if (data.slots.Count > SaveSlotCount) data.slots.RemoveRange(SaveSlotCount, data.slots.Count - SaveSlotCount);
    }

    private static SaveData TryReadSlotData(int slotIndex)
    {
        try
        {
            string json = ReadSlotJson(slotIndex);
            return string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<SaveData>(json);
        }
        catch { return null; }
    }

    private static void RefreshAllSlotMetadata()
    {
        EnsureGlobalSlotCapacity(globalData);
        for (int i = 0; i < SaveSlotCount; i++)
            UpdateSlotMetadata(i, TryReadSlotData(i));
    }

    private static void UpdateSlotMetadata(int slotIndex, SaveData data)
    {
        if (globalData == null || slotIndex < 0 || slotIndex >= SaveSlotCount) return;
        EnsureGlobalSlotCapacity(globalData);
        globalData.slots[slotIndex] = data == null
            ? new SaveSlotMetadata()
            : new SaveSlotMetadata
            {
                exists = true,
                lastSavedUtcTicks = data.lastSavedUtcTicks,
                chapter = data.currentChapter,
                sceneName = data.savedScene,
                hasActiveRun = data.hasActiveRun
            };
    }

    public static void UpdateGlobalSettings(int language, float master, float bgm, float sfx)
    {
        EnsureGlobalDataLoaded();
        globalData.language = language;
        globalData.masterVolume = Mathf.Clamp01(master);
        globalData.bgmVolume = Mathf.Clamp01(bgm);
        globalData.sfxVolume = Mathf.Clamp01(sfx);
        CommitGlobalSave();
    }

    private static string ReadSlotJson(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);
        string backupPath = GetBackupPath(slotIndex);
        try
        {
            if (File.Exists(path)) return File.ReadAllText(path);
            if (File.Exists(backupPath)) return File.ReadAllText(backupPath);
            // 최초 1회만 구 버전 PlayerPrefs 데이터를 마이그레이션한다.
            return PlayerPrefs.HasKey(GetSlotKey(slotIndex)) ? PlayerPrefs.GetString(GetSlotKey(slotIndex)) : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Save] 슬롯 {slotIndex + 1} 읽기 실패: {e.Message}");
            return null;
        }
    }

    private static void WriteSlotJson(int slotIndex, string json)
    {
        try
        {
            string path = GetSlotPath(slotIndex);
            string tempPath = path + ".tmp";
            string backupPath = GetBackupPath(slotIndex);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(tempPath, json);
            if (File.Exists(path)) File.Copy(path, backupPath, true);
            File.Copy(tempPath, path, true);
            File.Delete(tempPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Save] 슬롯 {slotIndex + 1} 저장 실패: {e.Message}");
        }
    }

    private static void DeleteSlotFiles(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);
        string backupPath = GetBackupPath(slotIndex);
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(backupPath)) File.Delete(backupPath);
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

        data.partyUnitStates.Clear();
        foreach (Unit unit in partySlots)
        {
            if (unit != null)
                data.partyUnitStates.Add(CreateUnitEntry(unit));
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

    public static string GetSavedScene(string defaultScene = "Forest")
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
        // 새 런의 시작 맵은 Forest다. 메뉴의 새 게임 진입 씬과 저장 씬을 일치시킨다.
        data.savedScene = "Forest";

        if (ChapterManager.instance != null)
        {
            ChapterManager.instance.StartChapter(data.currentChapter);
            data.currentZone = (int)ChapterManager.instance.GetCurrentZone();
        }
        else
        {
            data.currentZone = (int)ZoneType.Forest;
        }

        data.mapZone = data.currentZone;
        data.mapSeed = CreateMapSeed();

        CommitSave();
        Debug.Log($"<color=green>[Save]</color> 제 {data.currentChapter}챕터 신규 런 시작 세이브 저장 완료");
    }

    /// <summary>현재 지역 맵을 재생성해도 같은 구조가 나오도록 지역별 시드를 관리합니다.</summary>
    public static int GetOrCreateMapSeed(ZoneType zone)
    {
        SaveData data = GetSaveData();
        int zoneValue = (int)zone;
        if (data.mapZone != zoneValue || data.mapSeed == 0)
        {
            data.mapZone = zoneValue;
            data.mapSeed = CreateMapSeed();
            CommitSave();
        }
        return data.mapSeed;
    }

    private static int CreateMapSeed()
    {
        int seed = UnityEngine.Random.Range(int.MinValue + 1, int.MaxValue);
        return seed == 0 ? 1 : seed;
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
        // 메인 메뉴에서 백그라운드로 전환돼도 진행 중인 게임의 씬 정보를 덮어쓰지 않는다.
        if (SceneManager.GetActiveScene().name == "Mainmenu")
            return;

        SaveData data = GetSaveData();

        GlobalSaveData settings = GetGlobalSaveData();
        settings.language = GetCurrentLanguageIndex();
        CommitGlobalSave();

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
            data.clearedNodeIds.Clear();
            foreach (RoomNode node in RoomManager.instance.allNodes)
            {
                if (node != null && node.cleared)
                    data.clearedNodeIds.Add(node.id);
            }
        }

        if (PartyManager.instance != null)
        {
            EnsurePartySlotsCapacity(data, PartyManager.instance.partySlots.Length);
            data.partyUnitStates.Clear();
            for (int i = 0; i < PartyManager.instance.partySlots.Length; i++)
            {
                Unit unit = PartyManager.instance.partySlots[i];
                data.partySlotUnitNames[i] = unit != null ? unit.name.Replace("(Clone)", "").Trim() : "";
                if (unit != null)
                    data.partyUnitStates.Add(CreateUnitEntry(unit));
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

        // 전투 전용 씬은 전투 턴/적 상태를 별도로 직렬화하지 않으므로,
        // 모바일 재시작 시에는 마지막으로 안전하게 저장된 탐험 씬으로 복귀한다.
        if (BattleManager.instance == null || !BattleManager.instance.isBattle)
            data.savedScene = SceneManager.GetActiveScene().name;

        CommitSave();
        Debug.Log("게임 전체 데이터 JSON 저장 완료");
    }

    public void LoadGame()
    {
        SaveData data = GetSaveData();

        ChangeLanguage(GetGlobalSaveData().language);

        if (ChapterManager.instance != null)
        {
            ChapterManager.instance.currentChapter = data.currentChapter;
            ChapterManager.instance.currentZoneIndex = data.currentZoneIndex;
        }

        if (RoomManager.instance != null)
        {
            RoomManager.instance.currentZone = (ZoneType)data.currentZone;
            if (data.clearedNodeIds != null)
            {
                foreach (RoomNode node in RoomManager.instance.allNodes)
                    node.cleared = data.clearedNodeIds.Contains(node.id);
            }
            if (data.currentNodeId >= 0)
            {
                RoomManager.instance.LoadNode(data.currentNodeId);
            }
        }

        if (PartyManager.instance != null)
        {
            PartyManager.instance.LoadParty();
            RestorePartyUnitStates(data);
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
        PlayerPrefs.DeleteKey(KEY_ACTIVE_SLOT);
        for (int i = 0; i < SaveSlotCount; i++)
        {
            PlayerPrefs.DeleteKey(GetSlotKey(i));
            DeleteSlotFiles(i);
        }
        try
        {
            if (File.Exists(GetGlobalSavePath())) File.Delete(GetGlobalSavePath());
            if (File.Exists(GetGlobalBackupPath())) File.Delete(GetGlobalBackupPath());
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Save] 전역 메타 삭제 실패: {e.Message}");
        }
        globalData = null;
        PlayerPrefs.Save();
        Debug.Log("모든 저장 데이터 초기화 완료");
    }

    private static SavedUnitEntry CreateUnitEntry(Unit unit)
    {
        SavedUnitEntry entry = new SavedUnitEntry
        {
            unitName = unit.name.Replace("(Clone)", "").Trim(),
            health = unit.health,
            energyCurrent = unit.energyCurrent,
            communication = unit.communication,
            isPowerDown = unit.isPowerDown,
            isEmergencyMode = unit.isEmergencyMode,
            hasSelfDestructed = unit.hasSelfDestructed,
            attackLevel = unit.attackLevel,
            MaxHpLevel = unit.MaxHPLevel,
            defenseLevel = unit.defenseLevel,
            accuracyUpgrade = unit.AccurancyLevel,
            circuitShortStacks = unit.circuitShortStacks,
            ghostingMissPenalty = unit.ghostingMissPenalty
        };

        foreach (KeyValuePair<StatusType, int> status in unit.StatusHandler.GetActiveStatuses())
            entry.statuses.Add(new SavedStatusEntry { type = status.Key, remainingTurns = status.Value });
        return entry;
    }

    private static void RestorePartyUnitStates(SaveData data)
    {
        if (data.partyUnitStates == null || PartyManager.instance == null)
            return;

        foreach (Unit unit in PartyManager.instance.partySlots)
        {
            if (unit == null) continue;
            string unitName = unit.name.Replace("(Clone)", "").Trim();
            SavedUnitEntry entry = data.partyUnitStates.Find(x => x.unitName == unitName);
            if (entry == null) continue;

            unit.health = Mathf.Clamp(entry.health, 0f, unit.maxHealth);
            unit.energyCurrent = Mathf.Clamp(entry.energyCurrent, 0, unit.energyMax);
            unit.communication = Mathf.Clamp(entry.communication, -100, 100);
            unit.isPowerDown = entry.isPowerDown;
            unit.isEmergencyMode = entry.isEmergencyMode;
            unit.hasSelfDestructed = entry.hasSelfDestructed;
            unit.attackLevel = entry.attackLevel;
            unit.MaxHPLevel = entry.MaxHpLevel;
            unit.defenseLevel = entry.defenseLevel;
            unit.AccurancyLevel = entry.accuracyUpgrade;
            unit.circuitShortStacks = entry.circuitShortStacks;
            unit.ghostingMissPenalty = entry.ghostingMissPenalty;
            unit.StatusHandler.RestoreStatuses(entry.statuses);
            unit.UpdateHealthBar();
        }
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
