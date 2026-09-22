using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController instance;

    [Header("Main Buttons")]
    public Button continueButton;
    public Button newGameButton;
    public TextMeshProUGUI continueButtonText;

    [Header("Chapter Select Panel")]
    public GameObject chapterSelectPanel;
    public Button backButton;
    public ChapterButtonEntry[] chapterButtons; // 1챕터 ~ 5챕터 버튼 항목 (인스펙터 또는 코드 자동 바인딩)

    [Serializable]
    public class ChapterButtonEntry
    {
        public int chapterNumber;
        public Button button;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI statusText; // "진행 가능", "클리어", "잠김" 등
        public TextMeshProUGUI zonesText;  // 예: "Forest, Coast"
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        AutoFindUI();
        BindButtons();
        RefreshMenuUI();
    }

    private void AutoFindUI()
    {
        if (continueButton == null)
        {
            var go = GameObject.Find("ContinueButton");
            if (go != null) continueButton = go.GetComponent<Button>();
        }
        if (continueButtonText == null && continueButton != null)
        {
            continueButtonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (newGameButton == null)
        {
            var go = GameObject.Find("NewGameButton");
            if (go != null) newGameButton = go.GetComponent<Button>();
        }

        if (chapterSelectPanel == null)
        {
            chapterSelectPanel = GameObject.Find("ChapterSelectPanel");
            if (chapterSelectPanel == null)
            {
                var all = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in all)
                {
                    if (go.hideFlags == HideFlags.None && go.name == "ChapterSelectPanel")
                    {
                        chapterSelectPanel = go;
                        break;
                    }
                }
            }
        }

        if (backButton == null && chapterSelectPanel != null)
        {
            var b = chapterSelectPanel.transform.Find("BackButton");
            if (b != null) backButton = b.GetComponent<Button>();
        }
    }

    private void BindButtons()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnClickContinue);
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(OnClickNewGame);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(CloseChapterSelect);
        }
    }

    /// <summary>
    /// 메인 메뉴의 이어하기 활성화 여부 및 챕터 상태를 최신 세이브 기준으로 갱신합니다.
    /// </summary>
    public void RefreshMenuUI()
    {
        SaveData data = Save.GetSaveData();
        bool hasActiveRun = Save.HasActiveRunSave();

        // 1. 이어하기 버튼 상태
        if (continueButton != null)
        {
            continueButton.interactable = hasActiveRun;
            if (continueButtonText != null)
            {
                if (hasActiveRun)
                {
                    continueButtonText.text = $"이어하기 (제 {data.currentChapter}챕터 - {((ZoneType)data.currentZone)})";
                    continueButtonText.color = Color.white;
                }
                else
                {
                    continueButtonText.text = "이어하기 (저장 없음)";
                    continueButtonText.color = new Color(1f, 1f, 1f, 0.4f);
                }
            }
        }

        // 2. 챕터 선택 버튼들 상태 갱신
        if (chapterButtons != null)
        {
            int unlocked = Mathf.Max(1, data.unlockedChapter);
            int cleared = data.highestClearedChapter;

            foreach (var entry in chapterButtons)
            {
                if (entry == null || entry.button == null) continue;

                int ch = entry.chapterNumber;
                bool isUnlocked = ch <= unlocked;
                bool isCleared = ch <= cleared;

                entry.button.interactable = isUnlocked;

                // 시각적 스타일 (배경색 및 텍스트 밝기)
                var img = entry.button.GetComponent<Image>();
                if (img != null)
                {
                    img.color = isUnlocked 
                        ? new Color(0.18f, 0.22f, 0.28f, 0.95f) 
                        : new Color(0.08f, 0.08f, 0.1f, 0.6f);
                }

                // 타이틀 텍스트
                if (entry.titleText != null)
                {
                    entry.titleText.text = $"제 {ch}챕터";
                    entry.titleText.color = isUnlocked ? Color.white : new Color(0.45f, 0.45f, 0.45f, 0.6f);
                }

                // 지역 목록 텍스트
                if (entry.zonesText != null)
                {
                    if (ChapterManager.ChapterZones.TryGetValue(ch, out ZoneType[] zones))
                    {
                        entry.zonesText.text = string.Join(" → ", zones);
                    }
                    entry.zonesText.color = isUnlocked ? new Color(0.75f, 0.75f, 0.75f, 1f) : new Color(0.35f, 0.35f, 0.35f, 0.5f);
                }

                // 상태 텍스트
                if (entry.statusText != null)
                {
                    if (isCleared)
                    {
                        entry.statusText.text = "<color=#FFD700>[클리어 완료]</color>";
                    }
                    else if (isUnlocked)
                    {
                        entry.statusText.text = "<color=#44FF88>[입장 가능]</color>";
                    }
                    else
                    {
                        entry.statusText.text = $"<color=#FF5555>[잠김] ({ch - 1}챕터 클리어 필요)</color>";
                    }
                }

                // 클릭 이벤트 바인딩
                int selectedChapter = ch;
                entry.button.onClick.RemoveAllListeners();
                if (isUnlocked)
                {
                    entry.button.onClick.AddListener(() => OnSelectChapter(selectedChapter));
                }
            }
        }
    }

    /// <summary>
    /// [이어하기] 클릭 시: 저장된 런 데이터를 불러와 바로 진입합니다.
    /// </summary>
    public void OnClickContinue()
    {
        if (!Save.HasActiveRunSave())
        {
            Debug.LogWarning("[MainMenu] 진행 중인 런 세이브가 없습니다.");
            return;
        }

        if (Save.instance != null)
        {
            Save.instance.LoadGame();
        }

        string sceneToLoad = Save.GetSavedScene("Factory");
        Debug.Log($"<color=cyan>[MainMenu]</color> 이어하기 시작! (씬: {sceneToLoad})");
        SceneChanger.BG(sceneToLoad);
    }

    /// <summary>
    /// [새로 시작] 클릭 시: 챕터 선택 패널을 엽니다.
    /// </summary>
    public void OnClickNewGame()
    {
        RefreshMenuUI();
        if (chapterSelectPanel != null)
        {
            chapterSelectPanel.SetActive(true);
        }
        else
        {
            // 만약 챕터 패널 오브젝트가 아직 할당되지 않았다면 즉시 1챕터로 시작
            OnSelectChapter(1);
        }
    }

    /// <summary>
    /// 챕터 선택 창 닫기
    /// </summary>
    public void CloseChapterSelect()
    {
        if (chapterSelectPanel != null)
        {
            chapterSelectPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 특정 챕터를 선택하여 새로운 런을 시작합니다.
    /// </summary>
    public void OnSelectChapter(int chapter)
    {
        Debug.Log($"<color=green>[MainMenu]</color> 제 {chapter}챕터 신규 시작!");

        // 신규 런 세이브 데이터 생성 및 저장
        Save.StartNewRun(chapter);

        // 씬 이동
        string sceneToLoad = "Factory";
        SceneChanger.BG(sceneToLoad);
    }
}
