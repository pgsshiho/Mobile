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

        // 시작 시 기존 인벤토리 보유 상황 검사 (소급 적용)
        RefreshProgress();
    }

    /// <summary>
    /// 새로운 퀘스트 데이터를 설정하고 진행 상황을 초기화하여 퀘스트를 수주/시작합니다.
    /// (수주 전 이미 보유하고 있던 아이템도 소급 적용되어 즉시 완료 가능)
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

        // 수주 즉시 기존 보유 아이템 확인 및 완료 검사 (소급 적용)
        RefreshProgress();
    }

    /// <summary>
    /// 현재 인벤토리 보유 상황을 조회하여 퀘스트 진행도 갱신 및 즉시 완료 검사를 수행합니다.
    /// (퀘스트 받기 전 이미 획득한 아이템이 있거나 대화 시 즉시 소급 완료 처리)
    /// </summary>
    public void RefreshProgress()
    {
        if (isCompleted || data == null) return;

        // 아이템 수집 퀘스트인 경우 인벤토리 내 현재 보유 총량을 소급 적용
        if (data.needType != QuestNeed.Kill && ItemManager.Instance != null)
        {
            int owned = ItemManager.Instance.GetItemCountByQuestNeed(data.needType);
            currentCount = Mathf.Max(currentCount, owned);
            Debug.Log($"[{data.questTitle}] 보유 아이템 확인: {data.needType} ({currentCount} / {data.needCount})");
        }

        CheckQuestCompletion();
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
