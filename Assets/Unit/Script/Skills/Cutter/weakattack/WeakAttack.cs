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

        int damage =
            user.CalculateDamage(
                target,
                skill
            );
        if (target.isBleeding || target.isFires || target.isStunned)
        {
            damage = (int)(damage * 1.5f);
        }
        target.TakeDamage(damage);

        target.isBleeding = true;

        Debug.Log(
            target.name +
            " 출혈!"
        );

        if (target.health <= 0)
        {
            target.Die();
        }
    }
}