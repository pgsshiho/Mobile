using UnityEngine;

[CreateAssetMenu(menuName = "Buff")]
public class BuffData : ScriptableObject
{
    public string buffName;

    public int duration;

    public int attackBonus;
    public int defenseBonus;

    public int speedBonus;

    public int hitBonus;

    public float critBonus;

    public float damageMultiplier = 1f;

    public float healMultiplier = 1f;
}