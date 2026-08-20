using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Attack")]
public class Attack : SkillBase
{
    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        int finalHit =
            user.accuracy +
            skill.hitBonus;

        finalHit =
            Mathf.Clamp(finalHit, 0, 100);

        int roll =
            Random.Range(0, 100);

        if (roll >= finalHit)
        {
            Debug.Log("빗나감!");
            return;
        }

        int damage =
            Mathf.Max(
                1,
                user.attackPower +
                skill.power -
                target.defensePower
            );

        target.health -= damage;

        Debug.Log("공격!");
    }
}