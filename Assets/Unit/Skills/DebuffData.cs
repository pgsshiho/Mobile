using UnityEngine;

[CreateAssetMenu(menuName = "Debuff")]
public class DebuffData : ScriptableObject
{
    public string debuffName;

    public int duration;

    public int attackPenalty;
    public int defensePenalty;

    public int hitPenalty;

    public float critPenalty;
}