using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Hornet/ElectricBombing")]
public class ElectricBombing : SkillBase
{
    [Header("Stun Chance")]
    [Range(0, 100)]
    public int stunChance = 40;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 전기 폭격이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        if (Random.Range(0, 100) < stunChance)
        {
            target.isStunned = true;
            Debug.Log($"{target.Unitname} 전기 감전으로 기절!");
        }
    }
}
