using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Typing")]
    public float typingSpeed = 0.03f;

    Coroutine typingCoroutine;

    // 현재 대화들
    string[] currentDialogueKeys;

    // 현재 페이지
    int currentPage;

    // 타이핑 끝났는지
    bool isTyping;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (dialoguePanel.activeSelf &&
           Input.GetKeyDown(KeyCode.Space))
        {
            NextDialogue();
        }
    }

    // 대화 시작
    public void StartDialogue(string[] keys)
    {
        currentDialogueKeys = keys;

        currentPage = 0;

        ShowDialogue(currentDialogueKeys[currentPage]);
    }

    // 다음 대화
    public void NextDialogue()
    {
        if (isTyping)
            return;

        currentPage++;

        // 끝났으면 종료
        if (currentPage >= currentDialogueKeys.Length)
        {
            CloseDialogue();
            return;
        }

        ShowDialogue(currentDialogueKeys[currentPage]);
    }

    // 출력
    public void ShowDialogue(string key)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(TypeLocalizedText(key));
    }

    IEnumerator TypeLocalizedText(string key)
    {
        isTyping = true;

        dialoguePanel.SetActive(true);

        dialogueText.text = "";

        LocalizedString localizedString =
            new LocalizedString("MainText", key);

        string text =
            localizedString.GetLocalizedString();

        foreach (char c in text)
        {
            dialogueText.text += c;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}