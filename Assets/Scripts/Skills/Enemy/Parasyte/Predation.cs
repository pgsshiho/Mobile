using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Parasyte/Predation")]
public class Predation : SkillBase
{
    [Tooltip("산화 대상 타격 시 데미지 배율 (기본 1.5배)")]
    public float oxidationMultiplier = 1.5f;

    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 포식이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);

        // 산화 대상에게 추가 피해
        bool isOxidized = target.isOxidationI || target.isOxidationII ||
                          target.HasStatus(StatusType.OxidationI) || target.HasStatus(StatusType.OxidationII);

        if (isOxidized)
        {
            damage = Mathf.RoundToInt(damage * oxidationMultiplier);
            Debug.Log($"<color=red>[산화 포식]</color> {target.Unitname} 산화 상태로 피해 {oxidationMultiplier}배 증폭! ({damage})");
        }

        target.TakeDamage(damage);
        Debug.Log($"{user.Unitname}의 포식이 {target.Unitname}에게 적중하여 {damage} 피해!");
    }
}
