using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Apollo/ThreeRoundBurst")]
public class ThreeRoundBurst : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        int totalDamage = 0;
        int hitCount = 0;

        for (int i = 0; i < 3; i++)
        {
            if (user.CheckHit(target, skill))
            {
                int singleDamage = Mathf.Max(1, user.CalculateDamage(target, skill) / 2);
                target.TakeDamage(singleDamage);
                totalDamage += singleDamage;
                hitCount++;
            }
        }

        if (hitCount > 0)
        {
            TryApplyStatus(target, skill);
        }

        Debug.Log($"{user.Unitname} 3점사 사격 완료! {hitCount}발 명중, 총 {totalDamage} 피해!");
    }
}
