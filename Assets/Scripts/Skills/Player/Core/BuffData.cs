using UnityEngine;

[CreateAssetMenu(menuName = "Buff")]
public class BuffData : ScriptableObject
{
    public string buffName;

    public int duration;

    public int attackBonus;
    public float defenseBonus;
    public int speedBonus;

    public int hitBonus;

    public float critBonus;

    public float damageMultiplier = 1f;

    public float healMultiplier = 1f;

    [Header("회피율")]
    [Tooltip("고정 회피율 추가 (예: 0.05 = 5% 추가)")]
    public float dodgeBonus = 0f;

    [Tooltip("회피율 배율 (예: 1.2 = 현재 회피율의 20% 증가 / 1.0 = 변화 없음)")]
    public float dodgeMultiplier = 1f;
}
