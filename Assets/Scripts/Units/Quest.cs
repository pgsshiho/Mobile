using UnityEngine;

public class Quest : MonoBehaviour
{
    [Header("퀘스트 정보 (ScriptableObject)")]
    public QuestData data;

    [Header("개별 진행 상황")]
    public int currentCount = 0;
    public bool isCompleted = false;

    private void Start()
    {
        // QuestManager에 자신 등록
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RegisterQuest(this);
        }
    }

    public void CheckKill(GameObject killedEnemy)
    {
        if (isCompleted || data == null || data.needType != QuestNeed.Kill) return;

        if (killedEnemy.CompareTag(data.targetTag))
        {
            currentCount++;
            Debug.Log($"[{data.questTitle}] {data.targetTag} 처치! ({currentCount} / {data.needCount})");
            CheckQuestCompletion();
        }
    }

    public void CheckItemObtained(QuestNeed itemType, int amount = 1)
    {
        if (isCompleted || data == null || data.needType != itemType) return;

        currentCount += amount;
        Debug.Log($"[{data.questTitle}] {itemType} 획득! ({currentCount} / {data.needCount})");
        CheckQuestCompletion();
    }

    private void CheckQuestCompletion()
    {
        if (!isCompleted && currentCount >= data.needCount)
        {
            isCompleted = true;
            Debug.Log($"<color=green>★ [{data.questTitle}] 완료! ★</color>");
            AddParty addParty = GetComponent<AddParty>();
            addParty?.Add();
        }
    }
}