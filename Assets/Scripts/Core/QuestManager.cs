using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private List<Quest> activeQuests = new List<Quest>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
        }
    }

    // 적이 죽었을 때 호출 -> 진행중인 모든 퀘스트에 처치 이벤트 전달
    public void NotifyKill(GameObject killedEnemy)
    {
        foreach (var quest in activeQuests)
        {
            quest.CheckKill(killedEnemy);
        }
    }

    // 아이템을 주웠을 때 호출 -> 진행중인 모든 퀘스트에 아이템 획득 이벤트 전달
    public void NotifyItemObtained(QuestNeed itemType, int amount = 1)
    {
        foreach (var quest in activeQuests)
        {
            quest.CheckItemObtained(itemType, amount);
        }
    }
}