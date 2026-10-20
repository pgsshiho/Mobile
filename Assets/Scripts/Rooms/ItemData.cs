using UnityEngine;

public enum ItemEffectType
{
    HealHp,
    RecoverEnergy,
    RecoverAntenna,
    RecoverFuse,
    SandpaperOxidation,
    RemoveOxidation,
    CoolDown,
    ExtinguishFire,
    RemovePollution,
    RemoveShortCircuit,
    RemoveOilLeak
}

/// <summary>아이템의 변하지 않는 정의 데이터입니다. 보유 수량은 ItemRuntime에서 관리합니다.</summary>
[CreateAssetMenu(menuName = "OverCharge/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("소지 및 사용 규칙")]
    [Min(1)] public int maxStackCount = 10;
    [Tooltip("전투 중 사용 가능한지 여부입니다.")]
    public bool usableInBattle = true;

    [Header("효과")]
    public ItemEffectType effectType;
    public int healAmount = 20;
    public int energyAmount = 30;
    public int communicationAmount = 50;
    public int sandpaperHpPenalty = 2;
    [Tooltip("0은 사용 횟수 제한 없음입니다.")]
    public int maxUseCount = 0;

    // 기존 에셋과의 직렬화 호환용 필드입니다. 실제 수량은 ItemRuntime에만 저장합니다.
    [HideInInspector] public int currentCount;
}
