using UnityEngine;

public enum TargetType
{
    SingleEnemy,
    TwoEnemy,
    ThreeEnemy,
    AllEnemy,
    Self,
    Ally,
    AllAlly,
    DeadAlly
}

[CreateAssetMenu(menuName = "Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;

    public Sprite icon;

    [TextArea]
    public string description;

    public TargetType targetType;

    // 위력
    public int power;

    // 명중 보정
    public int hitBonus = 0;

    // 실제 행동 로직
    public SkillBase skillLogic;
}