using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/FrontWait")]
public class FrontWait : SkillBase
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
        target.TakeDamage(damage);
        target.AddStatus(StatusType.Bleeding, 3);

        Debug.Log(target.name + " 출혈 부여!");
    }
}