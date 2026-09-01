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

    [Header("Target Range")]
    [Tooltip("파티와 적 사이에서 공격 가능한 최대 열 거리입니다. 기본 공격 범위는 2열입니다.")]
    [Min(1)]
    public int maxTargetDistance = 2;

    [Header("User Position")]
    [Tooltip("이 스킬을 사용할 수 있는 파티 열 범위입니다. 0은 최전열, 3은 최후열입니다.")]
    [Range(0, 3)]
    public int minUserColumn = 0;

    [Range(0, 3)]
    public int maxUserColumn = 3;

    // 위력
    public int power;

    // 명중 보정
    public int hitBonus = 0;

    public AudioClip soundEffect;
    // 실제 행동 로직
    public SkillBase skillLogic;
}
