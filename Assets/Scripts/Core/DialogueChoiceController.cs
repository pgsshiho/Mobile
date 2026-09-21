using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
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
    public bool CanShowChoices => (choiceButtons != null && choiceButtons.Length > 0) || GetComponentsInChildren<Button>(true).Length > 0;

    private void Awake()
    {
        if (choicePanel == null)
            choicePanel = gameObject;

        CacheButtons();
        HideChoices();
    }

    private void CacheButtons()
    {
        if (choiceButtons == null || choiceButtons.Length == 0)
        {
            choiceButtons = GetComponentsInChildren<Button>(true);
        }

        if (choiceButtons == null || choiceButtons.Length == 0)
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
        if (choices == null || choices.Length == 0)
            return;

        if (choiceButtons == null || choiceButtons.Length == 0 || buttonLabels == null || buttonLabels.Length != choiceButtons.Length)
        {
            CacheButtons();
        }

        if (choiceButtons == null || choiceButtons.Length == 0)
        {
            Debug.LogWarning("[DialogueChoiceController] 표시할 선택지 버튼(Button)이 없습니다.");
            return;
        }

        if (choicePanel != null)
            choicePanel.SetActive(true);

        gameObject.SetActive(true);
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
            if (buttonLabels != null && i < buttonLabels.Length && buttonLabels[i] != null)
            {
                // LocalizeStringEvent가 있는 경우 텍스트를 강제 덮어쓰지 않도록 비활성화
                var loc = buttonLabels[i].GetComponent<LocalizeStringEvent>();
                if (loc != null)
                {
                    loc.enabled = false;
                }

                buttonLabels[i].text = choice.text;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onChoiceSelected?.Invoke(choice));
        }

        Debug.Log($"[DialogueChoiceController] 선택지 {choices.Length}개 표시 완료");
    }

    public void HideChoices()
    {
        isShowing = false;
        if (choicePanel != null)
            choicePanel.SetActive(false);
    }
}
