using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class MainmenuManager : MonoBehaviour
{
    public TextMeshProUGUI Savefile1Chapter;
    public TextMeshProUGUI Savefile2Chapter;
    public TextMeshProUGUI Savefile3Chapter;
    public AudioClip StartSFX;

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

        // 해당 슬롯에 저장 데이터가 있고 진행 중인 활성 런이 있는지 확인
        SaveData data = Save.GetSaveSlotData(slotIndex);

        if (data == null || !data.hasActiveRun)
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

        if (data == null || !data.hasActiveRun)
        {
            // 빈 슬롯이거나 활성 런이 없으면 1챕터부터 새 런 시작
            Save.SelectSlot(savefile);
            Save.StartNewRun(1);

            AudioManager.instance?.StopMainBgm();
            AudioManager.instance?.PlaySfx(StartSFX);

            string startScene = Save.GetSaveData().savedScene;
            SceneChanger.BG(string.IsNullOrEmpty(startScene) ? "Forest" : startScene);
            return;
        }

        if (string.IsNullOrEmpty(data.savedScene))
        {
            Debug.LogWarning("저장된 씬 이름이 없습니다.");
            return;
        }

        // 불러올 저장 슬롯 선택
        Save.SelectSlot(savefile);
        AudioManager.instance?.StopMainBgm();
        AudioManager.instance?.PlaySfx(StartSFX);

        // 저장된 씬(해당 챕터에 맞는 Scene)으로 이동
        SceneChanger.BG(data.savedScene);
    }

    public void DeleteSaveFile(int SaveFile)
    {
        Save.DeleteSaveSlot(SaveFile);
        RefreshSavefileChapters();
    }

    public void CloseDelSaveFile(GameObject these)
    {
        these.SetActive(false);
    }
}
