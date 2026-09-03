using UnityEngine;

// 1. 퀘스트 요구 대상 종류 (아이템 or 적 처치)
public enum QuestNeed
{
    [InspectorName("고철덩어리")] ScrapMetal,
    [InspectorName("비상배터리")] EmergencyBattery,
    [InspectorName("안테나")] Antenna,
    [InspectorName("퓨즈")] Fuse,
    [InspectorName("사포")] Sandpaper,
    [InspectorName("녹 제거제")] RustRemover,
    [InspectorName("냉각제")] Coolant,
    [InspectorName("소화기")] FireExtinguisher,
    [InspectorName("솔")] Brush,
    [InspectorName("절연 테이프")] ElectricalTape,
    [InspectorName("방수 테이프")] WaterproofTape,
    [InspectorName("적 처치")] Kill
}

[CreateAssetMenu(fileName = "New Quest Data", menuName = "Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("퀘스트 기본 정보")]
    public string questTitle;
    [TextArea] public string questDescription;

    [Header("퀘스트 목표 유형 및 수량")]
    public QuestNeed needType;  // 드롭다운으로 아이템 또는 Kill 선택
    public int needCount;       // 목표 개수 / 처치 수

    [Header("적 처치 퀘스트 전용 설정")]
    [Tooltip("needType이 'Kill'일 때만 사용됩니다.")]
    public string targetTag = "Enemy";
}