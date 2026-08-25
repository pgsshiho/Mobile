using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Apollo/WildFire")]
public class WildFire : SkillBase
{
    [Header("Hit Count")]
    public int hitCount = 3;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        int totalDamage = 0;
        for (int i = 0; i < hitCount; i++)
        {
            if (user.CheckHit(target, skill))
            {
                int singleDamage = Mathf.Max(1, user.CalculateDamage(target, skill) / 2);
                target.TakeDamage(singleDamage);
                totalDamage += singleDamage;
            }
        }

        if (totalDamage > 0)
        {
            TryApplyStatus(target, skill);
        }

        Debug.Log($"{user.Unitname} 난사 완료! {target.Unitname}에게 총 {totalDamage} 피해!");
    }
}
