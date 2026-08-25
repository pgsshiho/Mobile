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

    public AudioClip soundEffect;

    [Header("상태이상")]
    [Tooltip("이 스킬이 부여하는 상태이상 종류 (None이면 부여 없음)")]
    public StatusType statusEffect = StatusType.None;

    [Tooltip("상태이상 지속 턴 수 (0이면 상태이상 없음, -1이면 무한)")]
    public int statusTurns = 0;

    [Tooltip("상태이상 부여 확률 (0~100%)")]
    [Range(0, 100)]
    public int statusChance = 100;

    // 실제 행동 로직
    public SkillBase skillLogic;
}