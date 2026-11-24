using UnityEngine;

[CreateAssetMenu(menuName = "Debuff")]
public class DebuffData : ScriptableObject
{
    public string debuffName;

    public int duration;

    public int attackPenalty;
    public int defensePenalty;

    public int hitPenalty;

    public int speedPenalty;

    public float critPenalty;

    [Header("회피율 패널티")]
    [Tooltip("고정 회피율 감소 (예: 0.05 = 5% 감소)")]
    public float dodgePenalty = 0f;

    [Tooltip("회피율 배율 패널티 (예: 0.8 = 현재 회피율의 20% 감소 / 1.0 = 변화 없음)")]
    public float dodgeMultiplierPenalty = 1f;
}
