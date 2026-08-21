using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Divide")]
public class Divide : SkillBase
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

        // 데미지 계산
        int damage =
            user.CalculateDamage(
                target,
                skill
            );

        target.TakeDamage(damage);

        Debug.Log(
            user.name +
            " → " +
            target.name +
            " 절단 " +
            damage
        );

        // 사망 체크
        if (target.health <= 0)
        {
            target.Die();
        }
    }
}