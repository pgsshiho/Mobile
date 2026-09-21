using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Choices")]
    [Tooltip("선택지 전용 컨트롤러입니다. 별도 UI 오브젝트에 붙여 연결하세요.")]
    public DialogueChoiceController choiceController;

    [Header("Localization Settings")]
    [Tooltip("Unity Localization String Table의 이름을 입력하세요.")]
    public string tableName = "DialogueTable"; // Inspector에서 설정 가능

    [Header("Typing")]
    public float typingSpeed = 0.03f;

    // 캐싱된 번역 완료 텍스트들
    private string[] cachedDialogueTexts;

    // 현재 페이지 및 코루틴
    public int currentPage;
    private Coroutine typingCoroutine;

    // 상태 플래그
    private bool isTyping;
    private bool isLoading;
    private DialogueChoice[] pendingChoices;

    private void Awake()
    {
        instance = this;
        EnsureChoiceController();
    }

    private void Start()
    {
        EnsureChoiceController();
    }

    /// <summary>
    /// choiceController가 인스펙터에 미연결 상태일 경우 씬에서 자동 탐색하여 연결합니다.
    /// </summary>
    public void EnsureChoiceController()
    {
        if (choiceController == null)
        {
            choiceController = FindObjectOfType<DialogueChoiceController>(true);
        }

        if (choiceController == null)
        {
            GameObject choiceObj = GameObject.Find("ChoisPnale") ?? GameObject.Find("ChoicePanel") ?? GameObject.Find("Choice Panel");
            if (choiceObj == null)
            {
                var allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
                foreach (var t in allTransforms)
                {
                    if (t.gameObject.scene.IsValid() && (t.name == "ChoisPnale" || t.name == "ChoicePanel" || t.name == "Choice Panel"))
                    {
                        choiceObj = t.gameObject;
                        break;
                    }
                }
            }

            if (choiceObj != null)
            {
                choiceController = choiceObj.GetComponent<DialogueChoiceController>() ?? choiceObj.AddComponent<DialogueChoiceController>();
            }
        }
    }

    private void Update()
    {
        // 로딩 중이거나 타이핑 중일 때 클릭 이벤트 처리
        if (dialoguePanel != null && dialoguePanel.activeSelf &&
            (choiceController == null || !choiceController.IsShowing) &&
            Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 타이핑 중 클릭 시 전체 문장 즉시 출력 (스킵)
                CompleteCurrentLine();
            }
            else if (!isLoading)
            {
                // 타이핑이 완전히 끝난 후 클릭 시 다음 문장으로
                NextDialogue();
            }
        }
    }

    // 대화 시작: 모든 키를 한번에 번역
    public void StartDialogue(string[] keys)
    {
        pendingChoices = null;
        StartDialogueInternal(keys);
    }

    /// <summary>
    /// 마지막 대사 뒤 선택지를 보여 주는 대화 시작 메서드입니다.
    /// </summary>
    public void StartDialogueWithChoices(
        string[] keys,
        DialogueChoice[] choices)
    {
        pendingChoices = choices;
        StartDialogueInternal(keys);
    }

    private void StartDialogueInternal(string[] keys)
    {
        if (keys == null || keys.Length == 0) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(PreloadAndStartDialogue(keys));
    }

    // 일괄 번역 처리 후 출력 시작
    private IEnumerator PreloadAndStartDialogue(string[] keys)
    {
        isLoading = true;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = "";

        cachedDialogueTexts = new string[keys.Length];

        // 1. 모든 키를 한번에 비동기로 로드
        for (int i = 0; i < keys.Length; i++)
        {
            LocalizedString localizedString = new LocalizedString(tableName, keys[i]);
            var handle = localizedString.GetLocalizedStringAsync();

            yield return handle;

            cachedDialogueTexts[i] = handle.Result;
        }

        isLoading = false;
        currentPage = 0;

        // 2. 첫 문장 출력
        ShowDialogue(currentPage);
    }

    // 지정된 페이지의 대화 출력
    public void ShowDialogue(int pageIndex)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        if (cachedDialogueTexts != null && pageIndex >= 0 && pageIndex < cachedDialogueTexts.Length)
        {
            typingCoroutine = StartCoroutine(TypeDialogueText(cachedDialogueTexts[pageIndex]));
        }
    }

    private WaitForSeconds cachedTypingWait;
    private float cachedTypingSpeed = -1f;

    private WaitForSeconds GetTypingWait()
    {
        if (cachedTypingWait == null || !Mathf.Approximately(cachedTypingSpeed, typingSpeed))
        {
            cachedTypingSpeed = typingSpeed;
            cachedTypingWait = new WaitForSeconds(typingSpeed);
        }
        return cachedTypingWait;
    }

    IEnumerator TypeDialogueText(string text)
    {
        isTyping = true;
        if (dialogueText != null)
        {
            dialogueText.text = text;
            dialogueText.maxVisibleCharacters = 0;

            WaitForSeconds wait = GetTypingWait();
            int len = text.Length;
            for (int i = 1; i <= len; i++)
            {
                dialogueText.maxVisibleCharacters = i;
                yield return wait;
            }
        }

        isTyping = false;
    }

    // 타이핑 스킵 (즉시 전체 출력)
    private void CompleteCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        if (dialogueText != null && cachedDialogueTexts != null && currentPage < cachedDialogueTexts.Length)
        {
            dialogueText.text = cachedDialogueTexts[currentPage];
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }
        isTyping = false;
    }

    // 다음 대화
    public void NextDialogue()
    {
        currentPage++;

        // 대화 종료 지점 도달
        if (cachedDialogueTexts == null || currentPage >= cachedDialogueTexts.Length)
        {
            EnsureChoiceController();

            if (pendingChoices != null && pendingChoices.Length > 0 && choiceController != null)
            {
                choiceController.ShowChoices(pendingChoices, SelectChoice);
                return;
            }

            CloseDialogue();

            if (RoomNavigationUI.instance != null)
            {
                RoomNavigationUI.instance.SetNavigationActive(true);
            }
            return;
        }

        ShowDialogue(currentPage);
    }

    private void SelectChoice(DialogueChoice choice)
    {
        choice?.onSelected?.Invoke();
        CloseDialogue();

        if (RoomNavigationUI.instance != null)
            RoomNavigationUI.instance.SetNavigationActive(true);
    }

    public void CloseDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        EnsureChoiceController();
        if (choiceController != null)
            choiceController.HideChoices();

        cachedDialogueTexts = null;
        pendingChoices = null;
        currentPage = 0;
        isTyping = false;
        isLoading = false;

        // 대화 종료 시 카메라 포커스 아웃(FocusOut) 트리거
        FocusManager.RequestFocusOut?.Invoke();
        FocusManagerQuest.RequestFocusOut?.Invoke();
    }
}
