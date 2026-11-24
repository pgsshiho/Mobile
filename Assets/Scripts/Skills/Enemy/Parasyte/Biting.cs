using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Parasyte/Biting")]
public class Biting : SkillBase
{
    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 포식이 {target.Unitname}에게 빗나감!");
            return;
        }
        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
    }
}