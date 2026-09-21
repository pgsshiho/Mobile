using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 선택지 버튼의 표시와 클릭 전달만 담당한다.
/// 대화 흐름과 선택 결과는 DialogueManager가 담당한다.
/// </summary>
public class DialogueChoiceController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("선택지 버튼들을 감싸는 패널입니다. 비워 두면 이 오브젝트를 사용합니다.")]
    public GameObject choicePanel;

    [Tooltip("표시 순서대로 연결할 선택지 버튼들입니다.")]
    public Button[] choiceButtons;

    private TMP_Text[] buttonLabels;
    private bool isShowing;

    public bool IsShowing => isShowing;
    public bool CanShowChoices => choiceButtons != null && choiceButtons.Length > 0;

    private void Awake()
    {
        if (choicePanel == null)
            choicePanel = gameObject;

        CacheButtons();
        HideChoices();
    }

    private void CacheButtons()
    {
        if (choiceButtons == null)
        {
            buttonLabels = Array.Empty<TMP_Text>();
            return;
        }

        buttonLabels = new TMP_Text[choiceButtons.Length];
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
                buttonLabels[i] = choiceButtons[i].GetComponentInChildren<TMP_Text>(true);
        }
    }

    public void ShowChoices(DialogueChoice[] choices, Action<DialogueChoice> onChoiceSelected)
    {
        if (choices == null || choices.Length == 0 || !CanShowChoices)
            return;

        if (buttonLabels == null || buttonLabels.Length != choiceButtons.Length)
            CacheButtons();

        choicePanel.SetActive(true);
        isShowing = true;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            Button button = choiceButtons[i];
            if (button == null)
                continue;

            bool hasChoice = i < choices.Length && choices[i] != null;
            button.gameObject.SetActive(hasChoice);
            if (!hasChoice)
                continue;

            DialogueChoice choice = choices[i];
            if (buttonLabels[i] != null)
                buttonLabels[i].text = choice.text;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onChoiceSelected?.Invoke(choice));
        }
    }

    public void HideChoices()
    {
        isShowing = false;
        if (choicePanel != null)
            choicePanel.SetActive(false);
    }
}
