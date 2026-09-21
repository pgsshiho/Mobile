using UnityEngine;
using UnityEngine.Events;

public class Quest : MonoBehaviour
{
    [Header("퀘스트 정보 (ScriptableObject)")]
    public QuestData data;

    [Header("개별 진행 상황")]
    public int currentCount = 0;
    public bool isCompleted = false;

    [Header("퀘스트 완료 이벤트")]
    public UnityEvent onQuestCompleted;

    private void Start()
    {
        // QuestManager에 자신 등록
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RegisterQuest(this);
        }
    }

    /// <summary>
    /// 새로운 퀘스트 데이터를 설정하고 진행 상황을 초기화하여 퀘스트를 수주/시작합니다.
    /// </summary>
    public void GiveQuest(QuestData newQuestData = null)
    {
        if (newQuestData != null)
        {
            data = newQuestData;
        }

        currentCount = 0;
        isCompleted = false;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RegisterQuest(this);
        }

        Debug.Log($"<color=yellow>[Quest]</color> 퀘스트 수주: '{data?.questTitle}' (목표: {data?.needType} x{data?.needCount})");
    }

    public void CheckKill(GameObject killedEnemy)
    {
        if (isCompleted || data == null || data.needType != QuestNeed.Kill) return;

        if (killedEnemy != null && (string.IsNullOrEmpty(data.targetTag) || killedEnemy.CompareTag(data.targetTag)))
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

    public void CheckQuestCompletion()
    {
        if (!isCompleted && data != null && currentCount >= data.needCount)
        {
            isCompleted = true;
            Debug.Log($"<color=green>★ [{data.questTitle}] 퀘스트 완료! ★</color>");

            // 1. 씬 내 모든 FactoryAddUnit의 CanClick을 true로 활성화 (로봇 선택 가능 상태로 전환)
            FactoryAddUnit.UnlockAll();

            // 2. AddParty 하위 호환
            AddParty addParty = FindAnyObjectByType<AddParty>();
            addParty?.Add();

            // 3. 커스텀 완료 이벤트 실행
            onQuestCompleted?.Invoke();
        }
    }
}