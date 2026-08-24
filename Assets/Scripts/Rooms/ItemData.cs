using UnityEngine;

public enum ItemEffectType
{
    HealHp,             // 체력 회복 (고철덩어리)
    RecoverEnergy,      // 에너지 회복 (비상배터리)
    RecoverAntenna,     // 통신기 복구 (안테나)
    RecoverFuse,        // 퓨즈 파손 해제 (퓨즈)
    SandpaperOxidation, // 산화 해제 + HP 감소 (사포)
    RemoveOxidation,    // 산화 해제 (녹 제거제)
    CoolDown,           // 과열 해제 (냉각제)
    ExtinguishFire,     // 화재 → 과열 전환 (소화기)
    RemovePollution,    // 무장 오염 해제 (솔)
    RemoveShortCircuit, // 합선 해제 (절연 테이프)
    RemoveOilLeak       // 윤활유 누유 해제 (방수 테이프)
}

[CreateAssetMenu(menuName = "OverCharge/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;
    public string description;
    public Sprite icon;

    [Header("Effect")]
    public ItemEffectType effectType;

    [Header("Heal Settings")]
    public int healAmount = 20;

    [Header("Energy Settings")]
    public int energyAmount = 30;

    [Header("Sandpaper HP Penalty")]
    public int sandpaperHpPenalty = 2;

    [Header("Use Limit (0 = 무제한)")]
    public int maxUseCount = 0;
}
