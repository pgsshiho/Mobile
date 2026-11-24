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
    [Tooltip("파티와 적 사이에서 공격 가능한 최대 열 거리입니다. 기본 공격 범위는 2열입니다. (allowedTargetColumns가 비어있을 때 사용)")]
    [Min(1)]
    public int maxTargetDistance = 2;

    [Header("User Position")]
    [Tooltip(
        "이 스킬을 사용 가능한 파티 열 번호 목록입니다 (1열=0, 2열=1, 3열=2, 4열=3).\n" +
        "비워두면 모든 열에서 사용 가능합니다.\n" +
        "예) {0,3} → 1열·4열에서만 사용 가능, {2,3} → 3·4열에서만 사용 가능.")]
    public int[] allowedUserColumns = new int[0];

    [Header("Target Position")]
    [Tooltip(
        "이 스킬로 타겟팅(공격) 가능한 상대 열 번호 목록입니다 (1열=0, 2열=1, 3열=2, 4열=3).\n" +
        "비워두면 기존 maxTargetDistance(최대 열 거리) 규칙을 사용합니다.\n" +
        "예) {2,3} → 상대 3·4열만 타격 가능, {0,3} → 상대 1·4열만 타격 가능.")]
    public int[] allowedTargetColumns = new int[0];

    // 위력
    public int power;

    // 명중 보정
    public int hitBonus = 0;

    public AudioClip soundEffect;
    // 실제 행동 로직
    public SkillBase skillLogic;

    /// <summary>
    /// 지정한 시전자 열(0-based)에서 이 스킬을 사용할 수 있는지 반환합니다.
    /// allowedUserColumns가 비어 있으면 모든 열 허용.
    /// </summary>
    public bool IsAllowedAtColumn(int column)
    {
        if (allowedUserColumns == null || allowedUserColumns.Length == 0)
            return true;

        foreach (int allowed in allowedUserColumns)
        {
            if (allowed == column)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 지정한 대상의 열(0-based: 1열=0, 2열=1, 3열=2, 4열=3)을 타격 가능한지 반환합니다.
    /// allowedTargetColumns가 비어 있으면 true (기존 maxTargetDistance 규칙으로 위임).
    /// </summary>
    public bool IsAllowedAtTargetColumn(int targetColumn)
    {
        if (allowedTargetColumns == null || allowedTargetColumns.Length == 0)
            return true;

        foreach (int allowed in allowedTargetColumns)
        {
            if (allowed == targetColumn)
                return true;
        }

        return false;
    }
}
