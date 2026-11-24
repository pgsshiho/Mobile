using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/NavyCaptain/NavyTargetMark")]
public class NavyTargetMark : SkillBase
{
    [Tooltip("표식 부여 확률 (기본 100%)")]
    [Range(0, 100)]
    public int markChance = 100;

    [Tooltip("표식 지속 턴")]
    public int markTurns = 3;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 표식 상태이상 부여 (specialArmor 감쇄 적용)
        bool inflicted = user.InflictStatusWithChance(target, StatusType.Marked, markChance, markTurns, 0);

        if (user is NavyCaptainUnit captain)
        {
            captain.designatedTarget = target;
        }

        Debug.Log($"<color=red>[대상 지정]</color> {user.Unitname}이(가) {target.Unitname}을 집중 포격 및 공격 목표로 지정했습니다! (지속: {markTurns}턴)");
    }
}
