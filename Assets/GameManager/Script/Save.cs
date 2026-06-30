using UnityEngine;
using UnityEngine.Localization.Settings;

public class Save : MonoBehaviour
{
    public static Save instance;

    private void Awake()
    {
        instance = this;
    }

    // 저장
    public void SaveGame()
    {
        // 현재 방 저장
        PlayerPrefs.SetString(
            "CurrentRoom",
            RoomManager.instance.currentRoom.roomName
        );

        // 언어 저장
        PlayerPrefs.SetInt(
            "Language",
            (int)LocalizationManager()
        );
        // 저장 실행
        PlayerPrefs.Save();

        Debug.Log("게임 저장 완료");
    }

    // 불러오기
    public void LoadGame()
    {
        // 저장된 방 이름
        string roomName =
            PlayerPrefs.GetString("CurrentRoom", "");

        // 저장된 언어
        int language =
            PlayerPrefs.GetInt("Language", 0);

        // 언어 적용
        ChangeLanguage(language);

        // 방 찾기
        Room[] rooms = FindObjectsOfType<Room>(true);

        foreach (Room room in rooms)
        {
            if (room.roomName == roomName)
            {
                RoomManager.instance.ChangeRoom(room);
                break;
            }
        }

        Debug.Log("게임 불러오기 완료");
    }

    // 언어 가져오기
    int LocalizationManager()
    {
        return LocalizationSettings.SelectedLocale ==
            LocalizationSettings.AvailableLocales.Locales[0]
            ? 0 : 1;
    }

    // 언어 변경
    void ChangeLanguage(int index)
    {
        LocalizationSettings.SelectedLocale =
            LocalizationSettings.AvailableLocales.Locales[index];
    }
}