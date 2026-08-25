using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/WeakAttack")]
public class WeakAttack : SkillBase
{
    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        // 명중 체크
        if (!user.CheckHit(target, skill))
        {
            Debug.Log("빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);

        // 출혈 / 화재 / 기절 상태이상 보너스 (1.5배 피해)
        if (target.isBleeding || target.isFire || target.isStunned)
        {
            damage = (int)(damage * 1.5f);
        }

        target.TakeDamage(damage);

        target.AddStatus(StatusType.Bleeding, 3);
        TryApplyStatus(target, skill);

        Debug.Log(target.name + " 출혈!");
    }
}