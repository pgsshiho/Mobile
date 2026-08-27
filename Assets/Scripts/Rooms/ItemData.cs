using UnityEngine;

public enum ItemEffectType
{
    HealHp,             // 체력 회복 (고철덩어리 - 연료 소모)
    RecoverEnergy,      // 전력/에너지 회복 & 연료고갈 복구 (비상배터리)
    RecoverAntenna,     // 통신도/스트레스 복구 (안테나)
    RecoverFuse,        // 과전력/영구기절/퓨즈 복구 (퓨즈)
    SandpaperOxidation, // 녹(산화) 복구 및 체력 2 감소 (사포)
    RemoveOxidation,    // 녹(산화) 복구 (녹 제거제)
    CoolDown,           // 과열 복구 (냉각제)
    ExtinguishFire,     // 화재 → 과열 전환 (소화기)
    RemovePollution,    // 오염 복구 (솔)
    RemoveShortCircuit, // 합선 복구 (절연 테이프)
    RemoveOilLeak       // 누유 복구 (방수 테이프)
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

    [Header("Heal Settings (고철덩어리)")]
    public int healAmount = 20;

    [Header("Energy Settings (비상배터리)")]
    public int energyAmount = 30;

    [Header("Communication Settings (안테나)")]
    public int communicationAmount = 50;

    [Header("Sandpaper HP Penalty (사포)")]
    public int sandpaperHpPenalty = 2;

    [Header("Use Limit (0 = 무제한, 고철덩어리 연료고갈용)")]
    public int maxUseCount = 0;

    [Header("Stack Limit")]
    public int maxStackCount = 10;
}
