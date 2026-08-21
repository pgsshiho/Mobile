using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Eagle/AimedShot")]
public class AimedShot : SkillBase
{
    [Header("Charge Damage Multiplier")]
    public float chargeDamageMultiplier = 2.0f;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 조준 사격이 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        damage = Mathf.RoundToInt(damage * chargeDamageMultiplier);

        target.TakeDamage(damage);
        Debug.Log($"{user.Unitname}의 정밀 조준 사격 적중! {target.Unitname}에게 {damage} 치명적 피해!");
    }
}
