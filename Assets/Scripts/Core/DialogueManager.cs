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

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // 로딩 중이거나 타이핑 중일 때 클릭 이벤트 처리
        if (dialoguePanel.activeSelf && Input.GetMouseButtonDown(0))
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
        dialoguePanel.SetActive(true);
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

        typingCoroutine = StartCoroutine(TypeDialogueText(cachedDialogueTexts[pageIndex]));
    }

    IEnumerator TypeDialogueText(string text)
    {
        isTyping = true;
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;

        // 문자열을 글자마다 다시 연결하지 않고 TMP의 표시 글자 수만 늘린다.
        // 화면 결과는 기존 타이핑 연출과 동일하지만 GC 할당을 만들지 않는다.
        for (int i = 1; i <= text.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
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

        dialogueText.text = cachedDialogueTexts[currentPage];
        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
    }

    // 다음 대화
    public void NextDialogue()
    {
        currentPage++;

        // 대화 종료
        if (currentPage >= cachedDialogueTexts.Length)
        {
            CloseDialogue();

            if (RoomNavigationUI.instance != null)
            {
                RoomNavigationUI.instance.SetNavigationActive(true);
            }
            return;
        }

        ShowDialogue(currentPage);
    }

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        cachedDialogueTexts = null;
        currentPage = 0;
        isTyping = false;
        isLoading = false;
    }
}
