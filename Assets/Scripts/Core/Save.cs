using UnityEngine;
using UnityEngine.Localization.Settings;

public class Save : MonoBehaviour
{
    public static Save instance;

    private void Awake()
    {
        instance = this;
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt(
            "Language",
            GetCurrentLanguageIndex()
        );

        if (RoomManager.instance != null)
        {
            PlayerPrefs.SetInt(
                "CurrentNodeId",
                RoomManager.instance.GetCurrentNodeId()
            );
        }

        PlayerPrefs.Save();

        Debug.Log("게임 저장 완료");
    }

    public void LoadGame()
    {
        int language =
            PlayerPrefs.GetInt("Language", 0);

        ChangeLanguage(language);

        int nodeId =
            PlayerPrefs.GetInt("CurrentNodeId", -1);

        if (RoomManager.instance != null &&
            nodeId >= 0)
        {
            RoomManager.instance.LoadNode(nodeId);
        }

        Debug.Log("게임 불러오기 완료");
    }

    int GetCurrentLanguageIndex()
    {
        return LocalizationSettings.SelectedLocale ==
            LocalizationSettings.AvailableLocales.Locales[0]
            ? 0
            : 1;
    }

    void ChangeLanguage(int index)
    {
        if (index < 0 ||
            index >= LocalizationSettings
                .AvailableLocales.Locales.Count)
            return;

        LocalizationSettings.SelectedLocale =
            LocalizationSettings
            .AvailableLocales.Locales[index];
    }
}