
using System.Collections.Generic;
using UnityEngine;

public class RuinedBuilding : MonoBehaviour
{
    [Header("대화 설정")]
    public string[] FindDailogueKey;
    public string FindChoiceText = "FindChoice";
    public string exitChoiceText = "ExitChoice";
    [Header("탐색 보상 아이템")]
    public ItemData[] items;
    public GameObject HideUI;
    [Header("탐색 설정")]
    [Min(0)]
    public int damageAmount = 10;

    [Min(1)]
    public int rewardItemAmount = 2;

    [Range(0, 100)]
    public int damageChance = 20;

    public bool isFind = false;
    /// <summary>
    /// 폐허를 클릭했을 때 대화 및 선택지를 표시한다.
    /// </summary>
    public void ClickruninBuilding()
    {
        if (isFind)
        {
            AlreadyFind();
            return;
        }
        if (DialogueManager.instance == null)
        {
            Debug.LogWarning("[RuinedBuilding] DialogueManager를 찾을 수 없습니다.");
            return;
        }

        string[] keysToUse =
            (FindDailogueKey != null && FindDailogueKey.Length > 0)
            ? FindDailogueKey
            : new string[] { "RuinedBuilding" };

        DialogueManager.instance.StartDialogueWithChoices(
            keysToUse,
            new DialogueChoice[]
            {
                new DialogueChoice
                {
                    text = LocalizationManager.GetLocalizedString(FindChoiceText),
                    onSelected = CreateFindEvent()
                },
                new DialogueChoice
                {
                    text = LocalizationManager.GetLocalizedString(exitChoiceText)
                }
            }
        );
    }
    public void AlreadyFind()
    {
        if (DialogueManager.instance == null)
        {
            Debug.LogWarning("[RuinedBuilding] DialogueManager를 찾을 수 없습니다.");
            return;
        }
        DialogueManager.instance.StartDialogue(new string[] { "AlreadyFindBuilding" });
    }
    /// <summary>
    /// 탐색 선택지에 실행 이벤트를 등록한다.
    /// </summary>
    private UnityEngine.Events.UnityEvent CreateFindEvent()
    {
        var evt = new UnityEngine.Events.UnityEvent();
        evt.AddListener(BeginRepairSelection);
        return evt;
    }

    /// <summary>
    /// 폐허 탐색을 실행하고 결과를 결정한다.
    /// </summary>
    public void BeginRepairSelection()
    {
        if (PartyManager.instance == null)
        {
            Debug.LogWarning("[RuinedBuilding] PartyManager를 찾을 수 없습니다.");
            return;
        }

        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.CloseDialogue();
        }

        // 피해 발생 여부를 확률로 결정한다.
        int randomValue = Random.Range(0, 100);

        if (randomValue < damageChance)
        {
            ApplyRandomPartyDamage();
        }
        else
        {
            GenerateItemReward();
        }
        isFind= true;
    }

    /// <summary>
    /// 실제로 존재하는 파티원 중 한 명에게 피해를 준다.
    /// </summary>
    private void ApplyRandomPartyDamage()
    {
        var partySlots = PartyManager.instance.partySlots;

        if (partySlots == null || partySlots.Length == 0)
        {
            Debug.LogWarning("[RuinedBuilding] 파티 슬롯이 없습니다.");
            return;
        }
        DialogueManager.instance.StartDialogue(new string[] { "RuninPartyDamage" });
        var validParty = new List<Unit>();

        foreach (var unit in partySlots)
        {
            if (unit != null)
            {
                validParty.Add(unit);
            }
        }

        if (validParty.Count == 0)
        {
            Debug.LogWarning("[RuinedBuilding] 피해를 받을 파티원이 없습니다.");
            return;
        }

        int index = Random.Range(0, validParty.Count);
        Unit target = validParty[index];

        target.TakeDamage(damageAmount);

        Notion.Instance.NotionUpLocalizationFormat(
            "RuinedDamage",target.name, damageAmount.ToString(),target.health
        );

        Debug.Log(
            $"[RuinedBuilding] 탐색 중 피해 발생: {target.name}, 피해량 {damageAmount}"
        );
    }

    /// <summary>
    /// 무작위 아이템을 선택하고 보상 시스템에 전달한다.
    /// </summary>

    private void GenerateItemReward()
    {
        if (items == null || items.Length == 0)
        {
            Debug.LogWarning("[RuinedBuilding] 탐색 아이템이 설정되지 않았습니다.");
            return;
        }

        if (Reward.Instance == null)
        {
            Debug.LogWarning("[RuinedBuilding] Reward 시스템을 찾을 수 없습니다.");
            return;
        }
        DialogueManager.instance.StartDialogue(new string[] { "FindItem" });

        // 유효한 아이템만 추린다.
        List<ItemData> validItems = new List<ItemData>();

        foreach (ItemData item in items)
        {
            if (item != null)
            {
                validItems.Add(item);
            }
        }

        if (validItems.Count == 0)
        {
            Debug.LogWarning("[RuinedBuilding] 유효한 아이템이 없습니다.");
            return;
        }

        // 등록된 아이템 중 하나를 무작위로 선택한다.
        ItemData randomItem = validItems[Random.Range(0, validItems.Count)];

        // 선택한 아이템 2개를 보상으로 설정한다.
        Reward.Instance.SetRewards(
            0,
            0,
            new List<ItemData> { randomItem },
            new List<int> { 2 }
        );

        Reward.Instance.RewardOpen();

        Debug.Log($"[RuinedBuilding] {randomItem.itemName} x2 발견!");
    }

}
