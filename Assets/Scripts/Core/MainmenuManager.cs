
using TMPro;
using UnityEditor.U2D.Tooling.Analyzer;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class MainmenuManager : MonoBehaviour
{
    public TextMeshProUGUI Savefile1Chapter;
    public TextMeshProUGUI Savefile2Chapter;
    public TextMeshProUGUI Savefile3Chapter;

    void Start()
    {
        RefreshSavefileChapters();
    }

    public void RefreshSavefileChapters()
    {
        UpdateChapterText(Savefile1Chapter, 0);
        UpdateChapterText(Savefile2Chapter, 1);
        UpdateChapterText(Savefile3Chapter, 2);
    }

    private void UpdateChapterText(TextMeshProUGUI chapterText, int slotIndex)
    {
        if (chapterText == null)
            return;

        // 해당 슬롯에 저장 데이터가 있는지 확인
        SaveData data = Save.GetSaveSlotData(slotIndex);

        if (data == null)
        {
            chapterText.text = "New Game";
            return;
        }

        // 저장된 현재 챕터 표시
        chapterText.text = "Chapter: " + data.currentChapter;
    }


    public void LoadGame(int savefile)
    {
        if (savefile < 0 || savefile >= Save.SaveSlotCount)
        {
            Debug.LogWarning("잘못된 저장 슬롯 번호: " + savefile);
            return;
        }

        SaveData data = Save.GetSaveSlotData(savefile);

        if (data == null)
        {
            Debug.LogWarning("저장 데이터가 없습니다. 슬롯: " + savefile);
            SceneChanger.BG("Forest");
        }

        if (string.IsNullOrEmpty(data.savedScene))
        {
            Debug.LogWarning("저장된 씬 이름이 없습니다.");
            return;
        }

        // 불러올 저장 슬롯 선택
        Save.SelectSlot(savefile);

        // 저장된 씬으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene(data.savedScene);
    }
    public void DeleteSaveFile(int SaveFile)
    {
        SaveData data = Save.GetSaveSlotData(SaveFile);
        if (data == null)
        {
            Debug.LogWarning("저장 데이터가 없습니다. 슬롯: " + SaveFile);
            return;
        }
        Save.DeleteSaveSlot(SaveFile);
        RefreshSavefileChapters();
    }
    public void CloseDelSaveFile(GameObject these)
    {
        these.SetActive(false);
    }
}
